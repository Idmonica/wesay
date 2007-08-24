using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WeSay.Data;
using WeSay.Foundation;
using WeSay.Language;
using WeSay.LexicalModel;
using WeSay.LexicalModel.Db4o_Specific;
using WeSay.LexicalTools.Properties;
using WeSay.Project;
using Debug=System.Diagnostics.Debug;

namespace WeSay.LexicalTools
{
	public partial class DictionaryControl : UserControl
	{
		private IBindingList _records;
		private WritingSystem _listWritingSystem;
		private ContextMenu _cmWritingSystems;

		private Db4oRecordListManager _recordManager;

		private readonly ViewTemplate _viewTemplate;
		public event EventHandler SelectedIndexChanged = delegate {};
		private bool _recordListBoxActive;

		public DictionaryControl()
		{
			Debug.Assert(DesignMode);
			InitializeComponent();
		}

		public DictionaryControl(IRecordListManager recordManager, ViewTemplate viewTemplate)
		{
			if (recordManager == null)
			{
				throw new ArgumentNullException("recordManager");
			}
			if (viewTemplate == null)
			{
				throw new ArgumentNullException("viewTemplate");
			}
			_viewTemplate = viewTemplate;
			_recordManager = (Db4oRecordListManager)recordManager;

			WritingSystem listWritingSystem = null;
			_cmWritingSystems = new ContextMenu();

			Field field = viewTemplate.GetField(Field.FieldNames.EntryLexicalForm.ToString());
			if (field != null)
			{
				if (field.WritingSystems.Count > 0)
				{
					listWritingSystem = field.WritingSystems[0];
					foreach (WritingSystem writingSystem in field.WritingSystems)
					{
						RegisterWritingSystemAndField(field, writingSystem);
					}
				}
				else
				{
					MessageBox.Show(String.Format("There are no writing systems enabled for the Field '{0}'", field.FieldName), "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);//review
				}
			}
			Field glossfield = viewTemplate.GetField(Field.FieldNames.SenseGloss.ToString());
			if (glossfield != null)
			{
				foreach (WritingSystem writingSystem in glossfield.WritingSystems)
				{
					RegisterWritingSystemAndField(glossfield, writingSystem);
				}
			}

			if (listWritingSystem == null)
			{
				listWritingSystem = BasilProject.Project.WritingSystems.UnknownVernacularWritingSystem;
			}

			InitializeDisplaySettings();
			InitializeComponent();
			InitializeMonoWorkarounds();


			//this._btnNewWord.Font = StringCatalog.ModifyFontForLocalization(_btnNewWord.Font);
			//this._btnDeleteWord.Font = StringCatalog.ModifyFontForLocalization(_btnDeleteWord.Font);

			this._writingSystemChooser.Image = Resources.Expand.GetThumbnailImage(6, 6, ReturnFalse, IntPtr.Zero);
			this._btnFind.Image = Resources.Find.GetThumbnailImage(18, 18, ReturnFalse, IntPtr.Zero);
			this._btnDeleteWord.Image = Resources.DeleteWord.GetThumbnailImage(18, 18, ReturnFalse, IntPtr.Zero);
			this._btnNewWord.Image = Resources.NewWord.GetThumbnailImage(18, 18, ReturnFalse, IntPtr.Zero);

			Control_EntryDetailPanel.ViewTemplate = _viewTemplate;

			SetListWritingSystem(listWritingSystem);

			_findText.KeyDown += new KeyEventHandler(_findText_KeyDown);
			_recordsListBox.SelectedIndexChanged += new EventHandler(OnRecordSelectionChanged);
			_recordsListBox.Enter += new EventHandler(_recordsListBox_Enter);
			_recordsListBox.Leave += new EventHandler(_recordsListBox_Leave);
			_btnDeleteWord.Enabled = (CurrentRecord != null);
		}

	   private void InitializeMonoWorkarounds() { // Mono bug 82081
#if !MONO
			if (Type.GetType("Mono.Runtime") != null)
#endif
		   {
			   // this is not allowed in .Net but it is in Mono and is the only way to get
			   // it to work (setting width to 0 doesn't)
			   this._btnFind.FlatAppearance.BorderColor = System.Drawing.Color.Transparent;
			   this._writingSystemChooser.FlatAppearance.BorderColor = System.Drawing.Color.Transparent;
			   this._btnDeleteWord.FlatAppearance.BorderColor = System.Drawing.Color.Transparent;
			   this._btnNewWord.FlatAppearance.BorderColor = System.Drawing.Color.Transparent;
		   }
		}

		private void InitializeDisplaySettings() {
			BackColor = WeSay.UI.DisplaySettings.Default.BackgroundColor;
		}

		private void RegisterWritingSystemAndField(Field field, WritingSystem writingSystem) {
			MenuItem item =
					new MenuItem(writingSystem.Id + "\t" + StringCatalog.Get(field.DisplayName),
								 OnCmWritingSystemClicked);
			item.RadioCheck = true;
			item.Tag = writingSystem;
			this._cmWritingSystems.MenuItems.Add(item);


			LexEntrySortHelper sortHelper = new LexEntrySortHelper(_recordManager.DataSource,
													   writingSystem.Id,
													   IsWritingSystemUsedInLexicalForm(writingSystem));
			_recordManager.GetSortedList(sortHelper);

		}

		private bool IsWritingSystemUsedInLexicalForm(WritingSystem writingSystem)
		{
			Field field = _viewTemplate.GetField(Field.FieldNames.EntryLexicalForm.ToString());
			if (field != null)
			{
				return field.WritingSystems.Contains(writingSystem);
			}
			return false;
		}


		public void SetListWritingSystem(WritingSystem writingSystem) {
			if(writingSystem == null)
			{
				throw new ArgumentNullException();
			}
			if(_listWritingSystem == writingSystem)
			{
				return;
			}
			_listWritingSystem = writingSystem;

			LexEntrySortHelper sortHelper = new LexEntrySortHelper(_recordManager.DataSource,
																   this._listWritingSystem.Id,
																   IsWritingSystemUsedInLexicalForm(_listWritingSystem));
			this._records = _recordManager.GetSortedList(sortHelper);

			this._recordsListBox.DataSource = this._records;

			 Control_EntryDetailPanel.AllRecords = _records;
		   Control_EntryDetailPanel.DataSource = CurrentRecord;
			this._recordsListBox.WritingSystem = this._listWritingSystem;

			this._findText.ItemFilterer = ApproximateMatcher.FindClosestAndNextClosestAndPrefixedForms;
//            this._findText.Items = (CachedSortedDb4oList<string, LexEntry>)this._records;
			this._findText.Items = new WeSay.Foundation.StringToObjectEnumerableWrapper((CachedSortedDb4oList<string, LexEntry>)this._records);
			int originalHeight = this._findText.Height;
			this._findText.WritingSystem = this._listWritingSystem;

			this._findWritingSystemId.Text = this._listWritingSystem.Id;
			int width = this._findWritingSystemId.Width;
			this._findWritingSystemId.AutoSize = false;
			this._findWritingSystemId.Height = this._findText.Height;
			this._findWritingSystemId.Width = Math.Min(width, 25);

			int heightDifference = this._findText.Height - originalHeight;
			this._recordsListBox.Location = new Point(this._recordsListBox.Location.X,
												 this._recordsListBox.Location.Y + heightDifference);
			this._recordsListBox.Height -= heightDifference;
			this._btnFind.Height = this._findText.Height;
			this._writingSystemChooser.Height = this._findText.Height;
			this._btnFind.Image = Resources.Find.GetThumbnailImage(this._btnFind.Width - 2,
																   this._btnFind.Width - 2,
																   ReturnFalse, IntPtr.Zero);

			this._btnFind.Left = this._writingSystemChooser.Left - this._btnFind.Width;
			this._findText.Width = this._btnFind.Left - _findText.Left;
			this._findText.PopupWidth = this._recordsListBox.Width;
		}

		void OnCmWritingSystemClicked(object sender, EventArgs e)
		{
			MenuItem item = (MenuItem) sender;
			if (_listWritingSystem != item.Tag)
			{
				SetListWritingSystem((WritingSystem)item.Tag);
			}
		}

		void OnWritingSystemChooser_Click(object sender, EventArgs e)
		{
			Palaso.Reporting.Logger.WriteMinorEvent("WritingSystemChooser_Click");

			foreach (MenuItem menuItem in _cmWritingSystems.MenuItems)
			{
			  menuItem.Checked = (this._listWritingSystem == menuItem.Tag);
			}
			this._cmWritingSystems.Show(_writingSystemChooser, new Point(_writingSystemChooser.Width,_writingSystemChooser.Height));
		}

		void OnFindWritingSystemId_MouseClick(object sender, MouseEventArgs e)
		{
			Palaso.Reporting.Logger.WriteMinorEvent("FindWritingSystemId_MouseClick");
			_findText.Focus();
		}

		// primarily for testing
		public EntryViewControl Control_EntryDetailPanel
		{
			get
			{
				return this._entryViewControl;
			}
		}

		void _findText_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.Modifiers == Keys.None &&
			   e.KeyData == Keys.Enter)
			{
				e.Handled = true;
				if (Type.GetType("Mono.Runtime") == null) // Work around not yet implemented in Mono
				{
					SetSuppressKeyPressTrue(e);
				}
				Find(this._findText.Text);
			}
		}

