using System;
using System.IO;
using Db4objects.Db4o;
using NUnit.Framework;
using WeSay.Foundation.Tests;

namespace WeSay.Project.Tests
{
	[TestFixture]
	public class CacheUpdatingTests
	{
		private string _experimentDir;

		private string _projectDir;
		private string _weSayDir;
		private WeSayWordsProject _project;
		private string _liftPath;

		[SetUp]
		public void Setup()
		{
			_experimentDir = MakeDir(Path.GetTempPath(), Path.GetRandomFileName());
			_projectDir = MakeDir(_experimentDir, "TestProj");
			_weSayDir = _projectDir; // MakeDir(_projectDir, "WeSay");
			_liftPath = Path.Combine(_weSayDir, "test.lift");
			MakeEmptyFile(_liftPath);
			_project = new WeSayWordsProject();
			_project.PathToLiftFile = _liftPath;
		}

		private static void MakeEmptyFile(string path)
		{
			using (StreamWriter s = File.CreateText(path))
			{
				s.Close(); //must close and dispose to reliably free the lock
			}
		}

		[TearDown]
		public void TearDown()
		{
			_project.Dispose();
			TestUtilities.DeleteFolderThatMayBeInUse(_experimentDir);
		}

		private static string MakeDir(string existingParent, string newChild)
		{
			string dir = Path.Combine(existingParent, newChild);
			Directory.CreateDirectory(dir);
			return dir;
		}

		[Test]
		public void MissingCacheDirTriggersUpdate()
		{
			OutOfDate();
		}

		[Test]
		public void EmptyCacheDirTriggersUpdate()
		{
			MakeDir(_weSayDir, "Cache");
			OutOfDate();
		}

		[Test]
		public void OlderWordsFileTriggersUpdate()
		{
			string cacheDir = MakeDir(_weSayDir, "Cache");
			string s = Path.Combine(cacheDir, "test.words");
			MakeEmptyFile(s);
			File.SetLastWriteTimeUtc(s, TimeOfUpdate.Subtract(new TimeSpan(1)));
			//just one tick older
			OutOfDate();
		}

		[Test]
		public void MissingWordsFileTriggersUpdate()
		{
			MakeDir(_weSayDir, "Cache");
			OutOfDate();
		}

		[Test]
		public void OlderLiftFileTriggersUpdate()
		{
			MakeDir(_weSayDir, "Cache");
			using (
					IObjectContainer db =
							Db4oFactory.OpenFile(WeSayWordsProject.Project.PathToRepository)
					)
			{
				db.Close();
			}

			File.SetLastWriteTimeUtc(_liftPath, TimeOfUpdate.Subtract(new TimeSpan(1)));
			OutOfDate();
		}

		[Test]
		public void NoUpdateWhenSynced()
		{
			MakeDir(_weSayDir, "Cache");

			using (
					IObjectContainer db =
							Db4oFactory.OpenFile(WeSayWordsProject.Project.PathToRepository)
					)
			{
				CacheManager.UpdateSyncPointInCache(db, File.GetLastWriteTimeUtc(_liftPath));
				db.Close();
			}
			UpToDate();
		}

		private DateTime TimeOfUpdate
		{
			get { return File.GetLastWriteTimeUtc(_liftPath); }
		}

		private void UpToDate()
		{
			Assert.IsFalse(CacheManager.GetCacheIsOutOfDate(_project));
		}

		private void OutOfDate()
		{
			Assert.IsTrue(CacheManager.GetCacheIsOutOfDate(_project));
		}
	}
}