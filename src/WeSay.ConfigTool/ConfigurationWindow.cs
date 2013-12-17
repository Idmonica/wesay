using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Autofac;
using Palaso.Reporting;
using WeSay.ConfigTool.Properties;
using WeSay.Project;

namespace WeSay.ConfigTool
{
	public partial class ConfigurationWindow: Form
	{
		private WelcomeControl _welcomePage;
		private SettingsControl _projectSettingsControl;
		private WeSayWordsProject _project;

		public ConfigurationWindow(string[] args)
		{
			InitializeComponent();

			Project = null;

			//            if (this.DesignMode)
			//                return;
			//
			InstallWelcomePage();
			UpdateEnabledStates();

			if (args.Length > 0)
			{
				OpenProject(args[0].Trim(new char[] {'"'}));
			}

			if (!DesignMode)
			{
				UpdateWindowCaption();
			}
		}

		private WeSayWordsProject Project
		{
			get { return _project; }
			set
			{
				_project = value;
				UpdateEnabledStates();
			}
		}

		// This delegate enables asynchronous calls for setting
		// the properties from another thread.
		private delegate void UpdateStuffCallback();

		private void UpdateEnabledStates()
		{
			if (toolStrip2.InvokeRequired)
			{
				UpdateStuffCallback d = UpdateEnabledStates;
				Invoke(d, new object[] {});
			}
			else
			{
				openProjectInWeSayToolStripMenuItem.Enabled = (_project != null);
			}
		}

		private void OnChooseProject(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Title = "Open WeSay Project...";
			dlg.DefaultExt = ".WeSayConfig";
			dlg.Filter = "WeSay Configuration File (*.WeSayConfig)|*.WeSayConfig";
			dlg.Multiselect = false;
			dlg.InitialDirectory = GetInitialDirectory();
			if (DialogResult.OK != dlg.ShowDialog(this))
			{
				return;
			}

			OnOpenProject(dlg.FileName, null);
		}

		private static string GetInitialDirectory()
		{
			string initialDirectory = null;
			string latestProject = Settings.Default.MruConfigFilePaths.Latest;
			if (!String.IsNullOrEmpty(latestProject))
			{
				Debug.Assert(File.Exists(latestProject));
				try
				{
					initialDirectory = Path.GetDirectoryName(latestProject);
				}
				catch
				{
					//swallow

					//esa 2008-06-09 Why do we have this catch?
				}
			}

			if (initialDirectory == null || initialDirectory == "")
			{
				initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}
			return initialDirectory;
		}

		public void OnOpenProject(object sender, EventArgs e)
		{
			string configFilePath = (string) sender;
			if (!File.Exists(configFilePath))
			{
				ErrorReport.ReportNonFatalMessage(
						"WeSay could not find the file at {0} anymore.  Maybe it was moved or renamed?",
						configFilePath);
				return;
			}

			OpenProject(configFilePath);
		}

		private void OnCreateProject(object sender, EventArgs e)
		{
			NewProject dlg = new NewProject();
			if (DialogResult.OK != dlg.ShowDialog())
			{
				return;
			}
			CreateAndOpenProject(dlg.SelectedPath);
		}

		public void CreateAndOpenProject(string directoryPath)
		{
			//the "wesay" part my not exist yet
			if (!Directory.GetParent(directoryPath).Exists)
			{
				Directory.GetParent(directoryPath).Create();
			}

			CreateNewProject(directoryPath);
			OpenProject(directoryPath);
		}

		private void CreateNewProject(string directoryPath)
		{
			WeSayWordsProject p;

			try
			{
				//p = new WeSayWordsProject();
				WeSayWordsProject.CreateEmptyProjectFiles(directoryPath);
			}
			catch (Exception e)
			{
				ErrorReport.ReportNonFatalMessage(
						"WeSay was not able to create a project there. \r\n" + e.Message);
				return;
			}

//            if (Project != null)
//            {
//                Project.Dispose();
//            }
//            Project = p;
			//SetupProjectControls(p.Container);

			//p.Save();

		}

