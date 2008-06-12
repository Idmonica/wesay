using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Palaso.UI.WindowsForms.i8n;
using Palaso.Reporting;
using WeSay.AddinLib;
using WeSay.Foundation.Dashboard;
using WeSay.LexicalModel;
using WeSay.Project;
using WeSay.UI;
using System.Drawing.Drawing2D;

namespace WeSay.CommonTools
{

	public partial class Dash : UserControl, ITask, IFinishCacheSetup
	{
		private readonly LexEntryRepository _lexEntryRepository;
		private int _standardButtonWidth;
		private IList<IThingOnDashboard> _thingsToMakeButtonsFor;
		private List<ButtonGroup> _buttonGroups;
		private bool _isActive=false;
		private readonly ICurrentWorkTask _currentWorkTaskProvider;
		private int _oldFlowWidth;

		public Dash(LexEntryRepository RecordListManager, ICurrentWorkTask currentWorkTaskProvider)
		{
			_oldFlowWidth = 0;
			_lexEntryRepository = RecordListManager;
			_currentWorkTaskProvider = currentWorkTaskProvider;
			InitializeContextMenu();
		}

		private void InitializeContextMenu()
		{
			ContextMenu = new ContextMenu();
			ContextMenu.MenuItems.Add("Configure this project...", OnRunConfigureTool);
			ContextMenu.MenuItems.Add("Use projector-friendly colors", OnToggleColorScheme);
			ContextMenu.MenuItems[1].Checked = DisplaySettings.Default.UsingProjectorScheme;
		}

		private void OnToggleColorScheme(object sender, EventArgs e)
		{
			DisplaySettings.Default.ToggleColorScheme();
			ContextMenu.MenuItems[1].Checked = DisplaySettings.Default.UsingProjectorScheme;
			Invalidate(true);
		}

		private static void OnRunConfigureTool(object sender, EventArgs e)
		{
			string dir = Directory.GetParent(Application.ExecutablePath).FullName;
			ProcessStartInfo startInfo =
					new ProcessStartInfo(Path.Combine(dir, "WeSay Configuration Tool.exe"),
										 string.Format("\"{0}\"", WeSayWordsProject.Project.PathToConfigFile));
			try
			{
				Process.Start(startInfo);
			}
			catch
			{
				ErrorReport.ReportNonFatalMessage("Could not start " + startInfo.FileName);
				return;
			}

			Application.Exit();
		}

		private int DetermineStandardButtonWidth()
		{
			int maxRequestedWidth = 30;
			foreach (IThingOnDashboard item in ThingsToMakeButtonsFor)
			{
				int w = 100;// item.WidthToDisplayFullSizeLabel;
				if(w > maxRequestedWidth)
					maxRequestedWidth = w;
			}
			return maxRequestedWidth;
		}

		private void Fill()
		{
			DictionaryStatusControl title = new DictionaryStatusControl(_lexEntryRepository.CountAllEntries());
			title.Font = new Font("Arial", 14);
			title.BackColor = Color.Transparent;
			title.ShowLogo = true;
			_flow.Controls.Add(title);

			_standardButtonWidth = DetermineStandardButtonWidth();
			_standardButtonWidth += 30;//for space between text and button

			foreach (ButtonGroup group in _buttonGroups)
			{
				if (!group.MakeButtonsSameSize)
				{
					AddButtonGroup(group, 0);
				}
				else
				{
					AddButtonGroup(group, _standardButtonWidth);
				}
			}
		 }

//        private IEnumerable<string> GetGroups()
//        {
//            List<string> foundGroups = new List<string>();
//            foreach (IThingOnDashboard item in _thingsToMakeButtonsFor)
//            {
//                if (!foundGroups.Contains(item.GroupName))
//                {
//                    foundGroups.Add(item.GroupName);
//                    yield return item.GroupName;
//                }
//            }
//        }

