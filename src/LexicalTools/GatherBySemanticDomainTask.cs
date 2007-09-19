using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using WeSay.Data;
using WeSay.Foundation;
using WeSay.Language;
using WeSay.LexicalModel;
using WeSay.LexicalModel.Db4o_Specific;
using WeSay.Project;

namespace WeSay.LexicalTools
{
	public class SemanticDomainSortHelper : ISortHelper<string, LexEntry>
	{
		private Db4oDataSource _db4oData;
		private string _semanticDomainFieldName;

		public SemanticDomainSortHelper(Db4oDataSource db4oData, string semanticDomainFieldName)
		{
			if (db4oData == null)
			{
				throw new ArgumentNullException("db4oData");
			}
			if (semanticDomainFieldName == null)
			{
				throw new ArgumentNullException("semanticDomainFieldName");
			}
			if (semanticDomainFieldName == string.Empty)
			{
				throw new ArgumentOutOfRangeException("semanticDomainFieldName");
			}

			_db4oData = db4oData;
			_semanticDomainFieldName = semanticDomainFieldName;
		}

		#region IDb4oSortHelper<string,LexEntry> Members

		public IComparer<string> KeyComparer
		{
			get { return StringComparer.InvariantCulture; }
		}

		public List<KeyValuePair<string, long>> GetKeyIdPairs()
		{
			return KeyToEntryIdInitializer.GetKeyToEntryIdPairs(_db4oData, GetKeys);
		}

		public IEnumerable<string> GetKeys(LexEntry item)
		{
			List<string> keys = new List<string>();
			foreach (LexSense sense in item.Senses)
			{
				OptionRefCollection semanticDomains = sense.GetProperty<OptionRefCollection>(_semanticDomainFieldName);

				if (semanticDomains != null)
				{
					foreach (string s in semanticDomains)
					{
						if (!keys.Contains(s))
						{
							keys.Add(s);
						}
					}
				}
			}
			return keys;
		}

		public string Name
		{
			get { return "LexEntry sorted by " + _semanticDomainFieldName; }
		}

		#endregion
	}

	public class GatherBySemanticDomainTask : TaskBase
	{
		private readonly string _semanticDomainQuestionsFileName;
		private GatherBySemanticDomainsControl _gatherControl;
		private Dictionary<string, List<string>> _domainQuestions;
		private List<string> _domainKeys;
		private List<string> _domainNames;
		private List<string> _words;
		private CachedSortedDb4oList<string, LexEntry> _entries;

		private WritingSystem _lexicalFormWritingSystem;
		private WritingSystem _semanticDomainWritingSystem;
		private Field _semanticDomainField;
		private OptionsList _semanticDomainOptionsList;

		private int _currentDomainIndex;
		private int _currentQuestionIndex;

		public GatherBySemanticDomainTask(IRecordListManager recordListManager,
										  string label,
										  string description,
										  string semanticDomainQuestionsFileName,
										  ViewTemplate viewTemplate,
										  string semanticDomainFieldName)
				: base(label, description, false, recordListManager)
		{
			if (semanticDomainQuestionsFileName == null)
			{
				throw new ArgumentNullException("semanticDomainQuestionsFileName");
			}
			if (viewTemplate == null)
			{
				throw new ArgumentNullException("viewTemplate");
			}

			Field lexicalFormField = viewTemplate.GetField(Field.FieldNames.EntryLexicalForm.ToString());
			if (lexicalFormField == null || lexicalFormField.WritingSystems.Count < 1)
			{
				_lexicalFormWritingSystem = BasilProject.Project.WritingSystems.UnknownVernacularWritingSystem;
			}
			else
			{
				_lexicalFormWritingSystem = lexicalFormField.WritingSystems[0];
			}

			_currentDomainIndex = 0;
			_currentQuestionIndex = 0;
			_words = null;
			_semanticDomainQuestionsFileName = semanticDomainQuestionsFileName;
			if (!File.Exists(semanticDomainQuestionsFileName))
			{
				string pathInProject =
						Path.Combine(WeSayWordsProject.Project.PathToWeSaySpecificFilesDirectoryInProject,
									 semanticDomainQuestionsFileName);
				if (File.Exists(pathInProject))
				{
					_semanticDomainQuestionsFileName = pathInProject;
				}
				else
				{
					string pathInProgramDir =
							Path.Combine(WeSayWordsProject.Project.ApplicationCommonDirectory,
										 semanticDomainQuestionsFileName);
					if (!File.Exists(pathInProgramDir))
					{
						throw new ApplicationException(
								string.Format(
										"Could not find the semanticDomainQuestions file {0}. Expected to find it at: {1} or {2}",
										semanticDomainQuestionsFileName,
										pathInProject,
										pathInProgramDir));
					}
					_semanticDomainQuestionsFileName = pathInProgramDir;
				}
			}

			_semanticDomainField = viewTemplate.GetField(semanticDomainFieldName);
		}

