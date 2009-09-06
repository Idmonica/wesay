using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Chorus.VcsDrivers.Mercurial;
using Palaso.UI.WindowsForms.i8n;
using WeSay.ConfigTool.Properties;

namespace WeSay.ConfigTool
{
	public partial class WelcomeControl: UserControl
	{
		public event EventHandler NewProjectClicked;
		public event EventHandler NewProjectFromFlexClicked;
		public Action<string> OpenSpecifiedProject;
		public event EventHandler ChooseProjectClicked;

		public WelcomeControl()
		{
			Font = SystemFonts.MessageBoxFont;//use the default OS UI font
			InitializeComponent();
		  }

		private void LoadButtons()
		{
			flowLayoutPanel1.Controls.Clear();
			var createAndGetGroup = new TableLayoutPanel();
			createAndGetGroup.AutoSize = true;
			AddCreateChoices(createAndGetGroup);
			AddGetChoices(createAndGetGroup);

			var openChoices = new TableLayoutPanel();
			openChoices.AutoSize = true;
			AddSection("Open", openChoices);
			AddOpenProjectChoices(openChoices);
			flowLayoutPanel1.Controls.AddRange(new Control[] { createAndGetGroup, openChoices });
		}

		private void AddSection(string sectionName, TableLayoutPanel panel)
		{
			 panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			panel.RowCount++;
			Label label = new Label();
			label.Font = new Font(StringCatalog.LabelFont.FontFamily, _templateLabel.Font.Size, _templateLabel.Font.Style);
			label.ForeColor = _templateLabel.ForeColor;
			label.Text = sectionName;
			label.Margin = new Padding(0, 20, 0, 0);
			panel.Controls.Add(label);
		}

		private void AddFileChoice(string path, TableLayoutPanel panel)
		{
			var button = AddChoice(Path.GetFileNameWithoutExtension(path), path, "wesayProject", true, openRecentProject_LinkClicked, panel);
			button.Tag = path;
		}


		private Button AddChoice(string localizedLabel, string localizedTooltip, string imageKey, bool enabled,
   EventHandler clickHandler, TableLayoutPanel panel)
		{
			panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			panel.RowCount++;
			Button button = new Button();
			button.Anchor = AnchorStyles.Top | AnchorStyles.Left;

			button.Width = _templateButton.Width;//review
			button.Font = new Font(StringCatalog.LabelFont.FontFamily, _templateButton.Font.Size, _templateButton.Font.Style);
			button.ImageKey = imageKey;
			button.ImageList = _imageList;
			button.ImageAlign = ContentAlignment.MiddleLeft;
			button.Click += clickHandler;
			button.Text = "  "+localizedLabel;

			button.FlatAppearance.BorderSize = this._templateButton.FlatAppearance.BorderSize;
			button.FlatStyle = this._templateButton.FlatStyle;
			button.ImageAlign = this._templateButton.ImageAlign;
			button.TextImageRelation = this._templateButton.TextImageRelation ;
			button.UseVisualStyleBackColor = this._templateButton.UseVisualStyleBackColor;
			button.Enabled = enabled;

			toolTip1.SetToolTip(button, localizedTooltip);
			panel.Controls.Add(button);
			return button;
		}

		private void AddCreateChoices(TableLayoutPanel panel)
		{
			AddSection("Create", panel);
			AddChoice("Create new blank project", string.Empty, "newProject", true, this.createNewProject_LinkClicked, panel);
			AddChoice("Create new project from FLEx LIFT export", string.Empty, "flex", true, this.OnCreateProjectFromFLEx_LinkClicked, panel);
		}

		private void AddGetChoices(TableLayoutPanel panel)
		{
			var chorusMessage = HgRepository.GetEnvironmentReadinessMessage("en");
			bool haveChorus = string.IsNullOrEmpty(chorusMessage);

			AddSection("Get", panel);
			AddChoice("Get From USB drive", "Get a project from a Chorus repository on a USB flash drive", "getFromUsb", haveChorus, OnGetFromUsb, panel);
			AddChoice("Get from Internet", "Get a project from a Chorus repository which is hosted on the internet (e.g. public.languagedepot.org) and put it on this computer",
				"getFromInternet", haveChorus, OnGetFromInternet, panel);
		}

		private void OnGetFromInternet(object sender, EventArgs e)
		{
			if (!Directory.Exists(WeSay.Project.WeSayWordsProject.NewProjectDirectory))
			{
				//e.g. mydocuments/wesay
				Directory.CreateDirectory(WeSay.Project.WeSayWordsProject.NewProjectDirectory);
			}
			using (var dlg = new Chorus.UI.Clone.GetCloneFromInternetDialog(WeSay.Project.WeSayWordsProject.NewProjectDirectory))
			{
				if (DialogResult.Cancel == dlg.ShowDialog())
					return;
				OpenSpecifiedProject(dlg.PathToNewProject);
			}
		}

		private void OnGetFromUsb(object sender, EventArgs e)
		{
			using (var dlg = new Chorus.UI.Clone.GetCloneFromUsbDialog(WeSay.Project.WeSayWordsProject.NewProjectDirectory))
			{
				dlg.Model.ProjectFilter = dir => GetLooksLikeWeSayProject(dir);
				if (DialogResult.Cancel == dlg.ShowDialog())
					return;
				OpenSpecifiedProject(dlg.PathToNewProject);
			}
		}

		private static bool GetLooksLikeWeSayProject(string directoryPath)
		{
			return Directory.GetFiles(directoryPath, "*.WeSayConfig").Length > 0;
		}

		private void AddOpenProjectChoices(TableLayoutPanel panel)
		{
			int count = 0;
			foreach (string path in Settings.Default.MruConfigFilePaths.Paths)
			{
				AddFileChoice(path, panel);
				++count;
				if (count > 2)
					break;

			}
			AddChoice("Browse for other projects...", string.Empty, "browse", true, openDifferentProject_LinkClicked, panel);
		}

		private void openRecentProject_LinkClicked(object sender, EventArgs e)
		{
			if (OpenSpecifiedProject != null)
			{
				OpenSpecifiedProject.Invoke(((Button) sender).Tag as string);
			}
		}

		private void openDifferentProject_LinkClicked(object sender, EventArgs e)
		{
			if (ChooseProjectClicked != null)
			{
				ChooseProjectClicked.Invoke(this, null);
			}
		}

		private void createNewProject_LinkClicked(object sender, EventArgs e)
		{
			if (NewProjectClicked != null)
			{
				NewProjectClicked.Invoke(this, null);
			}
		}

		private void WelcomeControl_Load(object sender, EventArgs e)
		{
			LoadButtons();
		}

		private void OnCreateProjectFromFLEx_LinkClicked(object sender, EventArgs e)
		{
			if (NewProjectFromFlexClicked != null)
			{
				NewProjectFromFlexClicked.Invoke(this, null);
			}
		}

	}
}