		private void AddButtonGroup(ButtonGroup buttonGroup, int buttonWidth)
		{
			FlowLayoutPanel buttonFlow = new FlowLayoutPanel();
			buttonFlow.AutoSize = true;
			buttonFlow.FlowDirection = FlowDirection.LeftToRight;
		   // buttonGroup.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			buttonFlow.Margin =  new Padding(30,0,0,15);
			buttonFlow.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			buttonFlow.WrapContents = true;
			bool foundAtLeastOne = false;
			foreach (IThingOnDashboard item in ThingsToMakeButtonsFor)
			{
				if (item == this)
					continue;
				if (item.Group == buttonGroup.Group)
				{
					buttonFlow.Controls.Add(MakeButton(item, buttonWidth, buttonGroup));
					foundAtLeastOne = true;
				}
			}
			if (foundAtLeastOne)
			{
				Label header = new Label();
				header.Text = StringCatalog.Get(buttonGroup.Group.ToString());
				header.Font = new Font("Arial", 12);
				_flow.Controls.Add(header);
				_flow.Controls.Add(buttonFlow);
			}
		}

		private Control MakeButton(IThingOnDashboard item, int buttonWidth, ButtonGroup group)
		{
			DashboardButton button = MakeButton(item);
			button.BackColor = Color.Transparent;
			button.Font = this.Font;
			button.AutoSize = false;
			button.BorderColor = group.BorderColor;
			button.DoneColor = group.DoneColor;
//            if (buttonWidth == 0)
//                buttonWidth = item.WidthToDisplayFullSizeLabel;

			button.Size = new Size(buttonWidth, 40);
			button.Text = item.LocalizedLabel;
			button.Click += OnButtonClick;
			return button;
		}

		void OnButtonClick(object sender, EventArgs e)
		{
			DashboardButton b = (DashboardButton)sender;
			ITask task = b.ThingToShowOnDashboard as ITask;
			if (task != null && _currentWorkTaskProvider != null)
			{
				_currentWorkTaskProvider.ActiveTask = task;
			}
			else
			{
				IWeSayAddin addin = b.ThingToShowOnDashboard as IWeSayAddin;
				if (addin != null)
				{
					Cursor.Current = Cursors.WaitCursor;

					try
					{
						ProjectInfo projectInfo = WeSayWordsProject.Project.GetProjectInfoForAddin(_lexEntryRepository);
						addin.Launch(ParentForm, projectInfo);
					}
					catch (Exception error)
					{
						Palaso.Reporting.ErrorReport.ReportNonFatalMessage(error.Message);
					}

					Cursor.Current = Cursors.Default;

				}
			}
		}


		public DashboardButton MakeButton(IThingOnDashboard item)
		{
			switch (item.DashboardButtonStyle)
			{
				case ButtonStyle.FixedAmount:
					return new DashboardButton(item);
				case ButtonStyle.VariableAmount:
					return new DashboardButton(item);
				case ButtonStyle.IconFixedWidth:
					return new DashboardButtonWithIcon(item);
				case ButtonStyle.IconVariableWidth:
					return new DashboardButtonWithIcon(item);
				default:
					return new DashboardButton(item);
			}
		}

		private void ResizeFlows()
		{
			_flow.SuspendLayout();
			foreach (Control control in _flow.Controls)
			{
				FlowLayoutPanel buttonGroup = control as FlowLayoutPanel;
				if (buttonGroup == null)
				{
					continue;
				}
				buttonGroup.MaximumSize = new Size(_flow.Width - buttonGroup.Margin.Left - buttonGroup.Margin.Right, 0);
			}
			_flow.Height = _flow.GetPreferredSize(new Size(_flow.Width, 0)).Height;
			_flow.ResumeLayout();
		}

		#region ITask Members

		const int CountNotRelevant = -1;

		public int GetReferenceCount()
		{
			return CountNotRelevant;
		}

		public bool IsPinned
		{
			get { return true; }
		}

		public int GetRemainingCount()
		{
			return CountNotRelevant;
		}

		public int ExactCount
		{
			get { return CountNotRelevant; }
		}

		public void Activate()
		{
			if (IsActive)
			{
				throw new InvalidOperationException("Activate should not be called when object is active.");
			}

			Initialize();
			SuspendLayout();
			if (ThingsToMakeButtonsFor == null)
			{
				ThingsToMakeButtonsFor = new List<IThingOnDashboard>();
				foreach (ITask task in WeSayWordsProject.Project.Tasks)
				{
					ThingsToMakeButtonsFor.Add(task);
				}
				foreach (IWeSayAddin action in AddinSet.GetAddinsForUser())
				{
					ThingsToMakeButtonsFor.Add(action);
				}
			}


			Fill();
			ResumeLayout(true);
			_isActive = true;
		}