		private new Db4oRecordListManager RecordListManager
		{
			get { return (Db4oRecordListManager) base.RecordListManager; }
		}

		/// <summary>
		/// The GatherWordListControl associated with this task
		/// </summary>
		/// <remarks>Non null only when task is activated</remarks>
		public override Control Control
		{
			get
			{
				if (!IsActive)
				{
					throw new InvalidOperationException("Task must be active to use this property");
				}
				return _gatherControl;
			}
		}

		public List<string> DomainKeys
		{
			get { return _domainKeys; }
		}

		public string CurrentDomainKey
		{
			get { return DomainKeys[CurrentDomainIndex]; }
		}

		public List<string> DomainNames
		{
			get
			{
				if (_domainNames == null)
				{
					PopulateDomainNames();
				}
				return _domainNames;
			}
		}

		private void PopulateDomainNames()
		{
			_domainNames = new List<string>();
			foreach (string domainKey in _domainKeys)
			{
				_domainNames.Add(GetOptionNameFromKey(domainKey));
			}
		}

		public string CurrentDomainName
		{
			get { return GetOptionNameFromKey(CurrentDomainKey); }
		}

		private string GetOptionNameFromKey(string key)
		{
			Option option = GetOptionFromKey(key);
			if (option == null)
			{
				return key;
			}
			return option.Name.GetExactAlternative(SemanticDomainWritingSystemId);
		}

		private Option GetOptionFromKey(string key)
		{
			return _semanticDomainOptionsList.Options.Find(delegate(Option o) { return o.Key == key; });
		}

		public string CurrentDomainDescription
		{
			get
			{
				Option option = GetOptionFromKey(CurrentDomainKey);
				if (option == null)
				{
					return string.Empty;
				}
				return option.Description.GetExactAlternative(SemanticDomainWritingSystemId);
			}
		}

		public int CurrentDomainIndex
		{
			get { return _currentDomainIndex; }
			set
			{
				if (value < 0 || value >= DomainKeys.Count)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != _currentDomainIndex)
				{
					_currentDomainIndex = value;
					_currentQuestionIndex = 0;
					_words = null;
				}
			}
		}

		private void UpdateCurrentWords()
		{
			_words = null;
		}

		public string CurrentQuestion
		{
			get { return Questions[CurrentQuestionIndex]; }
		}

		public int CurrentQuestionIndex
		{
			get { return _currentQuestionIndex; }
			set
			{
				if (value < 0 || value >= Questions.Count)
				{
					throw new ArgumentOutOfRangeException();
				}
				_currentQuestionIndex = value;
			}
		}

		public List<string> Questions
		{
			get { return _domainQuestions[_domainKeys[_currentDomainIndex]]; }
		}

		public int WordsInDomain(int domainIndex)
		{
			if (domainIndex < 0 || domainIndex >= _domainKeys.Count)
			{
				throw new ArgumentOutOfRangeException();
			}
			int beginIndex;
			int pastEndIndex;
			GetWordsIndexes(domainIndex, out beginIndex, out pastEndIndex);
			return pastEndIndex - beginIndex;
		}

		public List<string> CurrentWords
		{
			get
			{
				if (_words == null)
				{
					_words = new List<string>();

					int beginIndex;
					int pastEndIndex;
					GetWordsIndexes(CurrentDomainIndex, out beginIndex, out pastEndIndex);
					for (int i = beginIndex; i < pastEndIndex; i++)
					{
						_words.Add(_entries.GetValue(i).LexicalForm.GetBestAlternative(WordWritingSystemId, "*"));
					}
				}
				return _words;
			}
		}

		public bool HasNextDomainQuestion
		{
			get
			{
				if (_currentDomainIndex < DomainKeys.Count - 1)
				{
					return true; // has another domain
				}

				if (_currentQuestionIndex < Questions.Count - 1)
				{
					return true; // has another question
				}

				return false;
			}
		}

		public void GotoNextDomainQuestion()
		{
			if (_currentQuestionIndex == Questions.Count - 1)
			{
				if (_currentDomainIndex < DomainKeys.Count - 1)
				{
					_currentDomainIndex++;
					_currentQuestionIndex = 0;
				}
			}
			else
			{
				_currentQuestionIndex++;
			}
			UpdateCurrentWords();
		}

