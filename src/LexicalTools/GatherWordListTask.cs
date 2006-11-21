using System.Collections;
using System.Collections.Generic;
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

		/// <summary>
		/// The GatherWordListControl associated with this task
		/// </summary>
		/// <remarks>Non null only when task is activated</remarks>
		public override Control Control
		{
			get
			{
				return _gatherControl;
			}
		}

		public override void Activate()
		{
			if (_words == null)
			{
				LoadWordList();
			}
			base.Activate();
			_gatherControl = new GatherWordListControl(this, _words);
		}

		public IList<LexEntry> GetMatchingRecords(MultiText gloss)
		{
			return Db4oLexQueryHelper.FindObjectsFromLanguageForm<LexEntry, SenseGlossMultiText>(this.RecordListManager, gloss.GetFirstAlternative());
		}

		public void WordCollected(MultiText word, MultiText gloss, bool flagIsOn)
		{
			LexSense sense = new LexSense();
			sense.Gloss.MergeIn(gloss);

			Db4oLexQueryHelper.AddSenseToLexicon(this.RecordListManager, word, sense);
			this.RecordListManager.GoodTimeToCommit();
		}



		public override void Deactivate()
		{
			base.Deactivate();
			_gatherControl.Dispose();
			_gatherControl = null;
			this.RecordListManager.GoodTimeToCommit();
		}
	}
}