		private void Initialize()
		{
			InitializeComponent();
			this.BackColor = DisplaySettings.Default.GetEndBackgroundColor(this);

			//_flow.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

			_buttonGroups = new List<ButtonGroup>();
			_buttonGroups.Add(new ButtonGroup(DashboardGroup.Gather, true,
											  Color.FromArgb(155, 187, 89),
											  Color.FromArgb(195, 214, 155)));
			_buttonGroups.Add(new ButtonGroup(DashboardGroup.Describe, true, Color.FromArgb(85, 142, 213),
											  Color.FromArgb(185, 205, 229)));
			_buttonGroups.Add(new ButtonGroup(DashboardGroup.Refine, true, Color.FromArgb(250, 192, 144),
											  Color.FromArgb(252, 213, 181)));
			_buttonGroups.Add(new ButtonGroup(DashboardGroup.Share, true, Color.FromArgb(119, 147, 60),
											  Color.White));

			LocalizationHelper helper = new LocalizationHelper(null);
			helper.Parent = this;
			helper.EndInit();
		}

		public void Deactivate()
		{
			if (!IsActive)
			{
				throw new InvalidOperationException("Deactivate should only be called once after Activate.");
			}
			Controls.Clear();
			_isActive = false;
		}

		public void GoToUrl(string url)
		{
		}

		public bool IsActive
		{
			get { return _isActive; }
		}

		public string Label
		{
			get
			{
				return
						StringCatalog.Get("~Home",
										  "The label for the 'dashboard'; the task which lets you see the status of other tasks and jump to them.");
			}
		}

		public string Description
		{
			get { return StringCatalog.Get("~Switch tasks and see current status of tasks"); }
		}

		public bool MustBeActivatedDuringPreCache
		{
			get { return false; }
		}

		public Control Control
		{
			get { return this; }
		}

		#region IThingOnDashboard Members

		public DashboardGroup Group
		{
			get { return DashboardGroup.DontShow; }
		}

		public string LocalizedLabel
		{
			get { throw new NotImplementedException(); }
		}

		public ButtonStyle DashboardButtonStyle
		{
			get { throw new NotImplementedException(); }
		}

		public Image DashboardButtonImage
		{
			get { throw new NotImplementedException(); }
		}

		public IList<IThingOnDashboard> ThingsToMakeButtonsFor
		{
			get { return _thingsToMakeButtonsFor; }
			set { _thingsToMakeButtonsFor = value; }
		}

		#endregion

		#endregion

		#region IFinishCacheSetup Members

		public void FinishCacheSetup()
		{
			Activate();
			Deactivate();
		}

		#endregion

		protected override void OnLayout(LayoutEventArgs e)
		{
			base.OnLayout(e);
			Invalidate(false);   // force redraw of background
			if (_flow.Width == _oldFlowWidth)
				return;
			_oldFlowWidth = _flow.Width;
			bool neededScroll = _flow.Bounds.Bottom >= ClientRectangle.Height;
			ResizeFlows();
			// If we need a scrollbar now, and we didn't before, do another layout
			// to add the scrollbar.  This prevents some problems when resizing
			if (!neededScroll && _flow.Bounds.Bottom >= ClientRectangle.Height)
				base.OnLayout(e);
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			DisplaySettings.Default.PaintBackground(this, e);
		}

		protected override void OnScroll(ScrollEventArgs se)
		{
			base.OnScroll(se);
			Invalidate(false);  // force redraw of background
		}
	}





	internal class ButtonGroup
	{
		private readonly DashboardGroup _group;
		private readonly bool _makeButtonsSameSize;
		private Color _doneColor;
		private Color _borderColor;

		public ButtonGroup(DashboardGroup group, bool makeButtonsSameSize, Color borderColor, Color doneColor)
		{
			_group = group;
			_makeButtonsSameSize = makeButtonsSameSize;
			_borderColor = borderColor;
			_doneColor = doneColor;
		}

		public bool MakeButtonsSameSize
		{
			get { return _makeButtonsSameSize; }
		}



		public Color DoneColor
		{
			get { return _doneColor; }
		}

		public Color BorderColor
		{
			get { return _borderColor; }
		}

		public DashboardGroup Group
		{
			get { return _group; }
		}
	}
}
