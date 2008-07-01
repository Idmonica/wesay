using System;
using System.Drawing;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.i8n;
using WeSay.AddinLib;
using WeSay.Foundation;

namespace Addin.Backup
{
	//not ready yet [Extension]
	public class Synchronize: IWeSayAddin, IWeSayAddinHasSettings
	{
		private SynchronizeSettings _settings;

		public Synchronize()
		{
			_settings = new SynchronizeSettings();
		}

		#region IWeSayAddin Members

		public Image ButtonImage
		{
			get { return Resources.backupToDeviceImage; }
		}

		public bool Available
		{
			get { return true; }
		}

		public string LocalizedName
		{
			get
			{
				return
						StringCatalog.Get("~Synchronize",
										  "Label for synchronize action. Here, 'synchronizing' means putting your changes in the repository, and getting changes that others have put in the repository.");
			}
		}

		public string Description
		{
			get
			{
				return
						StringCatalog.Get(
								"~Updates the repository with our work, and gets the work of others.",
								"description of syncrhonize action");
			}
		}

		public string ID
		{
			get { return "Synchronize"; }
			set { throw new NotImplementedException(); }
		}

		#region IWeSayAddinHasSettings Members

		public bool DoShowSettingsDialog(Form parentForm, ProjectInfo projectInfo)
		{
			return false;
		}

		public object Settings
		{
			get { return _settings; }
			set { _settings = value as SynchronizeSettings; }
		}

		#endregion

		public void Launch(Form parentForm, ProjectInfo projectInfo)
		{
			SynchronizeDialog d = new SynchronizeDialog(projectInfo, _settings);
			d.ShowDialog(parentForm);
		}

		#endregion

		#region IThingOnDashboard Members

		public DashboardGroup Group
		{
			get { return DashboardGroup.Share; }
		}

		public string LocalizedLabel
		{
			get { return LocalizedName; }
		}

		public string LocalizedLongLabel
		{
			get
			{
				return StringCatalog.Get("~Synchronize With Repository", "Long label for synchronize action.");
			}
		}

		public ButtonStyle DashboardButtonStyle
		{
			get { return ButtonStyle.IconVariableWidth; }
		}

		public Image DashboardButtonImage
		{
			get { return Resources.greenSynchronize; }
		}

		#endregion
	}

	[Serializable]
	public class SynchronizeSettings
	{
		private string _pathToExecutable = @"$wesayApplicationDirectory\mercurial.bat";
		private string _arguments = @"$projectPath";

		public string PathToExecutable
		{
			get { return _pathToExecutable; }
			set { _pathToExecutable = value; }
		}

		public string Arguments
		{
			get { return _arguments; }
			set { _arguments = value; }
		}

		public string GetRuntimeProcessPath(ProjectInfo info)
		{
			return
					_pathToExecutable.Replace("$wesayApplicationDirectory",
											  info.PathToApplicationRootDirectory);
		}

		public string GetRuntimeArguments(ProjectInfo info)
		{
			return _arguments.Replace("$projectPath", info.PathToTopLevelDirectory);
		}
	}
}