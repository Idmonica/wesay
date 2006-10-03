using System;
using System.Collections.Generic;
using WeSay.UI;
using System.Windows.Forms;

namespace WeSay.App
{
	public partial class TabbedForm : Form, ICurrentWorkTask
	{
		private ITask _activeTask;
		private TabPage _currentWorkTab;
		private Timer _pacifierTimer;

		public TabbedForm()
		{
			InitializeComponent();

			this.tabControl1.TabPages.Clear();

			this.tabControl1.SelectedIndexChanged += new System.EventHandler(tabControl1_SelectedIndexChanged);
		}

		public void InitializeTasks(IList<ITask> taskList)
		{
			TabPage selectedTab = null;
			foreach (ITask t in taskList)
			{
				if (t.IsPinned)
				{
					selectedTab = CreateTabPageForTask(t);
				}
			}

			foreach (ITask t in taskList)
			{
				if (CurrentWorkTask == null && !t.IsPinned)
				{
					_currentWorkTab = new TabPage();
					this.tabControl1.TabPages.Add(_currentWorkTab);
					selectedTab = _currentWorkTab;
					CurrentWorkTask = t; // default to first non-pinned task.
					break;
				}
			}

			if (selectedTab != null)
			{
				this.tabControl1.SelectedTab = selectedTab;
				ActivateTab(selectedTab);
			}

		}

		private TabPage CreateTabPageForTask(ITask t)
		{
			//t.Container = container;
			TabPage page = new TabPage(StringCatalog.Get(t.Label));
			page.Tag = t;
			this.tabControl1.TabPages.Add(page);
			return page;
		}

		public ITask ActiveTask
		{
			set
			{
				foreach (TabPage page in this.tabControl1.TabPages)
				{
					if (page.Tag == value)
					{
						ActivateTab(page);
						break;
					}
				}
			}
		}

		public ITask CurrentWorkTask
		{
			get
			{
				if (this._currentWorkTab == null)
				{
					return null;
				}
				return (ITask)this._currentWorkTab.Tag;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				if (value.IsPinned)
				{
					throw new ArgumentOutOfRangeException("A work task cannot be a pinned task.");
				}
				_currentWorkTab.Tag = value;
				_currentWorkTab.Text = StringCatalog.Get(value.Label);
				this.tabControl1.SelectedTab = _currentWorkTab;
				ActivateTab(_currentWorkTab);
			}
		}

		public IList<string> TabLabels
		{
			get
			{
				IList<string> labels = new List<string>();
				foreach (TabPage page in this.tabControl1.TabPages)
				{
					labels.Add(page.Text);
				}
				return labels;
			}
		}

		void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			TabPage page = ((TabControl)sender).SelectedTab;
			ActivateTab(page);
		}

		private void ActivateTab(TabPage page)
		{
			ITask task = (ITask)page.Tag;
			if (_activeTask == task)
				return; //debounce

			if (_activeTask != null)
				_activeTask.Deactivate();
			if (task != null)
			{
				page.Cursor = Cursors.WaitCursor;
				page.Controls.Clear();
				if (this.Visible)
				{
					ActivateAfterScreenDraw(page, task);
				}
				else
				{
					ActivateTask(page, task);
				}
			}
			_activeTask = task;
		}

		private void ActivateAfterScreenDraw(TabPage page, ITask task)
		{
			string previousTabText = page.Text;
			page.Text += " Loading...";
			System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
			t.Tick+=new EventHandler(delegate
										   {
											   ActivateTask(page, task);
												 t.Enabled = false;
												page.Text = previousTabText;
										 });
			t.Interval = 1;
			t.Enabled = true;

		}

		void ActivateTask(TabPage page, ITask task)
		{
			task.Activate();
			task.Control.Dock = DockStyle.Fill;
			page.Controls.Add(task.Control);
			page.Cursor = Cursors.Default;
		}


		private void ContinueLaunchingAfterInitialDisplay()
		{
			System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
			t.Tick += new EventHandler(delegate
										   {
											   InitializeTasks(WeSayWordsProject.Project.Tasks);
											   t.Enabled = false;
										   });
			t.Interval = 1;
			t.Enabled = true;
		}

		private void TabbedForm_Load(object sender, EventArgs e)
		{
			ContinueLaunchingAfterInitialDisplay();
		}
	}
}
