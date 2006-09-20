using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WeSay.LexicalModel;
using WeSay.UI;

namespace WeSay.LexicalTools
{
	public partial class EntryDetailTask : UserControl,ITask
	{

		private IBindingList _records;
		private int _currentIndex;

		public EntryDetailTask(IBindingList records)
		{
			InitializeComponent();
			_records = records;//new WeSay.Data.InMemoryBindingList<LexEntry>();

			_entryDetailPanel.BackColor = SystemColors.Control;//we like it to stand out at design time, but not runtime
		}

		void OnRecordSelectionChanged(object sender, EventArgs e)
		{
			_currentIndex = _recordsListBox.SelectedIndex;
			_entryDetailPanel.DataSource = CurrentRecord;
		}


		public void Activate()
		{
			_recordsListBox.DataSource = _records;
			_entryDetailPanel.DataSource = CurrentRecord;
			_recordsListBox.SelectedIndexChanged += new EventHandler(OnRecordSelectionChanged);

			_recordsListBox.Font = BasilProject.Project.WritingSystems.VernacularWritingSystemDefault.Font;
			_recordsListBox.AutoSize();
			_recordsListBox.Columns.StretchToFit();
		}

		public void Deactivate()
		{
			_recordsListBox.SelectedIndexChanged -= new EventHandler(OnRecordSelectionChanged);
		}

		public string Label
		{
			get
			{
				return "Words";
			}
		}

		public string Description
		{
			get
			{
				return "Edit all relevant fields for lexical entries.";
			}
		}

		public Control Control
		{
			get { return this; }
		}

		public IBindingList DataSource
		{
			get { return _records;}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				_records = value;
			}

		}

		private LexEntry CurrentRecord
		{
			get
			{
				if (_records.Count == 0)
				{
					return null;
				}
				return _records[_currentIndex] as LexEntry;
			}
		}

		private void btnDelete_Click(object sender, EventArgs e)
		{

		}
	}
}
