using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml;
using NUnit.Framework;
using WeSay.App;
using WeSay.Data;
using WeSay.LexicalModel;
using WeSay.LexicalModel.Db4o_Specific;
using WeSay.Project;

namespace WeSay.Project.Tests
{
	[TestFixture]
	public class LiftUpdateTests
	{
		protected string _dbFile;
		protected string _directory;
		protected Db4oDataSource _dataSource;
		private Db4oRecordList<LexEntry> _records;
		private LiftUpdateService _service;
		private Dictionary<string, Guid> _guidDictionary = new Dictionary<string, Guid>();

		[SetUp]
		public void Setup()
		{
			BasilProject.Project = null;

			WeSayWordsProject.InitializeForTests();
			_dbFile = Path.GetTempFileName();
			_dataSource = new Db4oDataSource(_dbFile);
			Db4oLexModelHelper.Initialize(_dataSource.Data);
			this._records = new Db4oRecordList<LexEntry>(this._dataSource);

			this._directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(_directory);

			_service = new LiftUpdateService(_dataSource);

		}

		[TearDown]
		public void TearDown()
		{
			_records.Dispose();
			_dataSource.Dispose();
			Directory.Delete(this._directory,true);
			File.Delete(_dbFile);
		}

		[Test]
		public void MissingFileAndEmptyRecordList()
		{
			IList newGuys = _service.GetRecordsNeedingUpdateInLift();

			Assert.AreEqual(_dataSource.Data.Query<LexEntry>().Count, newGuys.Count);
		}

//        [Test]
//        public void MissingLIFTFileWouldUpdateAllRecords()
//        {
//            _records.Add(new LexEntry());
//            _records.Add(new LexEntry());
//
//            IList newGuys= _service.GetRecordsNeedingUpdateInLift();
//            Assert.AreEqual(_records.Count, newGuys.Count);
//        }

		[Test]
		public void WouldUpdateOnlyNewRecords()
		{
			// Linux and fat32 has resolution of second not millisecond!
			Thread.Sleep(1000);
			_records.Add(MakeEntry());
			_records.Add(MakeEntry());
			_service.DoLiftUpdateNow(false);
			// Linux and fat32 has resolution of second not millisecond!
			Thread.Sleep(1000);
			_records.Add(new LexEntry());
			_records.Add(new LexEntry());
			_records.Add(new LexEntry());

			IList newGuys = _service.GetRecordsNeedingUpdateInLift();
			Assert.AreEqual(3, newGuys.Count);
		}

		private LexEntry MakeEntry()
		{
			LexEntry e= new LexEntry();
		   // e.LexicalForm.SetAlternative("abc", id);
			e.GetOrCreateId(true);
			return e;
		}


		[Test]
		public void DeletionIsRecorded()
		{
			SetupDeletionSituation();
			int count = GetLiftDoc().SelectNodes("//entry[contains(@id,'boo_') and @dateDeleted]")
				.Count;
			if (count != 1)
			{
				Debug.WriteLine(GetLiftDoc().OuterXml);
			}
			Assert.AreEqual(1,count);
		}

		private void SetupDeletionSituation()
		{
			LiftIO.Utilities.CreateEmptyLiftFile(WeSayWordsProject.Project.PathToLiftFile, "test", true);
			_records.Add(new LexEntry());
			LexEntry entryToDelete = MakeEntry("boo");
			entryToDelete.GetOrCreateId(true);
			_records.Add(new LexEntry());

			WeSayWordsProject.Project.LockLift();//the next call will expect this to be locked

			_service.DoLiftUpdateNow(true);
			// Linux and fat32 has resolution of second not millisecond!
			Thread.Sleep(1000);

			//now delete it
			_records.Remove(entryToDelete);
			//this deletion event comes from a higher-level class we aren't using, so we raise it ourselves here:
			_service.OnDataDeleted(this, new DeletedItemEventArgs(entryToDelete));
			_service.DoLiftUpdateNow(true);
			// Linux and fat32 has resolution of second not millisecond!
			Thread.Sleep(1000);

		}

		[Test]
		public void DeletionIsExpungedIfSameIdReused()
		{
			SetupDeletionSituation();

			//now make an entry with the same id and add it
			MakeEntry("boo");
			_service.DoLiftUpdateNow(true);
			Assert.AreEqual(0, GetLiftDoc().SelectNodes("//entry[contains(@id,'boo_') and @dateDeleted]").Count);
			Assert.AreEqual(1, GetLiftDoc().SelectNodes("//entry[contains(@id,'boo_') and not(@dateDeleted)]").Count);
		}

		private LexEntry MakeEntry(string id)
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm["zzz"] = id;

			Guid g;
			if (!_guidDictionary.TryGetValue(id, out g))
			{
				g = Guid.NewGuid();
				_guidDictionary.Add(id,g);
			}

			entry.Guid = g;
			_records.Add(entry);
			return entry;
		}

		static private XmlDocument GetLiftDoc()
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(WeSayWordsProject.Project.PathToLiftFile);// _service.PathToBaseLiftFile);
			//Console.WriteLine(doc.OuterXml);
			return doc;
		}


//
//        [Test]
//        public void BackupAfterImportCrashOriginal()
//        {
//            string path = @"C:\WeSay\SampleProjects\Thai\wesay\tiny.words";
//            WeSayWordsProject project = new WeSayWordsProject();
//            project.LoadFromLiftLexiconPath(path);
//            IRecordListManager recordListManager;
//            recordListManager = new Db4oRecordListManager(new WeSayWordsDb4oModelConfiguration(), project.PathToDb4oLexicalModelDB);
//            Db4oLexModelHelper.Initialize(((Db4oRecordListManager)recordListManager).DataSource.Data);
//            Db4oRecordListManager ds = recordListManager as Db4oRecordListManager;
//            BackupService backupService = new BackupService(project.PathToLocalBackup, ds.DataSource);
//            ds.DataCommitted += new EventHandler(backupService.OnDataCommitted);
//            backupService.DoLiftUpdateNow();
//        }


		[Test]
		public void LiftIsFreshNow_NotLocked()
		{
			Thread.Sleep(2000);
			LiftUpdateService.LiftIsFreshNow();
			DateTime timeStamp = File.GetLastWriteTimeUtc(WeSayWordsProject.Project.PathToLiftFile);
			TimeSpan lastWriteTimeSpan = DateTime.UtcNow - timeStamp;
			Assert.Less(lastWriteTimeSpan.Milliseconds,2000);
		}

		[Test]
		public void LiftIsFreshNow_Locked()
		{
			WeSayWordsProject.Project.LockLift();
			LiftUpdateService.LiftIsFreshNow();
			WeSayWordsProject.Project.ReleaseLockOnLift();
		}
	}
}