		public bool HasPreviousDomainQuestion
		{
			get
			{
				if (_currentDomainIndex == 0 &&
					_currentQuestionIndex == 0)
				{
					return false;
				}

				return true;
			}
		}

		public string WordWritingSystemId
		{
			get { return _lexicalFormWritingSystem.Id; }
		}

		public WritingSystem WordWritingSystem
		{
			get { return _lexicalFormWritingSystem; }
		}

		public string SemanticDomainWritingSystemId
		{
			get { return _semanticDomainWritingSystem.Id; }
		}

		public WritingSystem SemanticDomainWritingSystem
		{
			get { return _semanticDomainWritingSystem; }
		}

		public void GotoPreviousDomainQuestion()
		{
			if (_currentQuestionIndex != 0)
			{
				_currentQuestionIndex--;
			}
			else
			{
				if (_currentDomainIndex != 0)
				{
					_currentDomainIndex--;
					_currentQuestionIndex = Questions.Count - 1;
				}
			}
			UpdateCurrentWords();
		}

		public void AddWord(string lexicalForm)
		{
			if (lexicalForm == null)
			{
				throw new ArgumentNullException();
			}
			if (lexicalForm != string.Empty)
			{
				List<LexEntry> entries = GetEntriesHavingLexicalForm(lexicalForm);
				if (entries.Count == 0)
				{
					LexEntry entry = new LexEntry();
					entry.LexicalForm.SetAlternative(WordWritingSystemId, lexicalForm);
					AddCurrentSemanticDomainToEntry(entry);
					_entries.Add(entry);
				}
				else
				{
					foreach (LexEntry entry in entries)
					{
						AddCurrentSemanticDomainToEntry(entry);
					}
				}
			}

			UpdateCurrentWords();
			RecordListManager.GoodTimeToCommit();
		}

		public void RemoveWord(string lexicalForm)
		{
			if (lexicalForm == null)
			{
				throw new ArgumentNullException();
			}
			if (lexicalForm != string.Empty)
			{
				List<LexEntry> entries = GetEntriesHavingLexicalForm(lexicalForm);
				foreach (LexEntry entry in entries)
				{
					if (EntryHasLexicalFormAndSemanticDomainAsOnlyContent(entry))
					{
						_entries.Remove(entry);
					}
					else
					{
						DisassociateCurrentSemanticDomainFromEntry(entry);
					}
				}
			}

			UpdateCurrentWords();
			RecordListManager.GoodTimeToCommit();
		}

		private bool EntryHasLexicalFormAndSemanticDomainAsOnlyContent(LexEntry entry)
		{
			if (entry.LexicalForm.Count > 1)
			{
				return false;
			}

			if (entry.LexicalForm.Count == 1 &&
				!entry.LexicalForm.ContainsAlternative(WordWritingSystemId))
			{
				return false;
			}

			if (entry.HasProperties)
			{
				return false;
			}

			if (entry.Senses.Count > 1)
			{
				return false;
			}

			if (entry.Senses.Count == 1)
			{
				LexSense sense = (LexSense) entry.Senses[0];

				if (sense.ExampleSentences.Count != 0)
				{
					return false;
				}

				if (!sense.Gloss.Empty)
				{
					return false;
				}

				if (sense.Properties.Count > 1)
				{
					return false;
				}

				if (sense.Properties.Count == 1)
				{
					OptionRefCollection semanticDomains =
							sense.GetProperty<OptionRefCollection>(_semanticDomainField.FieldName);
					if (semanticDomains == null)
					{
						return false;
					}
				}
			}

			return true;
		}

		private void DisassociateCurrentSemanticDomainFromEntry(LexEntry entry)
		{
			// have to iterate through these in reverse order
			// since they might get modified
			for (int i = entry.Senses.Count - 1; i >= 0; i--)
			{
				LexSense sense = (LexSense) entry.Senses[i];
				OptionRefCollection semanticDomains =
						sense.GetProperty<OptionRefCollection>(_semanticDomainField.FieldName);
				if (semanticDomains != null)
				{
					semanticDomains.Remove(CurrentDomainKey);
				}
			}
			entry.CleanUpAfterEditting();
		}