		private static void SetSuppressKeyPressTrue(KeyEventArgs e) {
#if !MONO
			e.SuppressKeyPress = true; // otherwise it beeps!
#endif
		}

		void _findText_AutoCompleteChoiceSelected(object sender, EventArgs e)
		{
			Find(this._findText.Text);
		}

		void OnFind_Click(object sender, EventArgs e)
		{
			Palaso.Reporting.Logger.WriteMinorEvent("FindButton_Click");

			Find(this._findText.Text);
		}

		private void Find(string text)
		{
			Palaso.Reporting.Logger.WriteMinorEvent("Find");
			int index = ((CachedSortedDb4oList<string, LexEntry>) _records).BinarySearch(text);
			if (index < 0)
			{
				index = ~index;
				if (index == _records.Count && index != 0)
				{
					index--;
				}
			}
			if(0 <= index && index < _recordsListBox.Items.Count)
			{
				_recordListBoxActive = true;// allow onRecordSelectionChanged
				_recordsListBox.SelectedIndex = index;
				_recordListBoxActive = false;

			}
		}


		void _recordsListBox_Enter(object sender, EventArgs e)
		{
			_recordListBoxActive = true;
		}

		void _recordsListBox_Leave(object sender, EventArgs e)
		{
			_recordListBoxActive = false;
		}


