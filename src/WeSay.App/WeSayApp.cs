using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using CommandLine;
using Palaso.Reporting;
using Palaso.Services;
using Palaso.Services.Dictionary;
using Palaso.Services.ForServers;
using Palaso.UI.WindowsForms.i8n;
using WeSay.App.Properties;
using WeSay.Data;
using WeSay.LexicalModel;
using WeSay.LexicalModel.Db4o_Specific;
using WeSay.LexicalTools;
using WeSay.Project;
using WeSay.UI;

namespace WeSay.App
{
	public class WeSayApp
	{
		//private static Mutex _oneInstancePerProjectMutex;
		private  WeSayWordsProject _project;
#if !MONO
		//private  ServiceHost _dictionaryHost;
#endif
		private  DictionaryServiceProvider _dictionary;
		private  LexEntryRepository _recordListManager ;
		private CommandLineArguments _commandLineArguments = new CommandLineArguments();
		private ServiceAppSingletonHelper _serviceAppSingletonHelper;
		private TabbedForm _tabbedForm;
		private IDisposable _serviceLifeTimeHelper;

		[STAThread]
		static void Main(string[] args)
		{
			WeSayApp app = new WeSayApp(args);
			app.Run();
		}
		public WeSayApp(string[] args)
		{
		   // Palaso.Services.ForClients.IpcSystem.IsWcfAvailable = false;
			Application.EnableVisualStyles();
			//leave this at the top:
			try
			{
				Application.SetCompatibleTextRenderingDefault(false);
			}
			catch (Exception) //swallow
			{
				//this fails in some test scenarios; perhaps the unit testing framework is leaving us in
				//the same appdomain, and that remembers that we called this once before?
			}
			OsCheck();
			Logger.Init();
			SetupErrorHandling();
			 //problems with user.config: http://blogs.msdn.com/rprabhu/articles/433979.aspx

		   //bring in settings from any previous version
		   if (Settings.Default.NeedUpgrade)
		   {
			   Settings.Default.Upgrade();
			   Settings.Default.NeedUpgrade = false;
		   }
			UsageReporter.AppNameToUseInDialogs = "WeSay";
			UsageReporter.AppNameToUseInReporting = "WeSayApp";

			if (!Parser.ParseArguments(args, _commandLineArguments, new ReportError(ShowCommandLineError)))
			{
				Application.Exit();
			}
		}
		public bool ServerModeStartRequested
		{
			get { return _commandLineArguments.startInServerMode; }
		}

		public void Run()
		{
			string path = DetermineActualLiftPath(_commandLineArguments.liftPath);
			if (!String.IsNullOrEmpty(path))
			{
				path = path.Replace(Path.DirectorySeparatorChar, '-');
				path = path.Replace(Path.VolumeSeparatorChar, '-');

				_serviceAppSingletonHelper =
					ServiceAppSingletonHelper.CreateServiceAppSingletonHelperIfNeeded("WeSay-" + path,
									  _commandLineArguments.startInServerMode);
				if (_serviceAppSingletonHelper == null)
				{
					return; // there's already an instance of this app running
				}
			}

			try
			{
				DisplaySettings.Default.SkinName = Settings.Default.SkinName;

				_project = InitializeProject(_commandLineArguments.liftPath);
				if (_project == null)
				{
					return;
				}

				if (_project.PathToWeSaySpecificFilesDirectoryInProject.IndexOf("PRETEND") < 0)
				{
					RecoverUnsavedDataIfNeeded();
				}
				LiftPreparer preparer = new LiftPreparer(_project);
				if (!preparer.MakeCacheAndLiftReady())
				{
					return;
				}
				using (_recordListManager = _project.MakeRecordListManager())
				{
					using (_dictionary = new DictionaryServiceProvider(this, _project))
					{
						_project.LiftUpdateService = SetupUpdateService(_recordListManager);
						_project.LiftUpdateService.DoLiftUpdateNow(true);

						StartDictionaryServices();
						_dictionary.LastClientDeregistered += _serviceAppSingletonHelper.OnExitIfInServerMode;
						_serviceAppSingletonHelper.HandleEventsUntilExit(StartUserInterface);

						_dictionary.LastClientDeregistered -= _serviceAppSingletonHelper.OnExitIfInServerMode;

						//do a last backup before exiting
						_project.LiftUpdateService.DoLiftUpdateNow(true);
						Logger.WriteEvent("App Exiting Normally.");
					}
				}
			}
			finally
			{
				 if (_serviceLifeTimeHelper != null)
				{
					_serviceLifeTimeHelper.Dispose();
				}
				if (_serviceAppSingletonHelper != null)
				{
					_serviceAppSingletonHelper.Dispose();
				}

			}
			Logger.ShutDown();
			Settings.Default.Save();
		}