		public void OpenProject(string path)
		{
			//System.Configuration.ConfigurationManager.AppSettings["LastConfigFilePath"] = path;

			//strip off any trailing '\'
			if (path[path.Length - 1] == Path.DirectorySeparatorChar ||
				path[path.Length - 1] == Path.AltDirectorySeparatorChar)
			{
				path = path.Substring(0, path.Length - 1);
			}

			try
			{
				Project = new WeSayWordsProject();

				//just open the accompanying lift file.
				path = path.Replace(".WeSayConfig", ".lift");

				if (path.Contains(".lift"))
				{
					path = Project.UpdateFileStructure(path);
					if (!Project.LoadFromLiftLexiconPath(path))
					{
						Project = null;
						return;
					}
				}
						//                else if (path.Contains(".WeSayConfig"))
						//                {
						//                    this.Project.LoadFromConfigFilePath(path);
						//                }
				else if (Directory.Exists(path))
				{
					Project.LoadFromProjectDirectoryPath(path);
				}
				else
				{
					throw new ApplicationException(path +
												   " is not named as a .lift file or .WeSayConfig file.");
				}
			}
			catch (Exception e)
			{
				ErrorReport.ReportNonFatalMessage("WeSay was not able to open that project. \r\n" +
												  e.Message);
				return;
			}

			IContainer container = _project.Container.CreateInnerContainer();
			var containerBuilder = new Autofac.Builder.ContainerBuilder();
			containerBuilder.Register(typeof(Tasks.TaskListView));
			containerBuilder.Register(typeof(Tasks.TaskListPresentationModel));
			containerBuilder.Build(container);

			SetupProjectControls(container);

			if (Project != null)
			{
				Settings.Default.MruConfigFilePaths.AddNewPath(Project.PathToConfigFile);
			}
		}

		private void SetupProjectControls(IContext context)
		{
			UpdateWindowCaption();
			RemoveExistingControls();
			InstallProjectsControls(context);
		}

		private void UpdateWindowCaption()
		{
			string projectName = "";
			if (Project != null)
			{
				projectName = Project.Name;
			}
			Text = projectName + " - WeSay Configuration Tool";
			_versionToolStripLabel.Text = ErrorReport.UserFriendlyVersionString;
		}

		private void InstallWelcomePage()
		{
			_welcomePage = new WelcomeControl();
			Controls.Add(_welcomePage);
			_welcomePage.BringToFront();
			_welcomePage.Dock = DockStyle.Fill;
			_welcomePage.NewProjectClicked += OnCreateProject;
			_welcomePage.OpenPreviousProjectClicked += OnOpenProject;
			_welcomePage.ChooseProjectClicked += OnChooseProject;
		}

		private void InstallProjectsControls(IContext context)
		{
			_projectSettingsControl = new SettingsControl(context);
			Controls.Add(_projectSettingsControl);
			_projectSettingsControl.Dock = DockStyle.Fill;
			_projectSettingsControl.BringToFront();
			_projectSettingsControl.Focus();
		}

		private void RemoveExistingControls()
		{
			if (_welcomePage != null)
			{
				Controls.Remove(_welcomePage);
				_welcomePage.Dispose();
				_welcomePage = null;
			}
			if (_projectSettingsControl != null)
			{
				Controls.Remove(_projectSettingsControl);
				_projectSettingsControl.Dispose();
				_projectSettingsControl = null;
			}
		}

		private void AdminWindow_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (_projectSettingsControl != null)
			{
				_projectSettingsControl.Dispose();
			}
		}

		private void AdminWindow_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				if (Project != null)
				{
					Project.Save();
				}
				Settings.Default.Save();
			}
			catch (Exception error)
			{
				//would make it impossible to quit. e.Cancel = true;
				ErrorReport.ReportNonFatalMessage(error.Message);
			}
		}

		private void OnAboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			new AboutBox().ShowDialog();
		}

		private void OnOpenThisProjectInWeSay(object sender, EventArgs e)
		{
			_project.Save(); //want the client to see the latest
			string dir = Directory.GetParent(Application.ExecutablePath).FullName;
			ProcessStartInfo startInfo = new ProcessStartInfo(Path.Combine(dir, "WeSay.App.exe"),
															  string.Format("\"{0}\"",
																			_project.PathToLiftFile));
			Process.Start(startInfo);
		}

		private void OnExit_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}