		void OnRecordSelectionChanged(object sender, EventArgs e)
		{
			if (Control_EntryDetailPanel.DataSource == CurrentRecord)
			{
				//we were getting 3 calls to this for each click on a new word
				return;
			}
			_btnDeleteWord.Enabled = (CurrentRecord != null);
			if (!_recordListBoxActive)
			{
				// When we change the content of the displayed string,
				// Windows.Forms.ListBox removes the item (and sends
				// the new selection event) then adds it in to the right
				// place (and sends the new selection event again)
				// We don't want to know about this case
				// We only want to know about the case where the user
				// has selected a record in the list box itself (so has to enter
				// the list box first)
				return;
			}

			if (CurrentRecord != null)
			{
				Palaso.Reporting.Logger.WriteEvent("RecordSelectionChanged to " +
											CurrentRecord.LexicalForm.GetFirstAlternative());
				Control_EntryDetailPanel.DataSource = CurrentRecord;
			}
			else
			{
				Palaso.Reporting.Logger.WriteEvent("RecordSelectionChanged Skipping because record is null");
			}

			SelectedIndexChanged.Invoke(this, null);
		}

		public LexEntry CurrentRecord
		{
			get
			{
				if (_records.Count == 0 || CurrentIndex == -1)
				{
					return null;
				}
				return ((CachedSortedDb4oList<string, LexEntry>)_records).GetValue(CurrentIndex);
			}
		}

		protected int CurrentIndex
		{
			get
			{
			   return _recordsListBox.SelectedIndex;
			}
		}

		private void OnNewWord_Click(object sender, EventArgs e)
		{
			Palaso.Reporting.Logger.WriteEvent("NewWord_Click");

			if (!this._btnNewWord.Focused)
			{
				// if we use a hot key, it may not have received the focus
				// but we assume it has the focus when we do our selection change event
				this._btnNewWord.Focus();
			}
			LexEntry entry = new LexEntry();
			bool NoPriorSelection = _recordsListBox.SelectedIndex == -1;
			_recordListBoxActive = true;// allow onRecordSelectionChanged
			_records.Add(entry);
			_recordsListBox.SelectedIndex = _records.IndexOf(entry);
			if(NoPriorSelection)
			{
				// Windows.Forms.Listbox does not consider it a change of Selection
				// index if the index was -1 and a record is added.
				// (No event is sent so we must do it ourselves)
				OnRecordSelectionChanged(this, null);
			}
			_recordListBoxActive = false;

			_entryViewControl.Focus();
		}

		private void OnDeleteWord_Click(object sender, EventArgs e)
		{
			Palaso.Reporting.Logger.WriteEvent("DeleteWord_Clicked");

			Debug.Assert(CurrentIndex >= 0);
			if(CurrentIndex == -1)
			{
				return;
			}
			if (!this._btnDeleteWord.Focused)
			{
				// if we use a hot key, it may not have received the focus
				// but we assume it has the focus when we do our selection change event
				this._btnDeleteWord.Focus();
			}
			_recordListBoxActive = true; // allow onRecordSelectionChanged
			_records.RemoveAt(CurrentIndex);
			OnRecordSelectionChanged(this, null);
			_recordListBoxActive = false;

			if (CurrentRecord == null)
			{
				Control_EntryDetailPanel.DataSource = CurrentRecord;
			}

			_entryViewControl.Focus();
		}

	}
}