		private void RecoverUnsavedDataIfNeeded()
		{
			if (!File.Exists(_project.PathToDb4oLexicalModelDB))
			{
				return;
			}


			WeSayWordsDb4oModelConfiguration config = new WeSayWordsDb4oModelConfiguration();
			config.Configure();
			try
			{
				using (Db4oDataSource ds = new Db4oDataSource(_project.PathToDb4oLexicalModelDB))
				{
					Db4oLexModelHelper.Initialize(ds.Data);
					LiftUpdateService updateServiceForCrashRecovery = new LiftUpdateService(ds);
					updateServiceForCrashRecovery.RecoverUnsavedChangesOutOfCacheIfNeeded();
					Db4oLexModelHelper.Deinitialize(ds.Data);
				}
			}
			catch (System.IO.IOException e)
			{
				Palaso.Reporting.ErrorNotificationDialog.ReportException(e, null, false);
				Thread.CurrentThread.Abort();
			}
		}

		void OnBringToFrontRequest(object sender, EventArgs e)
		{
			if (_tabbedForm == null)
			{
				_serviceAppSingletonHelper.EnsureUIRunningAndInFront();
			}
			else
			{
				_tabbedForm.synchronizationContext.Send(
					   delegate
						   {
							   _tabbedForm.MakeFrontMostWindow();
						   }, null);
			}
		}

		private void StartDictionaryServices()
		{
			//Problem: if there is already a cache miss, this will be slow, and somebody will time out
			StartCacheWatchingStuff();

			Palaso.Reporting.Logger.WriteMinorEvent("Starting Dictionary Services at {0}", DictionaryAccessor.GetServiceName(_project.PathToLiftFile));
			_serviceLifeTimeHelper =  IpcSystem.StartServingObject(DictionaryAccessor.GetServiceName(_project.PathToLiftFile), _dictionary);

		}


		public bool IsInServerMode
		{
			get { return _serviceAppSingletonHelper.CurrentState  ==  ServiceAppSingletonHelper.State.ServerMode; }
		}

		/// <summary>
		/// Only show a dialog if the operation takes more than two seconds
		/// </summary>
		/// <param name="message"></param>
		public  void NotifyOfLongStartupThread(object message)
		{
			try
			{
				Thread.Sleep(2000);

				LongStartupNotification dlg = new LongStartupNotification();
				dlg.Message = (string)message;
				dlg.Show();
				Application.DoEvents();
				try
				{
					while (true)
					{
						Thread.Sleep(100);
						Application.DoEvents();//otherwise we get (Not Responding)
					}
				}
				catch (ThreadInterruptedException)
				{
					dlg.Close();
					dlg.Dispose();
				}
			}
			catch (ThreadInterruptedException)
			{
			}
		}

