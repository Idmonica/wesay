using System.Windows.Forms;
using NUnit.Framework;
using NUnit.Extensions.Forms;
using WeSay.Data;
using WeSay.LexicalModel;
using WeSay.UI;

namespace WeSay.LexicalTools.Tests
{
	[TestFixture]
	public class EntryDetailControlTests : NUnit.Extensions.Forms.NUnitFormTest
	{
	   // private FormTester _mainWindowTester;
		private EntryDetailTask _task;
		IRecordListManager _recordListManager;
		string _filePath;
		private IRecordList<LexEntry> _records;
		private string _vernacularWsId;
		private TabControl _tabControl = new TabControl();
		private Form _window;

		public override void Setup()
		{
			base.Setup();

			BasilProject.InitializeForTests();
			this._vernacularWsId = BasilProject.Project.WritingSystems.VernacularWritingSystemDefault.Id;

			this._filePath = System.IO.Path.GetTempFileName();
			this._recordListManager = new Db4oRecordListManager(_filePath);

			this._records = this._recordListManager.Get<LexEntry>();
			AddEntry("Initial");
			AddEntry("Secondary");
			AddEntry("Tertiary");

			try
			{
				this._task = new EntryDetailTask(_recordListManager);
				this._task.Dock = DockStyle.Fill;
				TabPage detailTaskPage = new TabPage();
				detailTaskPage.Controls.Add(this._task);

				this._tabControl.TabPages.Add(detailTaskPage);
				this._tabControl.TabPages.Add("Dummy");
				this._window = new Form();
				this._window.Controls.Add(this._tabControl);
				this._window.Show();
				this._task.Activate();
			}
			catch
			{
				//_records.Dispose();
				_recordListManager.Dispose();
			}
		}

		private void AddEntry(string lexemeForm)
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm.SetAlternative(this._vernacularWsId, lexemeForm);
			this._records.Add(entry);
		}

		public override void TearDown()
		{
			_window.Close();
			_window.Dispose();
			_window = null;
			this._recordListManager.Dispose();
			_recordListManager = null;
			_records.Dispose();
			_records = null;
			System.IO.File.Delete(_filePath);
			base.TearDown();
		}

		[Test]
		public void ClickingAddWordIncreasesRecordsByOne()
		{
			int before = _records.Count;
			ClickAddWord();
			Assert.AreEqual(1 + before, _records.Count);
		}

		[Test]
		public void ClickingAddWordShowsEmptyRecord()
		{
			LexicalFormMustMatch("Initial");
			ClickAddWord();
			LexicalFormMustMatch("");
		}

		[Test]
		public void ClickingAddWordSelectsNewWordInList()
		{
			Assert.AreEqual("Initial", LexemeFormOfSelectedEntry);
			ClickAddWord();
			Assert.AreEqual("", LexemeFormOfSelectedEntry);
		}

		[Test]
		public void ClickingDeleteWordDereasesRecordsByOne()
		{
			int before = _records.Count;
			ClickDeleteWord();
			Assert.AreEqual(before-1, _records.Count);
		}



		[Test]
		public void DeletingFirstWordSelectsNextWordInList()
		{
			Assert.AreEqual("Initial", LexemeFormOfSelectedEntry);
			ClickDeleteWord();
			Assert.AreEqual("Secondary", LexemeFormOfSelectedEntry);
		}

		[Test]
		public void DeletingLastWordSelectsPreviousWordInList()
		{
			BindingListGridTester t = new BindingListGridTester("_recordsListBox");
			t.Properties.SelectedIndex = 2;
			Assert.AreEqual("Tertiary", LexemeFormOfSelectedEntry);
			ClickDeleteWord();
			Assert.AreEqual("Secondary", LexemeFormOfSelectedEntry);
		}

		[Test]
		public void AddWordsThenDeleteDoesNotCrash()
		{
			ClickAddWord();
			ClickDeleteWord();
		}

		[Test]
		public void DeletingAllWordsThenAddingDoesNotCrash()
		{
			ClickDeleteWord();
			ClickDeleteWord();
			ClickAddWord();
		}

		[Test]
		public void IfNoWordsDeleteButtonDisabled()
		{
			NUnit.Extensions.Forms.LinkLabelTester l = new LinkLabelTester("_btnDeleteWord");
			Assert.IsTrue(l.Properties.Enabled);
			ClickDeleteWord();
			ClickDeleteWord();
			ClickDeleteWord();
			Assert.IsFalse(l.Properties.Enabled);
		}

		[Test]
		public void SwitchingToAnotherTaskDoesNotLooseBindings()
		{
			 NUnit.Extensions.Forms.TextBoxTester t = new TextBoxTester("LexicalForm");
		   LexicalFormMustMatch("Initial");
			TypeInLexicalForm("one");
			this._task.Deactivate();
			t.Properties.Visible = false;
			_tabControl.SelectedIndex = 1;
			_tabControl.SelectedIndex = 0;
			this._task.Activate();
			t.Properties.Visible = true;

			LexicalFormMustMatch("one");

			t = new TextBoxTester("LexicalForm");
			TypeInLexicalForm("two");
			this._task.Deactivate();
			t.Properties.Visible = false;
			_tabControl.SelectedIndex = 1;
			_tabControl.SelectedIndex = 0;
			this._task.Activate();
			t.Properties.Visible = true;
			LexicalFormMustMatch("two");
		}

		private static void LexicalFormMustMatch(string value)
		{
			NUnit.Extensions.Forms.TextBoxTester t = new TextBoxTester("LexicalForm");
			Assert.AreEqual(value, t.Properties.Text);
		}

		private static void TypeInLexicalForm(string value)
		{
			NUnit.Extensions.Forms.TextBoxTester t = new TextBoxTester("LexicalForm");
			t.Properties.Text = value;
		}

		private string LexemeFormOfSelectedEntry
		{
			get
			{
				BindingListGridTester t = new BindingListGridTester("_recordsListBox");
				return ((LexEntry)t.Properties.SelectedObject).LexicalForm.GetAlternative(_vernacularWsId);
			}
		}

		private static void ClickAddWord()
		{
			NUnit.Extensions.Forms.LinkLabelTester l = new LinkLabelTester("_btnNewWord");
			l.Click();
		}

		private void ClickDeleteWord()
		{
			NUnit.Extensions.Forms.LinkLabelTester l = new LinkLabelTester("_btnDeleteWord");
			l.Click();
		}
	}
}
