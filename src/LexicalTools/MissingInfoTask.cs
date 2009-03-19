using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Palaso.Reporting;
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
		private readonly Field _missingInfoField;
		private readonly ViewTemplate _viewTemplate;
		private bool _dataHasBeenRetrieved;
		private readonly bool _isBaseFormFillingTask;
		private readonly WritingSystem _writingSystem;

		public MissingInfoTask(LexEntryRepository lexEntryRepository,
							   string missingInfoField,
							   string label,
							   string description,
							   ViewTemplate viewTemplate)
				: this(
						lexEntryRepository,
						missingInfoField,
						label,
						label,
						description,
						string.Empty,
						string.Empty,
						viewTemplate) {}

		public MissingInfoTask(LexEntryRepository lexEntryRepository,
							   string field,
							   string label,
							   string longLabel,
							   string description,
							   string remainingCountText,
							   string referenceCountText,
							   ViewTemplate viewTemplate)
				: base(
						label,
						longLabel,
						description,
						remainingCountText,
						referenceCountText,
						false,
						lexEntryRepository)
		{
			if (field == null)
			{
				throw new ArgumentNullException("field");
			}
			if (viewTemplate == null)
			{
				throw new ArgumentNullException("viewTemplate");
			}

			_missingInfoField = viewTemplate[field];
			_viewTemplate = viewTemplate;
			_writingSystem = BasilProject.Project.WritingSystems.UnknownVernacularWritingSystem;
			// use the master view Template instead of the one for this task. (most likely the one for this
			// task doesn't have the EntryLexicalForm field specified but the Master (Default) one will
			Field fieldDefn =
					WeSayWordsProject.Project.DefaultViewTemplate.GetField(
							Field.FieldNames.EntryLexicalForm.ToString());
			if (fieldDefn != null)
			{
				if (fieldDefn.WritingSystemIds.Count > 0)
				{
					_writingSystem = BasilProject.Project.WritingSystems[fieldDefn.WritingSystemIds[0]];
				}
				else
				{
					throw new ConfigurationException("There are no writing systems enabled for the Field '{0}'",
													 fieldDefn.FieldName);
				}
			}
		}

		public MissingInfoTask(LexEntryRepository lexEntryRepository,
							   string missingInfoField,
							   string label,
							   string description,
							   ViewTemplate viewTemplate,
							   string fieldsToShow)
				: this(
						lexEntryRepository,
						missingInfoField,
						label,
						label,
						description,
						string.Empty,
						string.Empty,
						viewTemplate,
						fieldsToShow) {}

		/// <summary>
		/// Creates a generic Lexical Field editing task
		/// </summary>
		/// <param name="lexEntryRepository">The lexEntryRepository that will provide the data</param>
		/// <param name="field">The field declaration of what is missing</param>
		/// <param name="label">The task label</param>
		/// <param name="longLabel">Slightly longer task label (for ToolTips)</param>
		/// <param name="description">The task description</param>
		/// <param name="remainingCountText">Text describing the remaining count</param>
		/// <param name="referenceCountText">Text describing the reference count</param>
		/// <param name="viewTemplate">The base viewTemplate</param>
		/// <param name="showfields">The fields to show from the base Field Inventory</param>
		public MissingInfoTask(LexEntryRepository lexEntryRepository,
							   string field,
							   string label,
							   string longLabel,
							   string description,
							   string remainingCountText,
							   string referenceCountText,
							   ViewTemplate viewTemplate,
							   string showfields)
				: this(
						lexEntryRepository,
						field,
						label,
						longLabel,
						description,
						remainingCountText,
						referenceCountText,
						viewTemplate)
		{
			if (showfields == null)
			{
				throw new ArgumentNullException("showfields");
			}
			_viewTemplate = CreateViewTemplateFromListOfFields(viewTemplate, showfields);

			//hack until we overhaul how Tasks are setup:
			_isBaseFormFillingTask = showfields.Contains(LexEntry.WellKnownProperties.BaseForm);
			if (_isBaseFormFillingTask)
			{
				Field flagField = new Field();
				flagField.DisplayName = StringCatalog.Get("~This word has no Base Form",
														  "The user will click this to say that this word has no baseform.  E.g. Kindess has Kind as a baseform, but Kind has no other word as a baseform.");
				flagField.DataTypeName = "Flag";
				flagField.ClassName = "LexEntry";
				flagField.FieldName = "flag_skip_" + field;
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
							   string missingInfoField,
							   string label,
							   string description,
							   ViewTemplate viewTemplate,
							   string showFields,
							   string readOnly)
				: this(
						lexEntryRepository,
						missingInfoField,
						label,
						label,
						description,
						string.Empty,
						string.Empty,
						viewTemplate,
						showFields,
						readOnly) {}
		/// <summary>
		/// This is the ctor being used on a Nov-2008 era config
		/// </summary>
		/// <param name="lexEntryRepository"></param>
		/// <param name="field"></param>
		/// <param name="label"></param>
		/// <param name="longLabel"></param>
		/// <param name="description"></param>
		/// <param name="remainingCountText"></param>
		/// <param name="referenceCountText"></param>
		/// <param name="viewTemplate"></param>
		/// <param name="showfields"></param>
		/// <param name="readOnly"></param>
		public MissingInfoTask(LexEntryRepository lexEntryRepository,
							   string field,
							   string label,
							   string longLabel,
							   string description,
							   string remainingCountText,
							   string referenceCountText,
							   ViewTemplate viewTemplate,
							   string showfields,
							   string readOnly)
				: this(
						lexEntryRepository,
						field,
						label,
						longLabel,
						description,
						remainingCountText,
						referenceCountText,
						viewTemplate,
						showfields + " " + readOnly)
		{
			MarkReadOnlyFields(readOnly);
		}

		private void MarkReadOnlyFields(string fieldsToShowReadOnly)
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
						enabledField.Visibility = CommonEnumerations.VisibilitySetting.Visible;
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

			Predicate<LexEntry> filteringPredicate =
					new MissingFieldQuery(_missingInfoField).FilteringPredicate;
			_missingInfoControl = new MissingInfoControl(GetFilteredData(),
														 ViewTemplate,
														 filteringPredicate,
														 LexEntryRepository);
			_missingInfoControl.TimeToSaveRecord += OnSaveRecord;
		}

		private void OnSaveRecord(object sender, EventArgs e)
		{
			SaveRecord();
		}

		private void SaveRecord()
		{
				   if (_missingInfoControl != null && _missingInfoControl.CurrentEntry != null)
				{
					LexEntryRepository.SaveItem(_missingInfoControl.CurrentEntry);
				}

		}

		public override void Deactivate()
		{
			SaveRecord();
			base.Deactivate();
			if (_missingInfoControl != null)
			{
				_missingInfoControl.TimeToSaveRecord -= OnSaveRecord;
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
					LexEntryRepository.GetEntriesWithMissingFieldSortedByLexicalUnit(
							_missingInfoField, _writingSystem);
			_dataHasBeenRetrieved = true;
			return data;
		}

		public ViewTemplate ViewTemplate
		{
			get { return _viewTemplate; }
		}
	}
}