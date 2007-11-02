using System;
using System.Collections.Generic;
using System.IO;
using Db4objects.Db4o;
using NUnit.Framework;
using WeSay.Foundation;
using WeSay.Foundation.Progress;
using WeSay.LexicalModel;
using WeSay.Project;

namespace WeSay.Project.Tests
{
	[TestFixture]
	public class CacheBuilderTests
	{
//        private ProgressDialogHandler _progressHandler;
		private CacheBuilder _cacheBuilder;
		//private bool _finished;
		private ProgressState _progress;
		private string _simpleGoodLiftContents = string.Format("<?xml version='1.0' encoding='utf-8'?><lift version='{0}'><entry id='one'><sense><gloss lang='en'><text>hello</text></gloss></sense></entry><entry id='two'/></lift>", LiftIO.Validator.LiftVersion);
		private string _log;

		static protected string BackupPath
		{
			get
			{
				return WeSayWordsProject.Project.PathToLiftBackupDir;
				//return _cacheBuilder.DestinationDatabasePath + ".bak";
			}
		}

		[SetUp]
		public void Setup()
		{
			WeSayWordsProject.InitializeForTests();
			_cacheBuilder = new CacheBuilder(WeSayWordsProject.Project.PathToLiftFile);//Path.GetTempFileName());
			_progress = new ConsoleProgress();// ProgressState(_progressHandler);
			_progress.Log += new EventHandler<ProgressState.LogEvent>(OnLog);
//            _finished = false;
		}

//        private void _progressHandler_Finished(object sender, EventArgs e)
//        {
//            _finished = true;
//        }

		[TearDown]
		public void TearDown()
		{
			_progress.Dispose();
			if (Directory.Exists(WeSayWordsProject.Project.PathToCache))
			{
				Directory.Delete(WeSayWordsProject.Project.PathToCache, true);
			}

			if (File.Exists(_cacheBuilder.SourceLIFTPath))
			{
				File.Delete(_cacheBuilder.SourceLIFTPath);
			}
			if (Directory.Exists(WeSayWordsProject.Project.PathToLiftBackupDir))
			{
				string dirToEmptyOfBackupDirs =
						Directory.GetParent(WeSayWordsProject.Project.PathToLiftBackupDir).FullName;
				string[] backUpDirs = Directory.GetDirectories(dirToEmptyOfBackupDirs, "*incremental*");
				foreach (string dir in backUpDirs)
				{
					Directory.Delete(dir, true);
				}
			}
			WeSayWordsProject.Project.Dispose();
		}
		[Test]
		public void GoodLiftStopsWithProgressInFinishedState()
		{
			SimpleGoodLiftCore(false);
		}

		[Test]
		public void LeavesSynchronized()
		{
			SimpleGoodLiftCore(false);
			Assert.IsFalse(CacheManager.GetCacheIsOutOfDate(WeSayWordsProject.Project));
		}

		[Test]
		public void ReplacesExistingFiles()
		{
			SimpleGoodLiftCore(true);
		}

		private void SimpleGoodLiftCore(bool doMakeExistingFilesThatNeedToBeReplaced)
		{
			if (doMakeExistingFilesThatNeedToBeReplaced)
			{
//                string dir = Path.GetDirectoryName(_cacheBuilder.DestinationDatabasePath);
				string dir = WeSayWordsProject.Project.PathToCache;
				Directory.CreateDirectory(dir);
				string oldCache = WeSayWordsProject.Project.PathToCache;
				// _cacheBuilder.DestinationDatabasePath + " Cache";
				Directory.CreateDirectory(oldCache);

				File.WriteAllText(Path.Combine(oldCache, "foo"), "hello");
			}

			File.WriteAllText(_cacheBuilder.SourceLIFTPath, _simpleGoodLiftContents);
			Assert.AreEqual(ProgressState.StateValue.NotStarted, _progress.State);
			_cacheBuilder.DoWork(_progress);
			//WaitForFinish();
			//  Console.WriteLine(_log);
			Assert.AreEqual(ProgressState.StateValue.Finished, _progress.State, _log);
		}

		[Test]//, Ignore("Run this by hand if you have an E: volume (windows only)")]
		public void WorksWithTempDirectoryOnADifferentVolumne()
	   {
		   if (Environment.OSVersion.Platform == PlatformID.Unix)
		   {
			   Console.WriteLine("Ignored on non-Windows");
		   }
		   else
		   {
			   //testing approach: it's harder to get the temp locaiton changed, so we
			   // instead put the destination project over on the non-default volume
			   DriveInfo[] drives = DriveInfo.GetDrives();

			   // get a drive I might be able to use
			   string driveName = string.Empty;
			   foreach (DriveInfo drive in drives)
			   {
				   if (drive.IsReady &&
					   drive.DriveType != DriveType.CDRom &&
					   drive.Name != "C:\\")
				   {
					   driveName = drive.Name;
					   break;
				   }
			   }
			   if (driveName.Length == 0)
			   {
				   Console.WriteLine("Ignored when there is not an additional volume");
			   }
			   else
			   {
				   string directory;
				   do
				   {
					   directory = Path.Combine(driveName, Path.GetRandomFileName());

				   } while(Directory.Exists(directory));

				   string target = Path.Combine(directory, "pretend.lift");
				   Directory.CreateDirectory(directory);
				   try
				   {
					   //Directory.CreateDirectory(Path.Combine(directory, "WeSay"));
					   WeSayWordsProject.Project.SetupProjectDirForTests(target);
					   Assert.AreEqual(directory, WeSayWordsProject.Project.ProjectDirectoryPath);
					   SimpleGoodLiftCore(true);
				   }
				   finally
				   {
					   Directory.Delete(directory, true);
				   }
			   }
		   }
	   }

