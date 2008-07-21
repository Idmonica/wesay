using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

using LiftIO;
using LiftIO.Migration;
using LiftIO.Validation;

using Palaso.Progress;
using Palaso.Reporting;
using Palaso.UI.WindowsForms.Progress;

namespace WeSay.LexicalModel.Migration
{
	internal class LiftPreparer
	{
		private LiftRepository _repository;
		private string _liftFilePath;

		[Obsolete]
		public LiftPreparer(string liftFilePath, LiftRepository repository)
		{
			_repository = repository;
			_liftFilePath = liftFilePath;
		}

		public bool MakeCacheAndLiftReady()
		{

			if (!MigrateIfNeeded())
			{
				return false;
			}

			//if (!_repository.BackendBringCachesUpToDate())
			//{
			//    return false;
			//}

			// Check this out !!! We still need to fix the 'fresh install' problem. CJP
			//if (CacheManager.GetAssumeCacheIsFresh(_project.PathToCache))
			//{
			//    //prevent the update service from thinking the LIFT file is really old
			//    //compared to the cache, due to the installer messing with the dates.
			//    LiftUpdateService.LiftIsFreshNow();
			//}
			////whether or not we're out of date, remove this indicator file, which is only to get
			////fresh-from-install launchign without an installer-induced
			////false dirty cache signal
			//CacheManager.RemoveAssumeCacheIsFreshIndicator();

			return true;
		}

		private bool PreprocessLift()
		{
			using (ProgressDialog dlg = new ProgressDialog())
			{
				dlg.Overview = "Please wait while WeSay preprocesses your LIFT file.";
				BackgroundWorker preprocessWorker = new BackgroundWorker();
				preprocessWorker.DoWork += OnDoPreprocessLiftWork;
				dlg.BackgroundWorker = preprocessWorker;
				dlg.CanCancel = true;
				dlg.ShowDialog();
				if (dlg.ProgressStateResult.ExceptionThatWasEncountered != null)
				{
					ErrorReport.ReportNonFatalMessage(
							String.Format(
									"WeSay encountered an error while preprocessing the file '{0}'.  Error was: {1}",
									_liftFilePath,
									dlg.ProgressStateResult.ExceptionThatWasEncountered.Message));
				}
				return (dlg.DialogResult == DialogResult.OK);
			}
		}

		private void OnDoPreprocessLiftWork(object sender, DoWorkEventArgs e)
		{
			BackgroundWorkerState state = (BackgroundWorkerState) e.Argument;
			state.StatusLabel = "Preprocessing...";
			try
			{
				string pathToLift = _liftFilePath;
				string outputPath = Utilities.ProcessLiftForLaterMerging(pathToLift);
				//    int liftProducerVersion = GetLiftProducerVersion(pathToLift);

				outputPath = PopulateDefinitions(outputPath);
				MoveTempOverRealAndBackup(pathToLift, outputPath);
			}
			catch (Exception error)
			{
				state.ExceptionThatWasEncountered = error;
				state.State = ProgressState.StateValue.StoppedWithError;
				throw;
				// this will put the exception in the e.Error arg of the RunWorkerCompletedEventArgs
			}
		}

		internal static string PopulateDefinitions(string pathToLift)
		{
			string outputPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.NewLineOnAttributes = true;

			using (
					Stream xsltStream =
							Assembly.GetExecutingAssembly().GetManifestResourceStream(
									"WeSay.App.Migration.populateDefinitionFromGloss.xslt"))
			{
				TransformWithProgressDialog transformer =
						new TransformWithProgressDialog(pathToLift,
														outputPath,
														xsltStream,
														"//sense");
				transformer.TaskMessage = "Populating Definitions from Glosses";
				transformer.Transform(true);
				return outputPath;
			}
		}

		//        private int GetLiftProducerVersion(string pathToLift)
		//        {
		//            string s = FindFirstInstanceOfPatternInFile(pathToLift, "producer=\"()\"");
		//        }

		//private static string FindFirstInstanceOfPatternInFile(string inputPath, string pattern)
		//{
		//    Regex regex = new Regex(pattern);
		//    using (StreamReader reader = File.OpenText(inputPath))
		//    {
		//        while (!reader.EndOfStream)
		//        {
		//            Match m = regex.Match(reader.ReadLine());
		//            if (m != null)
		//            {
		//                return m.Value;
		//            }
		//        }
		//        reader.Close();
		//    }
		//    return string.Empty;
		//}

		private static void MoveTempOverRealAndBackup(string existingPath, string newFilePath)
		{
			string backupName = existingPath + ".old";

			try
			{
				if (File.Exists(backupName))
				{
					File.Delete(backupName);
				}

				File.Move(existingPath, backupName);
			}
			catch
			{
				Logger.WriteEvent(String.Format("Couldn't write out to {0} ", backupName));
			}

			File.Copy(newFilePath, existingPath);
			File.Delete(newFilePath);
		}

