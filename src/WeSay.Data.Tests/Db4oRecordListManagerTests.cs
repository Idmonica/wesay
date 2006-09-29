using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace WeSay.Data.Tests
{
	[TestFixture]
	public class Db4oRecordListManagerTests : RecordListManagerBaseTests
	{
		private string _filePath;

		[SetUp]
		public override void Setup()
		{
			_filePath = System.IO.Path.GetTempFileName();
			ForceCreateFilterCaches();
			base.Setup();
		}

		private void ForceCreateFilterCaches() {
			base.Setup();
			RecordListManager.Get<SimpleIntTestClass>(Filter10to19);
			RecordListManager.Get<SimpleIntTestClass>(Filter11to12);
			RecordListManager.Get<SimpleIntTestClass>(Filter11to17);
			RecordListManager.Get<SimpleIntTestClass>(Filter11to20);
			base.TearDown();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();

			string cacheDirectory = Path.Combine(Path.GetDirectoryName(this._filePath), "Cache");
			if(Directory.Exists(cacheDirectory))
			{
				Directory.Delete(cacheDirectory, true);
			}
			System.IO.File.Delete(_filePath);
		}

		protected override IRecordListManager CreateRecordListManager()
		{
			return new Db4oRecordListManager(_filePath);
		}

		[Test]
		public void ChangeRecord_NoLongerMeetsFilterCriteria_RemovedFromCachedRecordLists()
		{
			IRecordList<SimpleIntTestClass> masterRecordList = RecordListManager.Get<SimpleIntTestClass>();
			masterRecordList[11].I = 10;
			IRecordList<SimpleIntTestClass> recordList11to17 = RecordListManager.Get<SimpleIntTestClass>(Filter11to17);
			Assert.AreEqual(6, recordList11to17.Count);
			Assert.AreEqual(12, recordList11to17[0].I);
			IRecordList<SimpleIntTestClass> recordList11to20 = RecordListManager.Get<SimpleIntTestClass>(Filter11to20);
			Assert.AreEqual(9, recordList11to20.Count);
			Assert.AreEqual(12, recordList11to20[0].I);
		}

		[Test]
		public void ChangeRecord_MeetsFilterCriteria_AddedToCachedRecordList()
		{
			IRecordList<SimpleIntTestClass> masterRecordList = RecordListManager.Get<SimpleIntTestClass>();
			masterRecordList[0].I = 12;
			IRecordList<SimpleIntTestClass> recordList11to20 = RecordListManager.Get<SimpleIntTestClass>(Filter11to20);
			IRecordList<SimpleIntTestClass> recordList11to17 = RecordListManager.Get<SimpleIntTestClass>(Filter11to17);
			Assert.AreEqual(8, recordList11to17.Count);
			Assert.AreEqual(11, recordList11to20.Count);
			Assert.AreEqual(12, recordList11to17[recordList11to17.Count-1].I);
			Assert.AreEqual(12, recordList11to20[recordList11to20.Count-1].I);
		}

		[Test]
		public void AddRecord_AddedToCachedRecordLists()
		{
			IRecordList<SimpleIntTestClass> masterRecordList = RecordListManager.Get<SimpleIntTestClass>();
			masterRecordList.Add(new SimpleIntTestClass(15));
			IRecordList<SimpleIntTestClass> recordList11to20 = RecordListManager.Get<SimpleIntTestClass>(Filter11to20);
			IRecordList<SimpleIntTestClass> recordList11to17 = RecordListManager.Get<SimpleIntTestClass>(Filter11to17);
			Assert.AreEqual(8, recordList11to17.Count);
			Assert.AreEqual(11, recordList11to20.Count);
		}

		[Test]
		public void RemoveRecord_RemovedFromCachedRecordLists()
		{
			IRecordList<SimpleIntTestClass> masterRecordList = RecordListManager.Get<SimpleIntTestClass>();
			masterRecordList.RemoveAt(11);
			IRecordList<SimpleIntTestClass> recordList10to19 = RecordListManager.Get<SimpleIntTestClass>(Filter10to19);
			IRecordList<SimpleIntTestClass> recordList11to17 = RecordListManager.Get<SimpleIntTestClass>(Filter11to17);
			Assert.AreEqual(6, recordList11to17.Count);
			Assert.AreEqual(12, recordList11to17[0].I);
			Assert.AreEqual(9, recordList10to19.Count);
			Assert.AreEqual(12, recordList10to19[1].I);
		}

		private delegate void ChangeRecordList<T>(Db4oRecordList<T> recordList) where T:class, new();

		private void ChangeDatabaseOutFromUnderRecordListManager(ChangeRecordList<SimpleIntTestClass> change)
		{
			base.TearDown();
			using(Db4oDataSource dataSource = new Db4oDataSource(_filePath))
			{
				using (Db4oRecordList<SimpleIntTestClass> recordList = new Db4oRecordList<SimpleIntTestClass>(dataSource))
				{
					change(recordList);
				}
			}
			base.Setup();
		}

		[Test]
		[Ignore("To be fixed by WS-10")]
		public void SomeoneElseChangedRecord_NoLongerMeetsFilterCriteria_RemovedFromCachedRecordLists()
		{
			ChangeDatabaseOutFromUnderRecordListManager(delegate(Db4oRecordList<SimpleIntTestClass> recordList)
														{
															recordList[11].I = 10;
														});
			IRecordList<SimpleIntTestClass> recordList11to17 = RecordListManager.Get<SimpleIntTestClass>(Filter11to17);
			Assert.AreEqual(6, recordList11to17.Count);
			Assert.AreEqual(12, recordList11to17[0].I);
			IRecordList<SimpleIntTestClass> recordList11to20 = RecordListManager.Get<SimpleIntTestClass>(Filter11to20);
			Assert.AreEqual(9, recordList11to20.Count);
			Assert.AreEqual(12, recordList11to20[0].I);
		}

		[Test]
		[Ignore("To be fixed by WS-10")]
		public void SomeoneElseChangedRecord_MeetsFilterCriteria_AddedToCachedRecordList()
		{
			ChangeDatabaseOutFromUnderRecordListManager(delegate(Db4oRecordList<SimpleIntTestClass> recordList)
														{
															recordList[0].I = 12;
														});
			IRecordList<SimpleIntTestClass> recordList11to20 = RecordListManager.Get<SimpleIntTestClass>(Filter11to20);
			IRecordList<SimpleIntTestClass> recordList11to17 = RecordListManager.Get<SimpleIntTestClass>(Filter11to17);
			Assert.AreEqual(8, recordList11to17.Count);
			Assert.AreEqual(11, recordList11to20.Count);
			Assert.AreEqual(12, recordList11to17[recordList11to17.Count - 1].I);
			Assert.AreEqual(12, recordList11to20[recordList11to20.Count - 1].I);
		}

		[Test]
		[Ignore("To be fixed by WS-10")]
		public void SomeoneElseAddedRecord_AddedToCachedRecordLists()
		{
			ChangeDatabaseOutFromUnderRecordListManager(delegate(Db4oRecordList<SimpleIntTestClass> recordList)
											{
												recordList.Add(new SimpleIntTestClass(15));
											});

			IRecordList<SimpleIntTestClass> recordList11to20 = RecordListManager.Get<SimpleIntTestClass>(Filter11to20);
			IRecordList<SimpleIntTestClass> recordList11to17 = RecordListManager.Get<SimpleIntTestClass>(Filter11to17);
			Assert.AreEqual(8, recordList11to17.Count);
			Assert.AreEqual(11, recordList11to20.Count);
		}

		[Test]
		[Ignore("To be fixed by WS-10")]
		public void SomeoneElseRemovedRecord_RemovedFromCachedRecordLists()
		{
			ChangeDatabaseOutFromUnderRecordListManager(delegate(Db4oRecordList<SimpleIntTestClass> recordList)
											{
												recordList.RemoveAt(11);
											});

			IRecordList<SimpleIntTestClass> masterRecordList = RecordListManager.Get<SimpleIntTestClass>();
			masterRecordList.RemoveAt(11);
			IRecordList<SimpleIntTestClass> recordList10to19 = RecordListManager.Get<SimpleIntTestClass>(Filter10to19);
			IRecordList<SimpleIntTestClass> recordList11to17 = RecordListManager.Get<SimpleIntTestClass>(Filter11to17);
			Assert.AreEqual(6, recordList11to17.Count);
			Assert.AreEqual(12, recordList11to17[0].I);
			Assert.AreEqual(9, recordList10to19.Count);
			Assert.AreEqual(12, recordList10to19[1].I);
		}
	}

	[TestFixture]
	public class Db4oRecordListManagerCacheTests
	{
		private string _filePath;
		private IRecordListManager _recordListManager;
		private IRecordList<SimpleIntTestClass> _sourceRecords;
		private SimpleIntFilter _filter11to17;

		protected IRecordListManager RecordListManager
		{
			get
			{
				return _recordListManager;
			}
		}

		protected SimpleIntFilter Filter11to17
		{
			get
			{
				return _filter11to17;
			}
		}

		[SetUp]
		public void Setup()
		{
			_filePath = System.IO.Path.GetTempFileName();
			_recordListManager = new Db4oRecordListManager(_filePath);
			PopupateMasterRecordList();

			_filter11to17 = new SimpleIntFilter(11, 17);
			Filter11to17.UseInverseFilter = true;
			RecordListManager.Register<SimpleIntTestClass>(Filter11to17);
			RecordListManager.Get<SimpleIntTestClass>(Filter11to17);
			RecordListManager.Dispose();
			_recordListManager = new Db4oRecordListManager(_filePath);

			_filter11to17 = new SimpleIntFilter(11, 17);
			RecordListManager.Register<SimpleIntTestClass>(Filter11to17);

		}

		private void PopupateMasterRecordList()
		{
			_sourceRecords = RecordListManager.Get<SimpleIntTestClass>();
			for (int i = 0; i < 50; i++)
			{
				_sourceRecords.Add(new SimpleIntTestClass(i));
			}
		}

		[TearDown]
		public void TearDown()
		{
			RecordListManager.Dispose();

			string cacheDirectory = Path.Combine(Path.GetDirectoryName(this._filePath), "Cache");
			if (Directory.Exists(cacheDirectory))
			{
				Directory.Delete(cacheDirectory, true);
			}
			System.IO.File.Delete(_filePath);
		}

		[Test]
		public void FilterModified_ChangedRecord_NoLongerMeetsFilterCriteria_RemovedFromCachedRecordLists()
		{
			IRecordList<SimpleIntTestClass> masterRecordList = RecordListManager.Get<SimpleIntTestClass>();
			masterRecordList[11].I = 10;
			IRecordList<SimpleIntTestClass> recordList11to17 = RecordListManager.Get<SimpleIntTestClass>(Filter11to17);
			Assert.AreEqual(6, recordList11to17.Count);
			Assert.AreEqual(12, recordList11to17[0].I);
		}

		[Test]
		public void FilterModified_ChangedRecord_MeetsFilterCriteria_AddedToCachedRecordList()
		{
			IRecordList<SimpleIntTestClass> masterRecordList = RecordListManager.Get<SimpleIntTestClass>();
			masterRecordList[0].I = 12;
			IRecordList<SimpleIntTestClass> recordList11to17 = RecordListManager.Get<SimpleIntTestClass>(Filter11to17);
			Assert.AreEqual(8, recordList11to17.Count);
			SimpleIntTestClass[] List11to17 = new SimpleIntTestClass[recordList11to17.Count];
			recordList11to17.CopyTo(List11to17, 0);
			SimpleIntTestClass[] ListOf12 = Array.FindAll<SimpleIntTestClass>(List11to17,
														  delegate(SimpleIntTestClass testClass)
														  {
															  return testClass.I == 12;
														  });
			Assert.AreEqual(2, ListOf12.Length);
		}

		[Test]
		public void FilterModified_AddedRecord_AddedToCachedRecordLists()
		{
			IRecordList<SimpleIntTestClass> masterRecordList = RecordListManager.Get<SimpleIntTestClass>();
			masterRecordList.Add(new SimpleIntTestClass(15));
			IRecordList<SimpleIntTestClass> recordList11to17 = RecordListManager.Get<SimpleIntTestClass>(Filter11to17);
			Assert.AreEqual(8, recordList11to17.Count);
		}

		[Test]
		public void FilterModified_RemovedRecord_RemovedFromCachedRecordLists()
		{
			IRecordList<SimpleIntTestClass> masterRecordList = RecordListManager.Get<SimpleIntTestClass>();
			masterRecordList.RemoveAt(11);
			IRecordList<SimpleIntTestClass> recordList11to17 = RecordListManager.Get<SimpleIntTestClass>(Filter11to17);
			Assert.AreEqual(6, recordList11to17.Count);
			Assert.AreEqual(12, recordList11to17[0].I);
		}
	}
}
