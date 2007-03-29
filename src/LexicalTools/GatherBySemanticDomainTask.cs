using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using WeSay.Data;
using WeSay.Foundation;
using WeSay.Language;
using WeSay.LexicalModel;
using WeSay.LexicalModel.Db4o_Specific;

namespace WeSay.LexicalTools
{
	public class SemanticDomainSortHelper : IDb4oSortHelper<string, LexEntry>
	{
		Db4oDataSource _db4oData;
		string _semanticDomainFieldName;

		public SemanticDomainSortHelper(Db4oDataSource db4oData, string semanticDomainFieldName)
		{
			if (db4oData == null)
			{
				throw new ArgumentNullException("db4oData");
			}
			if(semanticDomainFieldName == null)
			{
				throw new ArgumentNullException("semanticDomainFieldName");
			}
			if(semanticDomainFieldName == string.Empty)
			{
				throw new ArgumentOutOfRangeException("semanticDomainFieldName");
			}

			_db4oData = db4oData;
			_semanticDomainFieldName = semanticDomainFieldName;
		}

		#region IDb4oSortHelper<string,LexEntry> Members

		public IComparer<string> KeyComparer
		{
			get
			{
				return StringComparer.InvariantCulture;
			}
		}

		public List<KeyValuePair<string, long>> GetKeyIdPairs()
		{
			// there isn't actually a very efficient way to do this yet.
			List<KeyValuePair<string, long>> result = new List<KeyValuePair<string, long>>();
			foreach (KeyValuePair<LexEntry, long> entryToId in KeyToEntryIdInitializer.GetEntryToEntryIdPairs(_db4oData))
			{
				foreach (string s in GetKeys(entryToId.Key))
				{
					result.Add(new KeyValuePair<string, long>(s, entryToId.Value));
				}
			}
			return result;
		}