		/// <summary>
		///
		/// </summary>
		/// <returns>true if everything is ok, false if something went wrong</returns>
		public bool MigrateIfNeeded()
		{
			if (Migrator.IsMigrationNeeded(_liftFilePath))
			{
				using (ProgressDialog dlg = new ProgressDialog())
				{
					dlg.Overview =
							"Please wait while WeSay migrates your lift database to the required version.";
					BackgroundWorker cacheBuildingWork = new BackgroundWorker();
					cacheBuildingWork.DoWork += DoMigrationWork;
					dlg.BackgroundWorker = cacheBuildingWork;
					dlg.CanCancel = false;

					dlg.ShowDialog();
					if (dlg.DialogResult != DialogResult.OK)
					{
						Exception err = dlg.ProgressStateResult.ExceptionThatWasEncountered;
						if (err != null)
						{
							ErrorNotificationDialog.ReportException(err, null, false);
						}
						else if (dlg.ProgressStateResult.State ==
								 ProgressState.StateValue.StoppedWithError)
						{
							ErrorReport.ReportNonFatalMessage(
									"Failed." + dlg.ProgressStateResult.LogString,
									null,
									false);
						}
						return false;
					}
				}
			}
			return true;
		}

		private void DoMigrationWork(object obj, DoWorkEventArgs args)
		{
			ProgressState progressState = (ProgressState) args.Argument;
			try
			{
				string oldVersion = Validator.GetLiftVersion(_liftFilePath);
				Logger.WriteEvent("Migrating from {0} to {1}", oldVersion, Validator.LiftVersion);
				progressState.StatusLabel =
						string.Format("Migrating from {0} to {1}", oldVersion, Validator.LiftVersion);
				string migratedFile = Migrator.MigrateToLatestVersion(_liftFilePath);
				string nameForOldFile =
						_liftFilePath.Replace(".lift", "." + oldVersion + ".lift");

				if (File.Exists(nameForOldFile))
						// like, if we tried to convert it before and for some reason want to do it again
				{
					File.Delete(nameForOldFile);
				}
				File.Move(_liftFilePath, nameForOldFile);
				File.Move(migratedFile, _liftFilePath);

				args.Result = args.Argument as ProgressState;
			}
			catch (Exception e)
			{
				//currently, error reporter can choke because this is
				//being called from a non sta thread.
				//so let's leave it to the progress dialog to report the error
				//                Reporting.ErrorReporter.ReportException(e,null, false);
				progressState.ExceptionThatWasEncountered = e;
				progressState.WriteToLog(e.Message);
				progressState.State = ProgressState.StateValue.StoppedWithError;
			}
		}

		//private bool BringCachesUpToDate()
		//{
		//    Debug.Assert(!string.IsNullOrEmpty(_liftFilePath));
		//    CacheBuilder builder = CacheManager.GetCacheBuilderIfNeeded(_project);

		//    if (builder == null)
		//    {
		//        return true;
		//    }

		//    //ProgressState progressState = new WeSay.Foundation.ConsoleProgress();//new ProgressState(progressDialogHandler);
		//    using (ProgressDialog dlg = new ProgressDialog())
		//    {
		//        if (!PreprocessLift())
		//        {
		//            return false;
		//        }

		//        dlg.Overview =
		//                "Please wait while WeSay updates its caches to match the new or modified LIFT file.";
		//        BackgroundWorker cacheBuildingWork = new BackgroundWorker();
		//        cacheBuildingWork.DoWork += builder.OnDoWork;
		//        dlg.BackgroundWorker = cacheBuildingWork;
		//        dlg.CanCancel = true;
		//        dlg.ShowDialog();
		//        if (dlg.DialogResult != DialogResult.OK)
		//        {
		//            Exception err = dlg.ProgressStateResult.ExceptionThatWasEncountered;
		//            if (err != null)
		//            {
		//                if (err is LiftFormatException)
		//                {
		//                    ErrorReport.ReportNonFatalMessage(
		//                            "WeSay had problems with the content of the dictionary file.\r\n\r\n" +
		//                            err.Message);
		//                }
		//                else
		//                {
		//                    ErrorNotificationDialog.ReportException(err, null, false);
		//                }
		//            }
		//            else if (dlg.ProgressStateResult.State ==
		//                     ProgressState.StateValue.StoppedWithError)
		//            {
		//                ErrorReport.ReportNonFatalMessage(
		//                        "Could not build caches. " + dlg.ProgressStateResult.LogString,
		//                        null,
		//                        false);
		//            }
		//            return false;
		//        }
		//        _repository.BackendLiftIsFreshNow();
		//    }
		//    return true;
		//}
	}
}