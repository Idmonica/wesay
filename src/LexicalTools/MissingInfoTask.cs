using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.i8n;
using WeSay.Data;
using WeSay.Foundation;
using WeSay.LexicalModel;
using WeSay.Project;

namespace WeSay.LexicalTools
{
	public class MissingInfoTask: TaskBase
	{
		private MissingInfoControl _missingInfoControl;
		private readonly IFieldQuery<LexEntry> _filter;
		private readonly ViewTemplate _viewTemplate;
		private bool _dataHasBeenRetrieved;
		private readonly bool _isBaseFormFillingTask;
		private readonly WritingSystem _writingSystem;

		public MissingInfoTask(LexEntryRepository lexEntryRepository,
							   IFieldQuery<LexEntry> filter,
							   string label,
							   string description,
							   ViewTemplate viewTemplate)
				: base(label, description, false, lexEntryRepository)
		{
			if (filter == null)
			{
				throw new ArgumentNullException("filter");
			}
			if (viewTemplate == null)
			{
				throw new ArgumentNullException("viewTemplate");
			}

			_filter = filter;
			_viewTemplate = viewTemplate;
			_writingSystem = BasilProject.Project.WritingSystems.UnknownVernacularWritingSystem;
			// use the master view Template instead of the one for this task. (most likely the one for this
			// task doesn't have the EntryLexicalForm field specified but the Master (Default) one will
			Field field =
					WeSayWordsProject.Project.DefaultViewTemplate.GetField(
							Field.FieldNames.EntryLexicalForm.ToString());
			if (field != null)
			{
				if (field.WritingSystems.Count > 0)
				{
					_writingSystem = field.WritingSystems[0];
				}
				else
				{
					MessageBox.Show(
							String.Format(
									"There are no writing systems enabled for the Field '{0}'",
									field.FieldName),
							"Error",
							MessageBoxButtons.OK,
							MessageBoxIcon.Exclamation); //review
				}
			}
		}

		/// <summary>
		/// Creates a generic Lexical Field editing task
		/// </summary>
		/// <param name="lexEntryRepository">The lexEntryRepository that will provide the data</param>
		/// <param name="filter">The filter that should be used to filter the data</param>
		/// <param name="label">The task label</param>
		/// <param name="description">The task description</param>
		/// <param name="viewTemplate">The base viewTemplate</param>
		/// <param name="fieldsToShow">The fields to show from the base Field Inventory</param>
		public MissingInfoTask(LexEntryRepository lexEntryRepository,
							   IFieldQuery<LexEntry> filter,
							   string label,
							   string description,
							   ViewTemplate viewTemplate,
							   string fieldsToShow)
				: this(lexEntryRepository, filter, label, description, viewTemplate)
		{
			if (fieldsToShow == null)
			{
				throw new ArgumentNullException("fieldsToShow");
			}
			_viewTemplate = CreateViewTemplateFromListOfFields(viewTemplate, fieldsToShow);

			//hack until we overhaul how Tasks are setup:
			_isBaseFormFillingTask = filter is MissingFieldQuery &&
									 fieldsToShow.Contains(LexEntry.WellKnownProperties.BaseForm);
			if (_isBaseFormFillingTask)
			{
				Field flagField = new Field();
				flagField.DisplayName =
						StringCatalog.Get("~This word has no Base Form",
										  "The user will click this to say that this word has no baseform.  E.g. Kindess has Kind as a baseform, but Kind has no other word as a baseform.");
				flagField.DataTypeName = "Flag";
				flagField.ClassName = "LexEntry";
				flagField.FieldName = "flag_skip_" + ((MissingFieldQuery) filter).FieldName;
				flagField.Enabled = true;
				_viewTemplate.Add(flagField);
			}
		}

		public override DashboardGroup Group
		{
			get
			{
				if (_isBaseFormFillingTask)
				{
					return DashboardGroup.Refine;
				}
				return base.Group;
			}
		}

		public MissingInfoTask(LexEntryRepository lexEntryRepository,
							   IFieldQuery<LexEntry> filter,
							   string label,
							   string description,
							   ViewTemplate viewTemplate,
							   string fieldsToShowEditable,
							   string fieldsToShowReadOnly)
				: this(
						lexEntryRepository,
						filter,
						label,
						description,
						viewTemplate,
						fieldsToShowEditable + " " + fieldsToShowReadOnly)
		{
			MarkReadOnlyFIelds(fieldsToShowReadOnly);
		}

