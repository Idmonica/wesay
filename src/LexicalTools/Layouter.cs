using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.i8n;
using WeSay.Data;
using WeSay.Foundation;
using WeSay.Foundation.Options;
using WeSay.Language;
using WeSay.LexicalModel;
using WeSay.Project;
using WeSay.UI;

namespace WeSay.LexicalTools
{
	/// <summary>
	/// A Layouter is responsible for filling a detailed list with the contents
	/// of a single data object (e.g. LexSense, LexExample), etc.
	/// There are will normally be a single subclass per class of data,
	/// and each of these layout erstwhile call a different layouter for each
	/// child object (e.g. LexEntryLayouter would employ a SenseLayouter to display senses).
	/// </summary>
	public abstract class Layouter
	{
		/// <summary>
		/// The DetailList we are filling.
		/// </summary>
		private DetailList _detailList;

		/// <summary>
		/// Use for establishing relations been this entry and the rest
		/// </summary>
		private readonly LexEntryRepository _recordListManager;

		private readonly ViewTemplate _viewTemplate;

		/// This field is for temporarily storing a ghost field about to become "real".
		/// This is critical, though messy, because
		/// otherwise, the first character typed into a ghost text field is not considered by
		/// Keyman, since switching focus to a new, "real" box clears Keymans history of your typing.
		/// See WS-23 for more info.
		private MultiTextControl _previouslyGhostedControlToReuse = null;

		private bool _showNormallyHiddenFields = false;

		protected DetailList DetailList
		{
			get { return _detailList; }
			set { _detailList = value; }
		}

		public ViewTemplate ActiveViewTemplate
		{
			get { return _viewTemplate; }
		}

		/// <summary>
		/// Use for establishing relations been this entry and the rest
		/// </summary>
		protected LexEntryRepository RecordListManager
		{
			get { return this._recordListManager; }
		}

		public bool ShowNormallyHiddenFields
		{
			get { return _showNormallyHiddenFields; }
			set { _showNormallyHiddenFields = value; }
		}

		protected Layouter(DetailList builder, ViewTemplate viewTemplate, LexEntryRepository recordListManager)
		{
			if (builder == null)
			{
				throw new ArgumentNullException("builder");
			}
			if (viewTemplate == null)
			{
				throw new ArgumentNullException("viewTemplate");
			}
			_detailList = builder;
			_viewTemplate = viewTemplate;
			_recordListManager = recordListManager;
		}

		/// <summary>
		/// actually add the widgets that are needed to the detailed list
		/// </summary>
		public int AddWidgets(IBindingList list, int index)
		{
			return AddWidgets(list, index, -1);
		}

		internal abstract int AddWidgets(IBindingList list, int index, int row);

		protected Control MakeBoundControl(MultiText multiTextToBindTo, Field field)
		{
			MultiTextControl m;
			if (_previouslyGhostedControlToReuse == null)
			{
				m = new MultiTextControl(field.WritingSystems,
										 multiTextToBindTo,
										 field.FieldName,
										 field.Visibility != CommonEnumerations.VisibilitySetting.ReadOnly,
										 //show annotation
										 BasilProject.Project.WritingSystems,
										 field.Visibility);
			}
			else
			{
				m = _previouslyGhostedControlToReuse;
				_previouslyGhostedControlToReuse = null;
			}

			BindMultiTextControlToField(m, multiTextToBindTo);
			return m;
		}

		private void BindMultiTextControlToField(MultiTextControl control, MultiText multiTextToBindTo)
		{
			foreach (WeSayTextBox box in control.TextBoxes)
			{
				TextBinding binding = new TextBinding(multiTextToBindTo, box.WritingSystem.Id, box);
				binding.ChangeOfWhichItemIsInFocus += this._detailList.OnBinding_ChangeOfWhichItemIsInFocus;
			}
		}

//        protected Control MakeGhostEntry(IBindingList list, string ghostPropertyName, IList<String> writingSystemIds)
//        {
//            WeSayMultiText m = new WeSayMultiText(writingSystemIds, new MultiText());
////
////            foreach (WeSayTextBox box in m.TextBoxes)
////            {
////                MakeGhostBinding(list, ghostPropertyName, box.WritingSystem, box);
////            }
////            GhostBinding g = MakeGhostBinding(list, "Sentence", writingSystem, entry);
////            //!!!!!!!!!!!!!!!!! g.ReferenceControl =
////                DetailList.AddWidgetRow(StringCatalog.GetListOfType("New Example"), false, entry, insertAtRow+rowCount);
////
//////            WeSayTextBox entry = new WeSayTextBox(writingSystem);
////            MakeGhostBinding(list, ghostPropertyName, writingSystem, entry);
//            return m;
//        }

