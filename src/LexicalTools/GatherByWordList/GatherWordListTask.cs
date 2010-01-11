using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Palaso.Code;
using Palaso.Data;
using Palaso.DictionaryServices.Model;
using Palaso.Lift;
using Palaso.Lift.Options;
using Palaso.Progress;
using Palaso.Reporting;
using Palaso.Text;
using WeSay.LexicalModel;
using WeSay.LexicalModel.Foundation;
using WeSay.Project;

namespace WeSay.LexicalTools.GatherByWordList
{
	public class GatherWordListTask: WordGatheringTaskBase
	{
		private readonly string _lexemeFormListFileName;
		private GatherWordListControl _gatherControl;
		private List<LexEntry> _words;
		private int _currentWordIndex;
		private readonly string _writingSystemIdForWordListWords;
		private readonly WritingSystem _lexicalUnitWritingSystem;

		public GatherWordListTask(IGatherWordListConfig config,
									LexEntryRepository lexEntryRepository,
								  ViewTemplate viewTemplate,
			 TaskMemoryRepository taskMemoryRepository)

				: base(config, lexEntryRepository, viewTemplate, taskMemoryRepository)
		{
			Guard.AgainstNull(config.WordListFileName, "config.WordListFileName");
			Guard.AgainstNull(config.WordListWritingSystemId, "config.WordListWritingSystemId");
			Guard.AgainstNull(viewTemplate, "viewTemplate");

			Field lexicalFormField = viewTemplate.GetField(
				Field.FieldNames.EntryLexicalForm.ToString()
			);
			if (lexicalFormField == null || lexicalFormField.WritingSystemIds.Count < 1)
			{
				_lexicalUnitWritingSystem =
						BasilProject.Project.WritingSystems.UnknownVernacularWritingSystem;
			}
			else
			{
				string firstWSid = lexicalFormField.WritingSystemIds[0];
				WritingSystem firstWS = BasilProject.Project.WritingSystems[firstWSid];
				_lexicalUnitWritingSystem = firstWS;
			}

			_lexemeFormListFileName = config.WordListFileName;
			_words = null;
			_writingSystemIdForWordListWords = config.WordListWritingSystemId;
		}

		private void LoadWordList()
		{
			string pathLocal =
					Path.Combine(
							WeSayWordsProject.Project.PathToWeSaySpecificFilesDirectoryInProject,
							_lexemeFormListFileName);
			string pathToUse = pathLocal;
			if (!File.Exists(pathLocal))
			{
				string pathInProgramDir = Path.Combine(BasilProject.ApplicationCommonDirectory,
													   _lexemeFormListFileName);
				pathToUse = pathInProgramDir;
				if (!File.Exists(pathToUse))
				{
					ErrorReport.NotifyUserOfProblem(
							"WeSay could not find the wordlist.  It expected to find it either at {0} or {1}.",
							pathLocal,
							pathInProgramDir);
					return;
				}
			}
			_words = new List<LexEntry>();
			if(".lift"==Path.GetExtension(pathToUse).ToLower())
			{
				LoadLift(pathToUse);
			}
			else
			{
				LoadSimpleList(pathToUse);
			}

			NavigateFirstToShow();
		}

		private void LoadLift(string path)
		{
			//Performance wise, the following is not expecting a huge, 10k word list.

			using (var reader = new Palaso.DictionaryServices.Lift.LiftReader(new NullProgressState(),
				WeSayWordsProject.Project.GetSemanticDomainsList(),
				WeSayWordsProject.Project.GetIdsOfSingleOptionFields()))
			using(var m = new MemoryDataMapper<LexEntry>())
			{
				reader.Read(path, m);
				_words.AddRange(from RepositoryId repositoryId in m.GetAllItems() select m.GetItem(repositoryId));
			}
		}

		private void LoadSimpleList(string path)
		{
			using (TextReader r = File.OpenText(path))
			{
				do
				{
					string s = r.ReadLine();
					if (s == null)
					{
						break;
					}
					s = s.Trim();

					if (!string.IsNullOrEmpty(s))//skip blank lines
					{
						var entry = new LexEntry();
						entry.LexicalForm.SetAlternative(_writingSystemIdForWordListWords, s);
						var sense = new LexSense(entry);
						sense.Gloss.SetAlternative(_writingSystemIdForWordListWords, s);
						entry.Senses.Add(sense);
						_words.Add(entry);
					}
				}
				while (true);
			}
		}

		public bool IsTaskComplete
		{
			get
			{
				if (_words != null)
				{
					return CurrentIndexIntoWordlist >= _words.Count;
				}
				return true;
			}
		}

		/// <summary>
		/// The GatherWordListControl associated with this task
		/// </summary>
		/// <remarks>Non null only when task is activated</remarks>
		public override Control Control
		{
			get
			{
				if (_gatherControl == null)
				{
					_gatherControl = new GatherWordListControl(this, _lexicalUnitWritingSystem);
				}
				return _gatherControl;
			}
		}

