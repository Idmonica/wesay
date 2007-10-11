using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Mono.Addins;
using Palaso.Reporting;
using WeSay.AddinLib;
using WeSay.Project;

namespace WeSay.Setup
{
	public partial class ActionsControl: ConfigurationControlBase
	{
		public ActionsControl(): base("setup and use plug-in actions")
		{
			InitializeComponent();
			Resize += ActionsControl_Resize;
		}

		private void ActionsControl_Resize(object sender, EventArgs e)
		{
			//this is part of dealing with .net not adjusting stuff well for different dpis

			_addinsList.Width = Width - 20;
			_addinsList.Height = (Height - _addinsList.Top) - 40;
		}

		//        private void OnVisibleChanged(object sender, EventArgs e)
		//        {
		//            if (Visible)
		//            {
		//                if (!_loaded)
		//                {
		//                    LoadAddins();
		//                    _loaded = true;
		//                }
		//               // UpdateStatesOfThings();
		//            }
		//        }

		private void LoadAddins()
		{
			List<string> alreadyFound = new List<string>();

			_addinsList.Clear();
			if (!AddinManager.IsInitialized)
			{
				AddinManager.Initialize(Application.UserAppDataPath);
				AddinManager.Registry.Rebuild(null);
				AddinManager.Shutdown();
				AddinManager.Initialize(Application.UserAppDataPath);
				//these (at least AddinLoaded) does get called after initialize, when you
				//do a search for objects (e.g. GetExtensionObjects())
				AddinManager.AddinLoaded +=AddinManager_AddinLoaded;
				AddinManager.AddinLoadError +=AddinManager_AddinLoadError;
				AddinManager.AddinUnloaded +=AddinManager_AddinUnloaded;
				AddinManager.ExtensionChanged +=AddinManager_ExtensionChanged;
			}

			foreach (
					IWeSayAddin addin in
							AddinManager.GetExtensionObjects(
									typeof (IWeSayAddin)))
			{
				//this alreadyFound business is a hack to prevent duplication in some
				// situation I haven't tracked down yet.
				if (!alreadyFound.Contains(addin.ID))
				{
					alreadyFound.Add(addin.ID);
					AddAddin(addin);
				}
			}
			AddAddin(new ComingSomedayAddin("Export To OpenOffice", ""));
			AddAddin(new ComingSomedayAddin("Export To Lexique Pro", ""));
			AddAddin(
					new ComingSomedayAddin("Send project to developers",
										   "Sends your project to WeSay for help/debugging."));
		}

		private void AddAddin(IWeSayAddin addin)
		{
			ActionItemControl control = new ActionItemControl(addin, true);
			control.DoShowInWeSay = AddinSet.Singleton.DoShowInWeSay(addin.ID);
			_addinsList.AddControlToBottom(control);
			control.Launch += OnLaunchAction;
		}

		private void OnLaunchAction(object sender, EventArgs e)
		{
			IWeSayAddin addin = (IWeSayAddin)sender;

			WeSayWordsProject project = WeSayWordsProject.Project;
			string[] filesBelongingToProject =
					WeSayWordsProject.GetFilesBelongingToProject(
							project.ProjectDirectoryPath);
			ProjectInfo projectInfo = new ProjectInfo(project.Name,
													  project.
															  ProjectDirectoryPath,
													  project.PathToLiftFile,
													  filesBelongingToProject,
													  AddinSet.Singleton.
															  LocateFile);

			try
			{
				addin.Launch(ParentForm, projectInfo);
			}
			catch (Exception error)
			{
				ErrorReport.ReportNonFatalMessage(error.Message);
			}
		}

		private static void AddinManager_ExtensionChanged(object sender,
												   ExtensionEventArgs args)
		{
			Logger.WriteEvent("Addin 'extensionChanged': {0}", args.Path);
		}

		private static void AddinManager_AddinUnloaded(object sender,
												AddinEventArgs args)
		{
			Logger.WriteEvent("Addin unloaded: {0}", args.AddinId);
		}

		private static void AddinManager_AddinLoadError(object sender,
												 AddinErrorEventArgs args)
		{
			Logger.WriteEvent("Addin load error: {0}", args.AddinId);
		}

		private static void AddinManager_AddinLoaded(object sender, AddinEventArgs args)
		{
			Logger.WriteEvent("Addin loaded: {0}", args.AddinId);
		}

		private void ActionsControl_Load(object sender, EventArgs e)
		{
			LoadAddins();
		}
	}
}