		/// <summary>
		/// Without this, if we add entries with no UI up, there is not dictionary task up, and the cache
		/// ignores new entries being added (and someday other stuff). Then when we do eventually pull
		/// the ui up, they'll get a painful cache rebuild.
		/// </summary>
		private void StartCacheWatchingStuff()
		{
			Thread notify = new Thread(NotifyOfLongStartupThread);

			notify.Start(
				StringCatalog.Get("~Please wait while WeSay prepares your data",
								  "This is shown in rare circumstances where WeSay finds it needs to prepare some indices so it can run faster.  The main point to get across is that the user should settle in for a long wait, not think something is broken or try to run WeSay again."));

			try
			{
				DictionaryTask dictionaryTask = new DictionaryTask(_recordListManager, _project.DefaultViewTemplate);
				dictionaryTask.RegisterWithCache(_project.DefaultViewTemplate);
			}
			finally
			{
				notify.Interrupt();
			}

//            LexEntryRepository manager = _recordListManager as LexEntryRepository;
//            if (manager != null)
//            {
//                HeadwordSortedListHelper helper = new HeadwordSortedListHelper(manager,
//                                                                     this._project.HeadWordWritingSystem);
//              manager.GetSortedList(helper);//installs it
//            }
		}

		public string CurrentUrl
		{
			get
			{
				if (_tabbedForm != null)
				{
					return _tabbedForm.CurrentUrl;
				}
				return string.Empty;
			}
		}

		public void GoToUrl(string url)
		{

			_serviceAppSingletonHelper.EnsureUIRunningAndInFront();

			//if it didn't timeout
			if(_serviceAppSingletonHelper.CurrentState == ServiceAppSingletonHelper.State.UiMode)
			{
				Debug.Assert(_tabbedForm != null, "tabbed form should have been started.");
				_tabbedForm.GoToUrl(url);
			}
		}

		private void StartUserInterface()
		{
			ITaskBuilder builder = null;
			try
			{
				_tabbedForm = new TabbedForm();
				_tabbedForm.Show(); // so the user sees that we did launch
				_tabbedForm.Text =
					StringCatalog.Get("~WeSay", "It's up to you whether to bother translating this or not.") + ": " +
					_project.Name + "        " + ErrorReport.UserFriendlyVersionString;
				Application.DoEvents();


				//MONO bug as of 1.1.18 cannot bitwise or FileShare on FileStream constructor
				//                    using (FileStream config = new FileStream(_project.PathTo_projectTaskInventory, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
				using (
					FileStream configFile =
						new FileStream(_project.PathToConfigFile, FileMode.Open, FileAccess.Read,
									   FileShare.ReadWrite))
				{
					builder = new ConfigFileTaskBuilder(configFile, _project,
														_tabbedForm as ICurrentWorkTask, _recordListManager);
				}
				_project.Tasks = builder.Tasks;
				Application.DoEvents();
				_tabbedForm.IntializationComplete += new EventHandler(OnTabbedForm_IntializationComplete);
				_tabbedForm.ContinueLaunchingAfterInitialDisplay();
				_tabbedForm.Activate();
				_tabbedForm.BringToFront();//needed if we were previously in server mode

				RtfRenderer.HeadWordWritingSystemId = _project.DefaultViewTemplate.HeadwordWritingSytem.Id;

				//run the ui
				Application.Run(_tabbedForm);

				Settings.Default.SkinName = DisplaySettings.Default.SkinName;

			}
			catch (IOException e)
			{
				ErrorReport.ReportNonFatalMessage(e.Message);
			}
			finally
			{
				//TODO(JH): having a builder than needs to be kept around so it can be disposed of is all wrong.
				//either I want to change it to something like TaskList rather than ITaskBuilder, or
				//it needs to create some disposable object other than a IList<>.
				//The reason we need to be able to dispose of it is because we need some way to
				//dispose of things that it might create, such as a data source.
				if (builder is IDisposable)
					((IDisposable) builder).Dispose();
			}
		}

		void OnTabbedForm_IntializationComplete(object sender, EventArgs e)
		{
			_serviceAppSingletonHelper.BringToFrontRequest += new EventHandler(OnBringToFrontRequest);
			_serviceAppSingletonHelper.UiReadyForEvents();
			_dictionary.UiSynchronizationContext = _tabbedForm.synchronizationContext;

		}