		private List<LexEntry> GetEntriesHavingLexicalForm(string lexicalForm)
		{
			List<LexEntry> result = new List<LexEntry>();
			// search dictionary for entry with new lexical form
			LexEntrySortHelper sortHelper = new LexEntrySortHelper(RecordListManager.DataSource,
																   WordWritingSystem,
																   true);
			CachedSortedDb4oList<string, LexEntry> entriesByLexicalForm = RecordListManager.GetSortedList(sortHelper);
			int index = entriesByLexicalForm.BinarySearch(lexicalForm);
			while (index >= 0 && index < entriesByLexicalForm.Count &&
				   entriesByLexicalForm.GetKey(index) == lexicalForm)
			{
				result.Add(entriesByLexicalForm.GetValue(index));
				++index;
			}
			return result;
		}

		private void AddCurrentSemanticDomainToEntry(LexEntry entry)
		{
			LexSense sense = entry.GetOrCreateSenseWithGloss(new MultiText());
			OptionRefCollection semanticDomains =
					sense.GetOrCreateProperty<OptionRefCollection>(_semanticDomainField.FieldName);
			if (!semanticDomains.Contains(CurrentDomainKey))
			{
				semanticDomains.Add(CurrentDomainKey);
			}
		}

		private void GetWordsIndexes(int domainIndex, out int beginIndex, out int pastEndIndex)
		{
			string domainKey = DomainKeys[domainIndex];

			beginIndex = _entries.BinarySearch(domainKey);
			if (beginIndex < 0)
			{
				pastEndIndex = beginIndex;
				return;
			}
			pastEndIndex = beginIndex + 1;
			while (pastEndIndex < _entries.Count &&
				   _entries.GetKey(pastEndIndex) == domainKey)
			{
				++pastEndIndex;
			}
		}

		private void ParseSemanticDomainFile()
		{
			XmlTextReader reader = null;
			try
			{
				reader = new XmlTextReader(_semanticDomainQuestionsFileName);
				reader.MoveToContent();
				if (!reader.IsStartElement("semantic-domain-questions"))
				{
					//what are we going to do when the file is bad?
					Debug.Fail("Bad file format, expected semantic-domain-questions element");
				}
				string ws = reader.GetAttribute("lang");
				// should verify that this writing system is in optionslist
				_semanticDomainWritingSystem = BasilProject.Project.WritingSystems[ws];
				string semanticDomainType = reader.GetAttribute("semantic-domain-type");
				// should verify that domain type matches type of optionList in semantic domain field

				reader.ReadToDescendant("semantic-domain");
				while (reader.IsStartElement("semantic-domain"))
				{
					string domainKey = reader.GetAttribute("id");
					List<string> questions = new List<string>();
					XmlReader questionReader = reader.ReadSubtree();
					questionReader.MoveToContent();
					questionReader.ReadToFollowing("question");
					while (questionReader.IsStartElement("question"))
					{
						questions.Add(questionReader.ReadElementString("question"));
					}
					_domainKeys.Add(domainKey);
					if (questions.Count == 0)
					{
						questions.Add(string.Empty);
					}
					_domainQuestions.Add(domainKey, questions);
					reader.ReadToFollowing("semantic-domain");
				}
			}
			catch (XmlException)
			{
				// log this;
			}
			finally
			{
				if (reader != null)
				{
					reader.Close();
				}
			}
		}

		public override void Activate()
		{
			base.Activate();
			if (DomainKeys == null)
			{
				_domainKeys = new List<string>();
				_domainQuestions = new Dictionary<string, List<string>>();
				ParseSemanticDomainFile();

				// always have at least one domain and one question
				// so default indexes of 0 are valid.
				if (_domainKeys.Count == 0)
				{
					_domainKeys.Add(string.Empty);
				}
				if (_domainQuestions.Count == 0)
				{
					List<string> emptyList = new List<string>();
					emptyList.Add(string.Empty);
					_domainQuestions.Add(string.Empty, emptyList);
				}
			}
			if (_semanticDomainOptionsList == null)
			{
				_semanticDomainOptionsList =
						WeSayWordsProject.Project.GetOptionsList(_semanticDomainField, false);
			}
			_entries =
					RecordListManager.GetSortedList(
							new SemanticDomainSortHelper(RecordListManager.DataSource, _semanticDomainField.FieldName));

			UpdateCurrentWords();
			_gatherControl = new GatherBySemanticDomainsControl(this);
		}

		public override void Deactivate()
		{
			base.Deactivate();
			_gatherControl.Dispose();
			_gatherControl = null;
			UpdateCurrentWords(); // clears out orphan records

			RecordListManager.GoodTimeToCommit();
		}

		/// <summary>
		/// Gives a sense of the overall size of the task versus what's left to do
		/// </summary>
		public override int ReferenceCount
		{
			get { return -1; //todo
			}
		}
	}
}