		[Test]
		public void BadLiftStopsWithProgressInErrorState()
		{
			TryToLoadBadLift();
			Assert.AreEqual(ProgressState.StateValue.StoppedWithError, _progress.State);
		}

		[Test]
		public void BadLiftOutputsToLog()
		{
			TryToLoadBadLift();
		  //  System.Diagnostics.Debug.WriteLine(_log);
			Assert.IsTrue(_log.Contains("Invalid") );
		}

		private void TryToLoadBadLift()
		{
			File.WriteAllText(_cacheBuilder.SourceLIFTPath, _simpleGoodLiftContents.Replace("</lift>", "<x/></lift>"));
			Assert.AreEqual(ProgressState.StateValue.NotStarted, _progress.State);
			_cacheBuilder.DoWork(_progress);
		   // WaitForFinish();
		}

		void OnLog(object sender, ProgressState.LogEvent e)
		{
			_log += e.message;
		}

		[Test]
		public void CreatesDb4oFileWhichContainsEntriesAndSenses()
		{
			File.WriteAllText(_cacheBuilder.SourceLIFTPath, _simpleGoodLiftContents);

			_cacheBuilder.DoWork(_progress);
		 //   WaitForFinish();
			string dbPath = WeSayWordsProject.Project.PathToDb4oLexicalModelDB;
			using (IObjectContainer db = Db4oFactory.OpenFile(dbPath))
			{
				IList<LexEntry> x = db.Query<LexEntry>();
				Assert.AreEqual(2, x.Count);

				int index = (x[0].Id == "one")?0:1;
				Assert.AreEqual("one", x[index].Id); //got the wrong order here
				Assert.AreEqual(1, x[index].Senses.Count); // sensitive to order (shame)
			}
			Assert.AreEqual(ProgressState.StateValue.Finished, _progress.State);
		}

		[Test]
		public void OkIfNoExistingDbToBackup()
		{
			File.Delete(BackupPath);
			MakeBackupOfExistingDBCore(BackupPath);
		}

		[Test, Ignore("Do we really need a backup of the cache?")]
		public void OkIfHasExistingDbToBackup()
		{
	//        File.WriteAllText(_cacheBuilder.DestinationDatabasePath, "old current");
			File.Delete(BackupPath);
			MakeBackupOfExistingDBCore(BackupPath);
			Assert.IsTrue(File.Exists(BackupPath));
			Assert.IsTrue(File.ReadAllText(BackupPath) == "old current");
		}

		[Test, Ignore("Do we really need a backup of the cache?")]
		public void OkIfHasExistingBackupToRemoveFirst()
		{

			File.WriteAllText(BackupPath, "old backup");
	  //      File.WriteAllText(_cacheBuilder.DestinationDatabasePath, "old current");
			MakeBackupOfExistingDBCore(BackupPath);
			Assert.IsTrue(File.Exists(BackupPath));
			Assert.IsTrue(File.ReadAllText(BackupPath) == "old current");
		}

		[Test]
		public void MakesBackupOfExistingDB()
		{
			//just in case
			File.Delete(BackupPath);

			MakeBackupOfExistingDBCore(BackupPath);
		}

		private void MakeBackupOfExistingDBCore(string backupPath)
		{
			File.WriteAllText(_cacheBuilder.SourceLIFTPath, _simpleGoodLiftContents);
			_cacheBuilder.DoWork(_progress);
	  //      WaitForFinish();
		}


		[Test]
		public void ClearIncrementalBackupCache_Command_DeletesIt()
		{
			string dir = WeSayWordsProject.Project.PathToLiftBackupDir;
			Directory.CreateDirectory(dir);
			string deleteThisGuy = Path.Combine(dir, "deleteMe.txt");
			File.WriteAllText(deleteThisGuy, "hello");
			CacheBuilder.ClearTheIncrementalBackupDirectory();
			Assert.IsFalse(File.Exists(deleteThisGuy));
		}

		[Test]
		public void ClearsIncrementalBackupCache()
		{
			string dir = WeSayWordsProject.Project.PathToLiftBackupDir;
			Directory.CreateDirectory(dir);
			string deleteThisGuy = Path.Combine(dir, "deleteMe.txt");
			File.WriteAllText(deleteThisGuy,"doesn't matter");
			File.WriteAllText(_cacheBuilder.SourceLIFTPath, _simpleGoodLiftContents);

			_cacheBuilder.DoWork(_progress);
		   // WaitForFinish();
			Assert.IsFalse(File.Exists(deleteThisGuy));
	   }

		//jh added
//        public void WaitForFinish()
//        {
//            while (!this._finished)
//            {
//                Application.DoEvents();
//                Thread.Sleep(5);
//            }
//        }
	}

}