		private LiftUpdateService SetupUpdateService(LexEntryRepository recordListManager)
		{
			LiftUpdateService liftUpdateService;
			LexEntryRepository ds = (LexEntryRepository)    recordListManager;
			liftUpdateService = new LiftUpdateService(ds.DataSource);
			ds.DataCommitted += new EventHandler(liftUpdateService.OnDataCommitted);
			ds.DataDeleted +=new EventHandler<DeletedItemEventArgs>(liftUpdateService.OnDataDeleted);
			return liftUpdateService;
		}

		private WeSayWordsProject InitializeProject(string liftPath)
		{
			WeSayWordsProject project = new WeSayWordsProject();
			liftPath = DetermineActualLiftPath(liftPath);
			if (liftPath == null)
			{
				ErrorReport.ReportNonFatalMessage("WeSay was unable to figure out what lexicon to work on. Try opening the LIFT file by double clicking on it. If you don't have one yet, run the WeSay Configuration Tool to make a new WeSay project.");
				return null;
			}

			liftPath = project.UpdateFileStructure(liftPath);

			if (project.LoadFromLiftLexiconPath(liftPath))
			{
				Settings.Default.PreviousLiftPath = liftPath;
			}
			else
			{
				return null;
			}

			WeSayWordsProject.Project.LockLift(); // Consume will expect it to be locked already

			return project;
		}

		private string DetermineActualLiftPath(string liftPath)
		{
			if (liftPath == null)
			{
				if (!String.IsNullOrEmpty(Settings.Default.PreviousLiftPath))
				{
					if (File.Exists(Settings.Default.PreviousLiftPath))
					{
						liftPath = Settings.Default.PreviousLiftPath;
					}
				}
			}
			if (!File.Exists(liftPath))
			{
				return null;
			}
			return liftPath;
		}



		private void OsCheck()
		{
#if DEBUG
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				Console.WriteLine("running on Unix");
			}
			else
			{
				Console.WriteLine("running on Windows");
			}
#endif
		}


		private void SetupErrorHandling()
		{
			ErrorReport.EmailAddress = "issues@wesay.org";
			if (BasilProject.IsInitialized)
			{
				ErrorReport.AddProperty("ProjectPath", BasilProject.Project.ProjectDirectoryPath);
			}
			ErrorReport.AddStandardProperties();
			ExceptionHandler.Init();
		}



		class CommandLineArguments
		{
			[DefaultArgument(ArgumentTypes.AtMostOnce,
			   // DefaultValue = @"..\..\SampleProjects\Thai\WeSay\thai5000.words",
				HelpText = "Path to the Lift Xml file (e.g. on windows, \"c:\\thai\\wesay\\thai.lift\").")]
			public string liftPath = null;

//            [Argument(ArgumentTypes.AtMostOnce,
//                HelpText = "Language to show the user interface in.",
//                LongName = "ui",
//                ShortName = "")]
//            public string ui = null;

			[Argument(ArgumentTypes.AtMostOnce,
			HelpText = "Start without a user interface (will have no effect if WeSay is already running with a UI.",
			LongName = "server",
				DefaultValue=false,
			ShortName = "")]
			public bool startInServerMode = false;
		}

		void ShowCommandLineError(string e)
		{
			Parser p = new Parser(typeof(CommandLineArguments), new ReportError(ShowCommandLineError));
			e = e.Replace("Duplicate 'liftPath' argument", "Please enclose project path in quotes if it contains spaces.");
			e += "\r\n\r\n" + p.GetUsageString(200);
			MessageBox.Show(e, "WeSay Command Line Problem");
		}
	}

	internal class ThreadExceptionHandler
	{
		///
		/// Handles the thread exception.
		///
		public void Application_ThreadException(
			object sender, ThreadExceptionEventArgs e)
		{
			MessageBox.Show("caught");
		}
	}

}