		protected int MakeGhostWidget<T>(IBindingList list, int insertAtRow, string fieldName, string label,
										 string propertyName, bool isHeading) where T : new()
		{
			int rowCount = 0;
			Field field = ActiveViewTemplate.GetField(fieldName);
			if (field != null && field.Enabled && field.Visibility == CommonEnumerations.VisibilitySetting.Visible)
			{
				MultiTextControl m =
						new MultiTextControl(field.WritingSystems,
											 new MultiText(),
											 fieldName + "_ghost",
											 false,
											 BasilProject.Project.WritingSystems,
											 field.Visibility);

				Control refWidget = DetailList.AddWidgetRow(label, isHeading, m, insertAtRow + rowCount, true);

				foreach (WeSayTextBox box in m.TextBoxes)
				{
					GhostBinding<T> g = MakeGhostBinding<T>(list, propertyName, box.WritingSystem, box);
					g.ReferenceControl = refWidget;
				}
				return 1;
			}
			else
			{
				return 0; //didn't add a row
			}
		}

		protected GhostBinding<T> MakeGhostBinding<T>(IBindingList list, string ghostPropertyName,
													  WritingSystem writingSystem,
													  WeSayTextBox entry) where T : new()
		{
			GhostBinding<T> binding = new GhostBinding<T>(list, ghostPropertyName, writingSystem, entry);
			binding.LayoutNeededAfterMadeReal += OnGhostBindingLayoutNeeded;
			binding.CurrentItemChanged += this._detailList.OnBinding_ChangeOfWhichItemIsInFocus;
			return binding;
		}

		protected virtual void OnGhostBindingLayoutNeeded<T>(GhostBinding<T> sender, IBindingList list, int index,
															 MultiTextControl previouslyGhostedControlToReuse,
															 bool doGoToNextField, EventArgs args) where T : new()
		{
			_previouslyGhostedControlToReuse = previouslyGhostedControlToReuse;
			AddWidgetsAfterGhostTrigger(list, index, sender.ReferenceControl, doGoToNextField);
		}

		protected void AddWidgetsAfterGhostTrigger(IBindingList list, int indexOfNewGuyInList, Control refControl,
												   bool doGoToNextField)
		{
			int ghostRow = _detailList.GetRow(refControl);
			this.UpdateGhostLabel(list, ghostRow);
			AddWidgets(list, indexOfNewGuyInList, ghostRow);
		   Application.DoEvents();
			if (doGoToNextField)
			{
				_detailList.MoveInsertionPoint(ghostRow + 1);
			}
			else
			{
				_detailList.MoveInsertionPoint(ghostRow);
			}
		}

		protected virtual void UpdateGhostLabel(IBindingList list, int index)
		{
		}

		protected static int AddChildrenWidgets(Layouter layouter, IBindingList list, int insertAtRow, int rowCount)
		{
			for (int i = 0; i < list.Count; i++)
			{
				int r;
				if (insertAtRow < 0)
				{
					r = insertAtRow; // just stick at the end
				}
				else
				{
					r = insertAtRow + rowCount;
				}
				rowCount += layouter.AddWidgets(list, i, r);
			}
			return rowCount;
		}

		protected int AddCustomFields(WeSayDataObject target, int insertAtRow)
		{
			int rowCount = 0;
			foreach (Field customField in ActiveViewTemplate.GetCustomFields(target.GetType().Name))
			{
#if GlossMeaning
#else
				if (customField.FieldName == LexSense.WellKnownProperties.Definition)
				{
					continue; //already put this in next to "Meaning"
				}
#endif
				rowCount = AddOneCustomField(target, customField, insertAtRow + rowCount/*changed feb 2008*/, rowCount);
			}


			//grab any basetype class (to just one level). E.g., 'Note'
			foreach (Field customField in ActiveViewTemplate.GetCustomFields(target.GetType().BaseType.Name))
			{
				if (target.GetType() == typeof(LexExampleSentence) && customField.FieldName == WeSayDataObject.WellKnownProperties.Note)
				{
					continue; //note actually isn't allowed at the moment
				}
				rowCount = AddOneCustomField(target, customField, insertAtRow + rowCount/*changed feb 2008*/, rowCount);
			}
			return rowCount;
		}

		private int AddOneCustomField(WeSayDataObject target, Field customField, int insertAtRow, int rowCount)
		{
			IReportEmptiness data= target.GetProperty<IReportEmptiness>(customField.FieldName);
			if (!customField.GetDoShow(data, this.ShowNormallyHiddenFields))
			{
				return rowCount;
			}
			Control box;
			switch (customField.DataTypeName)
			{
				case "Picture":
					box = MakePictureWidget(target, customField);
					break;
				case "Flag":
					box = MakeCheckBoxWidget(target, customField);
					break;
				case "Option":
					box = MakeOptionWidget(target, customField);
					break;
				case "OptionCollection":
					box = MakeOptionCollectionWidget(target, customField);
					break;
				case "MultiText":
					box =
						MakeBoundControl(target.GetOrCreateProperty<MultiText>(customField.FieldName),
										 customField);
					break;
				default:
					LexRelationType lexRelType = GetRelationType(customField.DataTypeName);
					if (lexRelType != null)
					{
						box = MakeRelationWidget(target, lexRelType, customField);
					}
					else
					{
						throw new ApplicationException(
							string.Format("WeSay doesn't understand how to layout a {0}",
										  customField.DataTypeName));
					}
					break;
			}

			string label = StringCatalog.Get(customField.DisplayName);

			//for checkboxes, the label is part of the control
			if (customField.DataTypeName == "Flag")
			{
				label = string.Empty;
			}

			Control c =DetailList.AddWidgetRow(StringCatalog.Get(label),
											   false,
											   box,
											   insertAtRow,
											   false);

			insertAtRow = DetailList.GetRow(c);
			++rowCount;
			return rowCount;
		}


