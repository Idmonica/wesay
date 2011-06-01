using System;
using System.IO;
using System.Text;
using System.Xml;
using Chorus.sync;
using Chorus.Utilities;
using Chorus.VcsDrivers.Mercurial;
using NUnit.Framework;
using Palaso.Progress.LogBox;
using Palaso.TestUtilities;
using Palaso.Xml;

namespace WeSay.Project.Tests
{
	[TestFixture]
	public class ChorusBackupMakerTests
	{
		class BackupScenario : IDisposable
		{
			private ProjectDirectorySetupForTesting _projDir;
			private TemporaryFolder _backupDir;
			private ChorusBackupMaker _backupMaker;
			public BackupScenario(string testName)
			{
				_projDir = new ProjectDirectorySetupForTesting("");

				_backupMaker = new ChorusBackupMaker(new CheckinDescriptionBuilder());
				_backupDir = new TemporaryFolder(testName);

				_backupMaker.PathToParentOfRepositories = _backupDir.FolderPath;

			}
			public string PathToBackupProjectDir
			{
				get { return Path.Combine(_backupDir.FolderPath, _projDir.ProjectDirectoryName);}
			}

			public ChorusBackupMaker BackupMaker
			{
				get { return _backupMaker; }
			}

			public string SourceProjectDir
			{
				get { return _projDir.PathToDirectory; }
			}

			public void Dispose()
			{
				_projDir.Dispose();
				_backupDir.Dispose();
			}

			public void BackupNow()
			{
				BackupMaker.BackupNow(SourceProjectDir, "en");
			}

			public void AssertDirExistsInWorkingDirectory(string s)
			{
				string  expectedDir = Path.Combine(PathToBackupProjectDir, s);
				Assert.IsTrue(Directory.Exists(expectedDir));
			}

			public void AssertFileExistsInWorkingDirectory(string s)
			{
				string  path = Path.Combine(PathToBackupProjectDir, s);
				Assert.IsTrue(File.Exists(path));
			}

			public void AssertFileDoesNotExistInWorkingDirectory(string s)
			{
				string path = Path.Combine(PathToBackupProjectDir, s);
				Assert.IsFalse(File.Exists(path));
			}

			public void AssertFileExistsInRepo(string s)
			{
				var  r = new HgRepository(PathToBackupProjectDir, new NullProgress());
				Assert.IsTrue(r.GetFileExistsInRepo(s));
			}
		}

		[Test]
		[Category("Known Mono Issue")]
		public void BackupNow_FirstTime_CreatesValidRepositoryAndWorkingTree()
		{
			using (BackupScenario scenario = new BackupScenario("BackupNow_NewFolder_CreatesNewRepository"))
			{
				scenario.BackupNow();
				scenario.AssertDirExistsInWorkingDirectory(".hg");
				scenario.AssertFileExistsInRepo("test.lift");
				scenario.AssertFileExistsInRepo("test.WeSayConfig");
				scenario.AssertFileExistsInRepo(Path.Combine("WritingSystems", "qaa.ldml"));
				scenario.AssertFileExistsInRepo(Path.Combine("WritingSystems", "en.ldml"));
				scenario.AssertFileExistsInWorkingDirectory("test.lift");
				scenario.AssertFileExistsInWorkingDirectory("test.WeSayConfig");
				scenario.AssertFileExistsInWorkingDirectory(Path.Combine("WritingSystems", "qaa.ldml"));
				scenario.AssertFileExistsInWorkingDirectory(Path.Combine("WritingSystems", "en.ldml"));
			}
		}

		[Test]
		public void BackupNow_ExistingRepository_AddsNewFileToBackupDir()
		{
			// Test causes a crash in WrapShellCall.exe - is there an updated version?
			using (BackupScenario scenario = new BackupScenario("BackupNow_ExistingRepository_AddsNewFileToBackupDir"))
			{
				scenario.BackupNow();
				File.Create(Path.Combine(scenario.SourceProjectDir, "blah.lift")).Close();
				scenario.BackupNow();
				scenario.AssertFileExistsInWorkingDirectory("blah.lift");
				scenario.AssertFileExistsInRepo("blah.lift");
			}
		}

		[Test]
		public void BackupNow_RemoveFile_RemovedFromBackupDir()
		{
			// Test causes a crash in WrapShellCall.exe - is there an updated version?
			using (BackupScenario scenario = new BackupScenario("BackupNow_RemoveFile_RemovedFromBackupDir"))
			{
				File.Create(Path.Combine(scenario.SourceProjectDir, "blah.lift")).Close();
				scenario.BackupNow();
				File.Delete(Path.Combine(scenario.SourceProjectDir, "blah.lift"));
				scenario.BackupNow();
				scenario.AssertFileDoesNotExistInWorkingDirectory("blah.lift");
			}
		}

		[Test]
		public void CanSerializeAndDeserializeSettings()
		{
			var b = new ChorusBackupMaker(new CheckinDescriptionBuilder());
			b.PathToParentOfRepositories = @"z:\";
			var builder = new StringBuilder();
			//var dom = new XmlDocument();
			//dom.LoadXml("<foobar><blah></blah></foobar>");

			using (var writer = XmlWriter.Create(builder, CanonicalXmlSettings.CreateXmlWriterSettings()))
			{
				b.Save(writer);
				var dom = new XmlDocument();
				dom.Load(new StringReader(builder.ToString()));
				var loadedGuy = ChorusBackupMaker.CreateFromDom(dom, new CheckinDescriptionBuilder());
				Assert.AreEqual(@"z:\", loadedGuy.PathToParentOfRepositories);

			}
		}

	}

}