		public string CurrentLexemeForm
		{
			get
			{
				return CurrentTemplateLexicalEntry.LexicalForm.GetExactAlternative(_writingSystemIdForWordListWords);
			}
		}

		public LexEntry CurrentTemplateLexicalEntry
		{
			get
			{
				Guard.Against(CurrentIndexIntoWordlist >= _words.Count, "CurrentIndexIntoWordlist must be < _words.Count");
				Guard.Against(_words.Count == 0, "There are no words in this list.");
				return _words[CurrentIndexIntoWordlist];
			}
		}

		public bool CanNavigateNext
		{
			get
			{
				if (_words == null)
				{
					return false;
				}
				return _words.Count > CurrentIndexIntoWordlist;
			}
		}

		public bool CanNavigatePrevious
		{
			get { return CurrentIndexIntoWordlist > 0; }
		}

		private int CurrentIndexIntoWordlist
		{
			get { return _currentWordIndex; }
			set
			{
				Guard.Against(value < 0, "_currentWordIndex must be > 0");
				_currentWordIndex = value;

				//nb: (CurrentWordIndex == _words.Count) is used to mark the "all done" state:

				//                if (!_suspendNotificationOfNavigation && UpdateSourceWord != null)
				//                {
				//                    UpdateSourceWord.Invoke(this, null);
				//                }
			}
		}

		public override void Activate()
		{
			if (
					!WeSayWordsProject.Project.WritingSystems.ContainsKey(
							 _writingSystemIdForWordListWords))
			{
				ErrorReport.NotifyUserOfProblem(
						"The writing system of the words in the word list will be used to add reversals and definitions.  Therefore, it needs to be in the list of writing systems for this project.  Either change the writing system that this task uses for the word list (currently '{0}') or add a writing system with this id to the project.",
						_writingSystemIdForWordListWords);
			}

			if (_words == null)
			{
				LoadWordList();
			}
			base.Activate();
		}

		/// <summary>
		/// Someday, we may indeed have multi-string foreign words
		/// </summary>
		public MultiText CurrentWordAsMultiText
		{
			get
			{
				var m = new MultiText();
				m.SetAlternative(_writingSystemIdForWordListWords, CurrentLexemeForm);
				return m;
			}
		}

		public void WordCollected(MultiText newVernacularWord)
		{
//            var sense = new LexSense();
//            sense.Definition.MergeIn(CurrentWordAsMultiText);
//            sense.Gloss.MergeIn(CurrentWordAsMultiText);

//            var templateSense = CurrentTemplateLexicalEntry.Senses.FirstOrDefault();
//            if(templateSense !=null)
//            {
//                foreach (var optionProperty in templateSense.Properties.Where(p => p.Value.GetType() == typeof(OptionRefCollection)))
//                {
//                    var templateRefs = templateSense.GetProperty<OptionRefCollection>(optionProperty.Key);
//                    var destinationOptionRefs =
//                        sense.GetOrCreateProperty<OptionRefCollection>(optionProperty.Key);
//                    destinationOptionRefs.AddRange(templateRefs.Keys);
//                }
//
//                foreach (var optionProperty in templateSense.Properties.Where(p => p.Value.GetType() == typeof(OptionRef)))
//                {
//                    var optionRef = templateSense.GetProperty<OptionRef>(optionProperty.Key);
//                    sense.GetOrCreateProperty<OptionRef>(optionProperty.Key).Key = optionRef.Key;
//                }
//
//                foreach (var textProperty in templateSense.Properties.Where(p=>p.Value.GetType()==typeof(MultiText)))
//                {
//                    var target = sense.GetOrCreateProperty<MultiText>(textProperty.Key);
//                    foreach (var form in ((MultiText)textProperty.Value).Forms)
//                    {
//                        target.SetAlternative(form.WritingSystemId,form.Form);
//                    }
//                }
//            }
			//sens
			//we use this for matching up, and well, it probably is a good gloss

		  //  AddSenseToLexicon(newVernacularWord, sense);
			var sense = CurrentTemplateLexicalEntry.Senses.FirstOrDefault();
			AddSenseToLexicon(newVernacularWord,sense);
		}