		private void MarkReadOnlyFIelds(string fieldsToShowReadOnly)
		{
			string[] readOnlyFields = SplitUpFieldNames(fieldsToShowReadOnly);

			for (int i = 0;i < _viewTemplate.Count;i++)
			{
				Field field = _viewTemplate[i];
				foreach (string s in readOnlyFields)
				{
					if (s == field.FieldName)
					{
						Field readOnlyVersion = new Field(field);
						readOnlyVersion.Visibility = CommonEnumerations.VisibilitySetting.ReadOnly;
						_viewTemplate.Remove(field);
						_viewTemplate.Insert(i, readOnlyVersion);
					}
				}
			}
		}

		private static ViewTemplate CreateViewTemplateFromListOfFields(IEnumerable<Field> fieldList,
																	   string fieldsToShow)
		{
			string[] fields = SplitUpFieldNames(fieldsToShow);
			ViewTemplate viewTemplate = new ViewTemplate();
			foreach (Field field in fieldList)
			{
				if (Array.IndexOf(fields, field.FieldName) >= 0)
				{
					if (field.Enabled == false)
							//make sure specified fields are shown (greg's ws-356)
					{
						Field enabledField = new Field(field);
						enabledField.Enabled = true;
						viewTemplate.Add(enabledField);
					}
					else
					{
						if (field.Visibility != CommonEnumerations.VisibilitySetting.Visible)
								//make sure specified fields are visible (not in 'rare mode)
						{
							Field visibleField = new Field(field);
							visibleField.Visibility = CommonEnumerations.VisibilitySetting.Visible;
							viewTemplate.Add(visibleField);
						}
						else
						{
							viewTemplate.Add(field);
						}
					}
				}
			}
			return viewTemplate;
		}

		private static string[] SplitUpFieldNames(string fieldsToShow)
		{
			return fieldsToShow.Split(new char[] {' ', ','}, StringSplitOptions.RemoveEmptyEntries);
		}

		public override void Activate()
		{
			base.Activate();

			_missingInfoControl =
					new MissingInfoControl(GetFilteredData(),
										   ViewTemplate,
										   _filter.FilteringPredicate,
										   LexEntryRepository);
			_missingInfoControl.SelectedIndexChanged += OnRecordSelectionChanged;
		}

		private void OnRecordSelectionChanged(object sender, EventArgs e)
		{
			LexEntryRepository.SaveItem(_missingInfoControl.CurrentEntry);
		}

		public override void Deactivate()
		{
			if (_missingInfoControl != null)
			{
				LexEntryRepository.SaveItem(_missingInfoControl.CurrentEntry);
			}
			base.Deactivate();
			if (_missingInfoControl != null)
			{
				_missingInfoControl.SelectedIndexChanged -= OnRecordSelectionChanged;
				_missingInfoControl.Dispose();
			}
			_missingInfoControl = null;
		}

		/// <summary>
		/// The MissingInfoControl associated with this task
		/// </summary>
		/// <remarks>Non null only when task is activated</remarks>
		public override Control Control
		{
			get { return _missingInfoControl; }
		}

		protected override int ComputeCount(bool returnResultEvenIfExpensive)
		{
			if (_dataHasBeenRetrieved || returnResultEvenIfExpensive)
			{
				return GetFilteredData().Count;
			}
			return CountNotComputed;
		}

		protected override int ComputeReferenceCount()
		{
			//TODO: Make this correct for Examples.  Currently it counts all words which
			//gives an incorrect progress indicator when not all words have meanings
			return LexEntryRepository.CountAllItems();
		}

		public ResultSet<LexEntry> GetFilteredData()
		{
			ResultSet<LexEntry> data =
					LexEntryRepository.GetEntriesMatchingFilterSortedByLexicalUnit(_filter,
																				   _writingSystem);
			_dataHasBeenRetrieved = true;
			return data;
		}

		public ViewTemplate ViewTemplate
		{
			get { return _viewTemplate; }
		}
	}
}