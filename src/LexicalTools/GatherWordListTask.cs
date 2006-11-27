using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using WeSay.Data;
using WeSay.Language;
using WeSay.LexicalModel;
using WeSay.LexicalModel.Db4o_Specific;
using WeSay.Project;

namespace WeSay.LexicalTools
{
	public class GatherWordListTask : TaskBase
	{
		private readonly string _wordListFileName;
		private GatherWordListControl _gatherControl;
		private List<string> _words;
		private int _currentWordIndex = 0;

		/// <summary>
		/// Fires when the user navigates to a new word from the wordlist
		/// </summary>
		public event EventHandler UpdateSourceWord;


		public GatherWordListTask(IRecordListManager recordListManager, string label, string description, string wordListFileName)
			: base(label, description, false, recordListManager)
		{
			_wordListFileName = wordListFileName;
			_words = null;
		}

		private void LoadWordList()
		{
			_words = new List<string>();
			string path = Path.Combine(WeSayWordsProject.Project.PathToWeSaySpecificFilesDirectoryInProject, _wordListFileName);
			if (!File.Exists(path))
			{
				path = Path.Combine(WeSayWordsProject.Project.ApplicationCommonDirectory, _wordListFileName);
			}
			using (TextReader r = File.OpenText(path))
			{
				do
				{
					string s = r.ReadLine();
					if (s == null)
					{
						break;
					}
					_words.Add(s);
				} while (true);
			}
		}


		public bool IsTaskComplete
		{
			get { return CurrentWordIndex >= _words.Count; }
		}

		/// <summary>
		/// The GatherWordListControl associated with this task
		/// </summary>
		/// <remarks>Non null only when task is activated</remarks>
		public override Control Control
		{
			get
			{
				if (_gatherControl==null)
			   {
				   _gatherControl = new GatherWordListControl(this);
			   }
			   return _gatherControl;
			}
		}

		public string CurrentWord
		{
			get {return _words[CurrentWordIndex]; }
		}

		public bool NavigateNextEnabled
		{
			get { return _words.Count > CurrentWordIndex ; }
		}

		public bool NavigatePreviousEnabled
		{
			get { return CurrentWordIndex > 0; }
		}

		protected int CurrentWordIndex
		{
			get { return _currentWordIndex; }
			set {
				_currentWordIndex = value;
				Debug.Assert(CurrentWordIndex >= 0);

				//nb: (CurrentWordIndex == _words.Count) is used to mark the "all done" state:

				if (this.UpdateSourceWord != null)
				{
					UpdateSourceWord.Invoke(this, null);
				}
			}
		}

		public override void Activate()
		{
			if (_words == null)
			{
				LoadWordList();
			}
			base.Activate();

		}

		public IList<LexEntry> GetMatchingRecords(MultiText gloss)
		{
			return Db4oLexQueryHelper.FindObjectsFromLanguageForm<LexEntry, SenseGlossMultiText>(this.RecordListManager, gloss.GetFirstAlternative());
		}


		/// <summary>
		/// Someday, we may indeed have multi-string foreign words
		/// </summary>
		public MultiText CurrentWordAsMultiText
		{
			get
			{
				MultiText m = new MultiText();
				m.SetAlternative(BasilProject.Project.WritingSystems.AnalysisWritingSystemDefaultId,
											  CurrentWord);
				return m;
			}
		}

		public void WordCollected(MultiText newVernacularWord, bool flagIsOn)
		{
			LexSense sense = new LexSense();
			sense.Gloss.MergeIn(CurrentWordAsMultiText);

			Db4oLexQueryHelper.AddSenseToLexicon(this.RecordListManager, newVernacularWord, sense);
			this.RecordListManager.GoodTimeToCommit();
		}

		public override void Deactivate()
		{
			base.Deactivate();
			if (_gatherControl != null)
			{
				_gatherControl.Dispose();
			}
			_gatherControl = null;
			this.RecordListManager.GoodTimeToCommit();
		}

		public void NavigatePrevious()
		{
			--CurrentWordIndex;
		}

		public void NavigateNext()
		{
			CurrentWordIndex++;
		}

		public void NavigateFirst()
		{
			CurrentWordIndex = 0;
		}
	}
}