		/// <summary>
		/// Try to add the sense to a matching entry. If none found, make a new entry with the sense
		/// </summary>
		private void AddSenseToLexicon(MultiTextBase lexemeForm, LexSense sense)
		{
			var definition = sense.Definition;
			if(definition.Empty)
			{
				foreach (var form in sense.Gloss.Forms)
				{
					definition.SetAlternative(form.WritingSystemId, form.Form);
				}
			}

			var gloss = sense.Gloss;
			if (gloss.Empty)
			{
				foreach (var form in sense.Definition.Forms)
				{
					gloss.SetAlternative(form.WritingSystemId, form.Form);
				}
			}

			//review: the desired semantics of this find are unclear, if we have more than one ws
			ResultSet<LexEntry> entriesWithSameForm =
					LexEntryRepository.GetEntriesWithMatchingLexicalForm(
							lexemeForm[_lexicalUnitWritingSystem.Id], _lexicalUnitWritingSystem);
			if (entriesWithSameForm.Count == 0)
			{
				LexEntry entry = LexEntryRepository.CreateItem();
				entry.LexicalForm.MergeIn(lexemeForm);
				entry.Senses.Add(sense);
				LexEntryRepository.SaveItem(entry);
			}
			else
			{
				LexEntry entry = entriesWithSameForm[0].RealObject;

				foreach (LexSense s in entry.Senses)
				{
					if (sense.Gloss.Forms.Length > 0)
					{
						LanguageForm glossWeAreAdding = sense.Gloss.Forms[0];
						string glossInThisWritingSystem =
								s.Gloss.GetExactAlternative(glossWeAreAdding.WritingSystemId);
						if (glossInThisWritingSystem == glossWeAreAdding.Form)
						{
							return; //don't add it again
						}
					}
				}
				entry.Senses.Add(sense);
				LexEntryRepository.NotifyThatLexEntryHasBeenUpdated(entry);
			}
		}

		public override void Deactivate()
		{
			base.Deactivate();
			if (_gatherControl != null)
			{
				_gatherControl.Dispose();
			}
			_gatherControl = null;
		}

		public void NavigatePrevious()
		{
			--CurrentIndexIntoWordlist;
		}

		public void NavigateNext()
		{
			// _suspendNotificationOfNavigation = true;

			CurrentIndexIntoWordlist++;
			while (CanNavigateNext && GetRecordsWithMatchingGloss().Count > 0)
			{
				++CurrentIndexIntoWordlist;
			}
			//  _suspendNotificationOfNavigation = false;
			//            if (UpdateSourceWord != null)
			//            {
			//                UpdateSourceWord.Invoke(this, null);
			//            }
		}

		public void NavigateFirstToShow()
		{
			_currentWordIndex = -1;
			NavigateNext();
		}

		public void NavigateAbsoluteFirst()
		{
			CurrentIndexIntoWordlist = 0;
		}

		public ResultSet<LexEntry> NotifyOfAddedWord()
		{
			return
					LexEntryRepository.GetEntriesWithMatchingGlossSortedByLexicalForm(
							CurrentWordAsMultiText.Find(_writingSystemIdForWordListWords),
							_lexicalUnitWritingSystem);
		}

		public ResultSet<LexEntry> GetRecordsWithMatchingGloss()
		{
				return
						LexEntryRepository.GetEntriesWithMatchingGlossSortedByLexicalForm(
								CurrentWordAsMultiText.Find(_writingSystemIdForWordListWords),
								_lexicalUnitWritingSystem);
		}

		protected override int ComputeCount(bool returnResultEvenIfExpensive)
		{
			return CountNotRelevant;
		}

		protected override int ComputeReferenceCount()
		{
			return CountNotRelevant; //Todo
		}

		/// <summary>
		/// Removes the sense (if otherwise empty) and deletes the entry if it has no reason left to live
		/// </summary>
		public void TryToRemoveAssociationWithListWordFromEntry(RecordToken<LexEntry> recordToken)
		{
			// have to iterate through these in reverse order
			// since they might get modified
			LexEntry entry = recordToken.RealObject;
			for (int i = entry.Senses.Count - 1;i >= 0;i--)
			{
				LexSense sense = entry.Senses[i];
				if (sense.Gloss != null)
				{
					if (sense.Gloss.ContainsAlternative(_writingSystemIdForWordListWords))
					{
						if (sense.Gloss[_writingSystemIdForWordListWords] == CurrentLexemeForm)
						{
							//since we copy the gloss into the defniition, too, if that hasn't been
							//modified, then we don't want to let it being non-empty keep us from
							//removing the sense. We're trying to enable typo correcting.
							if (sense.Definition[_writingSystemIdForWordListWords] ==
								CurrentLexemeForm)
							{
								sense.Definition.SetAlternative(_writingSystemIdForWordListWords,
																null);
								sense.Definition.RemoveEmptyStuff();
							}
							sense.Gloss.SetAlternative(_writingSystemIdForWordListWords, null);
							sense.Gloss.RemoveEmptyStuff();
							if (!sense.IsEmptyForPurposesOfDeletion)
							{
								//removing the gloss didn't make it empty. So repent of removing the gloss.
								sense.Gloss.SetAlternative(_writingSystemIdForWordListWords,
														   CurrentLexemeForm);
							}
						}
					}
				}
			}
			entry.CleanUpAfterEditting();
			if (entry.IsEmptyExceptForLexemeFormForPurposesOfDeletion)
			{
				LexEntryRepository.DeleteItem(entry);
			}
			else
			{
				LexEntryRepository.SaveItem(entry);
			}
		}


	}
}