		public IEnumerable<string> GetKeys(LexEntry item)
		{
			List<string> keys = new List<string>();
			foreach (LexSense sense in item.Senses)
			{
				OptionRefCollection semanticDomains = sense.GetProperty<OptionRefCollection>(_semanticDomainFieldName);

				if(semanticDomains != null)
				{
					foreach (string s in semanticDomains)
					{
						if(!keys.Contains(s))
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
			get
			{
				return "LexEntry sorted by " + _semanticDomainFieldName;
			}
		}

		#endregion
	}

	public class GatherBySemanticDomainTask : TaskBase
	{
		private readonly string _semanticDomainFileName;
		private GatherBySemanticDomainsControl  _gatherControl;
		private Dictionary<string, List<string>> _domainQuestions;
		private List<string> _domains;
		private List<string> _words;
		private CachedSortedDb4oList<string, LexEntry> _entries;

		string _lexicalFormWritingSystemId;
		string _semanticDomainFieldName;

		int _currentDomainIndex;
		private int _currentQuestionIndex;

		public GatherBySemanticDomainTask(Db4oRecordListManager recordListManager,
										  string label,
										  string description,
										  string semanticDomainFileName,
										  string semanticDomainFileWritingSystem,
										  string lexicalFormWritingSystemId,
										  string semanticDomainFieldName)
			: base(label, description, false, recordListManager)
		{
			if(semanticDomainFileName == null)
			{
				throw new ArgumentNullException("semanticDomainFileName");
			}
			if (semanticDomainFileWritingSystem == null)
			{
				throw new ArgumentNullException("semanticDomainFileWritingSystem");
			}
			if (lexicalFormWritingSystemId == null)
			{
				throw new ArgumentNullException("lexicalFormWritingSystemId");
			}
			if (semanticDomainFieldName == null)
			{
				throw new ArgumentNullException("semanticDomainFieldName");
			}

			_currentDomainIndex = 0;
			_currentQuestionIndex = 0;
			_words = null;
			_semanticDomainFileName = semanticDomainFileName;

			_semanticDomainFieldName = semanticDomainFieldName;
			_lexicalFormWritingSystemId = lexicalFormWritingSystemId;
		}

		private new Db4oRecordListManager RecordListManager
		{
			get
			{
				return (Db4oRecordListManager)base.RecordListManager;
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
				if(!IsActive)
				{
					throw new InvalidOperationException("Task must be active to use this property");
				}
				return _gatherControl;
			}
		}

		public List<string> Domains
		{
			get { return this._domains; }
		}

		public string CurrentDomain
		{
			get
			{
				return Domains[CurrentDomainIndex];
			}
		}

		public int CurrentDomainIndex
		{
			get { return _currentDomainIndex; }
			set
			{
				if (value < 0 || value >= Domains.Count)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (value != this._currentDomainIndex)
				{
					this._currentDomainIndex = value;
					this._currentQuestionIndex = 0;
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
			get
			{
				return Questions[CurrentQuestionIndex];
			}
		}

		public int CurrentQuestionIndex
		{
			get { return this._currentQuestionIndex; }
			set
			{
				if (value < 0 || value >= Questions.Count)
				{
					throw new ArgumentOutOfRangeException();
				}
				this._currentQuestionIndex = value;
			}
		}

		public List<string> Questions
		{
			get { return this._domainQuestions[_domains[_currentDomainIndex]]; }
		}

		public List<string> CurrentWords
		{
			get {
				if(this._words == null)
				{
					_words = new List<string>();

					int beginIndex;
					int pastEndIndex;
					GetWordsIndexes(out beginIndex, out pastEndIndex);
					for (int i = beginIndex; i < pastEndIndex; i++)
					{
						_words.Add(_entries.GetValue(i).LexicalForm.GetBestAlternative(_lexicalFormWritingSystemId, "*"));
					}
				}
				return this._words;
			 }
		}

		public bool HasNextDomainQuestion
		{
			get {
				if (_currentDomainIndex < Domains.Count - 1)
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
			if(_currentQuestionIndex == Questions.Count-1)
			{
				if(_currentDomainIndex < Domains.Count-1)
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
			get {
				if (_currentDomainIndex == 0 &&
					_currentQuestionIndex == 0)
				{
					return false;
				}

				return true;
			}
		}
		public void GotoPreviousDomainQuestion()
		{
			if (_currentQuestionIndex != 0)
			{
				_currentQuestionIndex--;
			}
			else
			{
				if(_currentDomainIndex != 0)
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
				if(entries.Count == 0)
				{
					LexEntry entry = new LexEntry();
					entry.LexicalForm.SetAlternative(_lexicalFormWritingSystemId, lexicalForm);
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
			if(entry.LexicalForm.Count > 1)
			{
				return false;
			}

			if(entry.LexicalForm.Count == 1 &&
			   !entry.LexicalForm.ContainsAlternative(_lexicalFormWritingSystemId))
			{
				return false;
			}

			if(entry.HasProperties)
			{
				return false;
			}

			if(entry.Senses.Count > 1)
			{
				return false;
			}

			if(entry.Senses.Count == 1)
			{
				LexSense sense = (LexSense) entry.Senses[0];

				if(sense.ExampleSentences.Count != 0)
				{
					return false;
				}

				if(!sense.Gloss.Empty)
				{
					return false;
				}

				if (sense.Properties.Count > 1)
				{
					return false;
				}

				if(sense.Properties.Count == 1)
				{
					OptionRefCollection semanticDomains = sense.GetProperty<OptionRefCollection>(_semanticDomainFieldName);
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
				OptionRefCollection semanticDomains = sense.GetProperty<OptionRefCollection>(_semanticDomainFieldName);
				if (semanticDomains != null)
				{
					semanticDomains.Remove(CurrentDomain);
				}
			}
		}

		private List<LexEntry> GetEntriesHavingLexicalForm(string lexicalForm)
		{
			List<LexEntry> result = new List<LexEntry>();
			// search dictionary for entry with new lexical form
			LexEntrySortHelper sortHelper = new LexEntrySortHelper(RecordListManager.DataSource,
																   this._lexicalFormWritingSystemId,
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
			OptionRefCollection semanticDomains = sense.GetOrCreateProperty<OptionRefCollection>(_semanticDomainFieldName);
			if (!semanticDomains.Contains(CurrentDomain))
			{
				semanticDomains.Add(CurrentDomain);
			}
		}

		private void GetWordsIndexes(out int beginIndex, out int pastEndIndex) {
			beginIndex = this._entries.BinarySearch(CurrentDomain);
			if(beginIndex < 0)
			{
				pastEndIndex = beginIndex;
				return;
			}
			pastEndIndex = beginIndex + 1;
			while (pastEndIndex < this._entries.Count &&
				   this._entries.GetKey(pastEndIndex) == CurrentDomain)
			{
				++pastEndIndex;
			}
		}

		private void ParseSemanticDomainFile()
		{
			// for now just create some sample data

			// in test data, the first domain has three questions
			AddDomainAndQuestions("1 Universe, creation",
								  new string[] { "question 1", "question 2", "question 3" });

			AddDomainAndQuestions("1.1 Sky",
								  new string[] { "question 1", "question 2" });
			AddDomainAndQuestions("1.1.1 Sun",
								  new string[] { "question 1", "question 2" });
			// in test data, the fourth domain has no questions
			AddDomainAndQuestions("1.1.1.1 Moon",
								  new string[] { "" });
			AddDomainAndQuestions("1.1.1.2 Star",
								  new string[] { "question 1", "question 2" });
			AddDomainAndQuestions("1.1.1.3 Planet",
								  new string[] { "question 1", "question 2" });
			AddDomainAndQuestions("1.1.2 Air",
								  new string[] { "question 1", "question 2" });
			AddDomainAndQuestions("1.1.2.1 Blow air",
								  new string[] { "question 1", "question 2" });
		}

		private void AddDomainAndQuestions(string domainKey, IEnumerable<string> questions)
		{
			this._domains.Add(domainKey);
			this._domainQuestions.Add(domainKey, new List<string>(questions));
		}

		public override void Activate()
		{
			base.Activate();
			if (Domains == null)
			{
				_domains = new List<string>();
				_domainQuestions = new Dictionary<string, List<string>>();
				ParseSemanticDomainFile();

				// always have at least one domain and one question
				// so default indexes of 0 are valid.
				if(_domains.Count == 0)
				{
					_domains.Add(string.Empty);
				}
				if(_domainQuestions.Count == 0)
				{
					List<string> emptyList = new List<string>();
					emptyList.Add(string.Empty);
					_domainQuestions.Add(string.Empty, emptyList);
				}
			}
			_entries = RecordListManager.GetSortedList(new SemanticDomainSortHelper(RecordListManager.DataSource, _semanticDomainFieldName));
			UpdateCurrentWords();
			_gatherControl = new GatherBySemanticDomainsControl();
		}


		public override void Deactivate()
		{
			base.Deactivate();
			_gatherControl.Dispose();
			_gatherControl = null;
			UpdateCurrentWords(); // clears out orphan records

			RecordListManager.GoodTimeToCommit();
		}
	}
}