		static private LexRelationType GetRelationType(string dataTypeName)
		{
			foreach (LexRelationType type in WeSayWordsProject.Project.RelationTypes)
			{
				if (type.ID == dataTypeName)
				{
					return type;
				}
			}
			return null;
		}

		/// <summary>
		/// This is used to convert from the IEnuerable<string> that the cache give us
		/// to the IEnumerable<object> that AutoComplete needs.
		/// </summary>
//        public class LexEntryEnumerableToObjectEnumerableWrapper : IEnumerable<object>
//        {
//            private readonly IEnumerable<LexEntry> _collection;
//
//            public LexEntryEnumerableToObjectEnumerableWrapper(IEnumerable<LexEntry> collection)
//            {
//                if (collection == null)
//                {
//                    throw new ArgumentNullException("collection");
//                }
//                _collection = collection;
//            }
//
//            IEnumerator<object> IEnumerable<object>.GetEnumerator()
//            {
//                foreach (object s in _collection)
//                {
//                    yield return s;
//                }
//            }
//
//            public IEnumerator GetEnumerator()
//            {
//                return ((IEnumerable<object>)this).GetEnumerator();
//            }
//
//        }
		private Control MakeRelationWidget(WeSayDataObject target, LexRelationType type, Field field)
		{
			return
					RelationController.CreateWidget(target,
													type,
													field,
													_recordListManager,
													_detailList.OnBinding_ChangeOfWhichItemIsInFocus);
		}

//        void OnSelectedItemChanged(object sender, EventArgs e)
//        {
//            WeSayAutoCompleteTextBox box = sender as WeSayAutoCompleteTextBox;
//            LexRelation relation = (LexRelation)box.Tag;
//            Palaso.Reporting.Logger.WriteMinorEvent("Changing value of LexRelation Control ({0})", relation.TypeId);
//
//            if (box.SelectedItem == null)
//            {
//                relation.Key = string.Empty;
//            }
//            else
//            {
//                relation.Target = box.SelectedItem;
//            }
//        }

		protected Control MakeOptionWidget(WeSayDataObject target, Field field)
		{
			OptionRef optionRefTarget = target.GetOrCreateProperty<OptionRef>(field.FieldName);

			OptionsList list = WeSayWordsProject.Project.GetOptionsList(field, false);
			SingleOptionControl control =
					new SingleOptionControl(optionRefTarget, list, field.WritingSystemIds[0], field.FieldName);
			SimpleBinding<string> binding = new SimpleBinding<string>(optionRefTarget, control);
			binding.CurrentItemChanged +=this._detailList.OnBinding_ChangeOfWhichItemIsInFocus;
			return control;
		}


		private static Control MakeOptionCollectionWidget(WeSayDataObject target, Field field)
		{
			OptionsList availableOptions = WeSayWordsProject.Project.GetOptionsList(field, false);
			OptionRefCollection refsOfChoices = target.GetOrCreateProperty<OptionRefCollection>(field.FieldName);
//            OptionCollectionControl control =
//                   new OptionCollectionControl(refsOfChoices, availableOptions, field.WritingSystemIds[0]);


			ReferenceCollectionEditor<Option, string, OptionRef> control = new ReferenceCollectionEditor<Option, string, OptionRef>(
				refsOfChoices.Members,
				availableOptions.Options,
				field.WritingSystems,
				field.Visibility,
				new OptionDisplayAdaptor(availableOptions, field.WritingSystemIds[0]));
			control.AlternateEmptinessHelper = refsOfChoices;
			return control;
		}

		protected Control MakeCheckBoxWidget(WeSayDataObject target, Field field)
		{
			FlagState boxState = target.GetOrCreateProperty<FlagState>(field.FieldName);

			CheckBoxControl control = new CheckBoxControl(boxState.Value, field.DisplayName, field.FieldName);
			SimpleBinding<bool> binding = new SimpleBinding<bool>(boxState, control);
			binding.CurrentItemChanged += this._detailList.OnBinding_ChangeOfWhichItemIsInFocus;
			return control;
		}

		private Control MakePictureWidget(WeSayDataObject target, Field field)
		{
			PictureRef pictureRef = target.GetOrCreateProperty<PictureRef>(field.FieldName);

			PictureControl control = new PictureControl(field.FieldName, WeSayWordsProject.Project.PathToPictures);
			if (!String.IsNullOrEmpty(pictureRef.Value))
			{
				control.Value = pictureRef.Value;
			}
			SimpleBinding<string> binding = new SimpleBinding<string>(pictureRef, control);
			binding.CurrentItemChanged += this._detailList.OnBinding_ChangeOfWhichItemIsInFocus;
			return control;
		}
	}
}
