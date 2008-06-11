using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace WeSay.Data.Tests
{
	public abstract class RecordListManagerBaseTests
	{
		private LexEntryRepository _recordListManager;
		private IRecordList<SimpleIntTestClass> _sourceRecords;
		private SimpleIntFilter _filter10to19;
		private SimpleIntFilter _filter11to12;
		private SimpleIntFilter _filter11to17;
		private SimpleIntFilter _filter11to20;
		private SimpleIntSortHelper _sortHelper;

		protected LexEntryRepository RecordListManager
		{
			get { return _recordListManager; }
		}

		protected SimpleIntFilter Filter10to19
		{
			get { return _filter10to19; }
		}

		protected SimpleIntFilter Filter11to12
		{
			get { return _filter11to12; }
		}

		protected SimpleIntFilter Filter11to17
		{
			get { return _filter11to17; }
		}

		protected SimpleIntFilter Filter11to20
		{
			get { return _filter11to20; }
		}

		protected SimpleIntSortHelper SortHelper
		{
			get { return _sortHelper; }
			set { _sortHelper = value; }
		}

		public virtual void Setup()
		{
			_recordListManager = CreateRecordListManager();
			_sortHelper = new SimpleIntSortHelper(_recordListManager);

			PopupateMasterRecordList();
			CreateFilters();
			RegisterFilters();
		}

		private void PopupateMasterRecordList()
		{
			_sourceRecords = RecordListManager.GetListOfType<SimpleIntTestClass>();
			if (_sourceRecords.Count == 0)
			{
				for (int i = 0; i < 50; i++)
				{
					_sourceRecords.Add(new SimpleIntTestClass(i));
				}
			}
		}

		private void RegisterFilters()
		{
			RecordListManager.Register(Filter10to19, SortHelper);
			RecordListManager.Register(Filter11to12, SortHelper);
			RecordListManager.Register(Filter11to17, SortHelper);
			RecordListManager.Register(Filter11to20, SortHelper);
		}

		protected virtual void CreateFilters()
		{
			_filter10to19 = new SimpleIntFilter(10, 19);
			_filter11to12 = new SimpleIntFilter(11, 12);
			_filter11to17 = new SimpleIntFilter(11, 17);
			_filter11to20 = new SimpleIntFilter(11, 20);
		}

		protected abstract LexEntryRepository CreateRecordListManager();

		public virtual void TearDown()
		{
			RecordListManager.Dispose();
		}

		protected static int CountMatching<T>(IEnumerable<T> enumerable, Predicate<T> match)
		{
			int count = 0;
			foreach (T t in enumerable)
			{
				if (match(t))
				{
					count++;
				}
			}
			return count;
		}

		[Test]
		public void Create()
		{
			Assert.IsNotNull(RecordListManager);
		}

		[Test]
		[ExpectedException(typeof (InvalidOperationException))]
		public void Get_UnregisteredFilter_ThrowsException()
		{
			RecordListManager.GetListOfTypeFilteredFurther(new SimpleIntFilter(21, 30), SortHelper);
		}

		[Test]
		public void Get_NoFilter_GivesAllRecords()
		{
			IRecordList<SimpleIntTestClass> data = RecordListManager.GetListOfType<SimpleIntTestClass>();
			Assert.IsNotNull(data);
			Assert.AreEqual(_sourceRecords, data);

			IRecordList<object> objectData = RecordListManager.GetListOfType<object>();
			Assert.IsNotNull(objectData);
			Assert.AreNotEqual(_sourceRecords, objectData);
		}

		[Test]
		public void Get_Filter_GivesFilterdRecords()
		{
			IRecordList<SimpleIntTestClass> data =
					RecordListManager.GetListOfTypeFilteredFurther(Filter11to20, SortHelper);
			Assert.IsNotNull(data);
			Assert.AreEqual(10, data.Count);

			int count11 = CountMatching(data,
										delegate(SimpleIntTestClass item) { return item.I == 11; });
			Assert.AreEqual(1, count11);

			Assert.AreNotEqual(_sourceRecords, data);
		}

		[Test]
		public void Get_SecondTime_GivesSameRecordList()
		{
			IRecordList<SimpleIntTestClass> recordList = RecordListManager.GetListOfType<SimpleIntTestClass>();
			Assert.AreSame(recordList, RecordListManager.GetListOfType<SimpleIntTestClass>());
		}

		[Test]
		public void Get_SameFilter_GivesSameRecordList()
		{
			IRecordList<SimpleIntTestClass> recordList =
					RecordListManager.GetListOfTypeFilteredFurther(Filter11to20, SortHelper);
			Assert.AreSame(recordList, RecordListManager.GetListOfTypeFilteredFurther(Filter11to20, SortHelper));
		}

		[Test]
		public void Get_DifferentFilter_GivesDifferentRecordList()
		{
			IRecordList<SimpleIntTestClass> recordList =
					RecordListManager.GetListOfTypeFilteredFurther(Filter11to20, SortHelper);
			Assert.AreNotSame(recordList, RecordListManager.GetListOfTypeFilteredFurther(Filter11to12, SortHelper));
		}

		[Test]
		public void ChangeRecord_NoLongerMeetsFilterCriteria_RemovedFromRecordList_StillInMaster()
		{
			IRecordList<SimpleIntTestClass> recordList11to20 =
					RecordListManager.GetListOfTypeFilteredFurther(Filter11to20, SortHelper);
			Assert.AreEqual(10, recordList11to20.Count);
			recordList11to20[0].I = 10;
			Assert.AreEqual(9, recordList11to20.Count);

			int count11 = CountMatching(recordList11to20,
										delegate(SimpleIntTestClass item) { return item.I == 11; });
			Assert.AreEqual(0, count11);
			Assert.AreEqual(50, _sourceRecords.Count);
		}

		[Test]
		public void ChangeRecord_NoLongerMeetsFilterCriteria_RemovedFromSimilarRecordList()
		{
			IRecordList<SimpleIntTestClass> recordList11to20 =
					RecordListManager.GetListOfTypeFilteredFurther(Filter11to20, SortHelper);
			IRecordList<SimpleIntTestClass> recordList11to17 =
					RecordListManager.GetListOfTypeFilteredFurther(Filter11to17, SortHelper);
			recordList11to20[0].I = 10;
			Assert.AreEqual(6, recordList11to17.Count);

			int recordList11to17count11 = CountMatching(recordList11to17,
														delegate(SimpleIntTestClass item) { return item.I == 11; });
			Assert.AreEqual(0, recordList11to17count11);

			int recordList11to20count11 = CountMatching(recordList11to20,
														delegate(SimpleIntTestClass item) { return item.I == 11; });
			Assert.AreEqual(0, recordList11to20count11);
		}

		[Test]
		public void ChangeRecord_MeetsFilterCriteria_RemainsInSimilarRecordList()
		{
			IRecordList<SimpleIntTestClass> recordList11to20 =
					RecordListManager.GetListOfTypeFilteredFurther(Filter11to20, SortHelper);
			IRecordList<SimpleIntTestClass> recordList11to17 =
					RecordListManager.GetListOfTypeFilteredFurther(Filter11to17, SortHelper);
			recordList11to20[0].I = 12;
			Assert.AreEqual(7, recordList11to17.Count);
			Assert.AreEqual(10, recordList11to20.Count);
			int recordList11to17count12 = CountMatching(recordList11to17,
														delegate(SimpleIntTestClass item) { return item.I == 12; });
			Assert.AreEqual(2, recordList11to17count12);

			int recordList11to20count12 = CountMatching(recordList11to20,
														delegate(SimpleIntTestClass item) { return item.I == 12; });
			Assert.AreEqual(2, recordList11to20count12);
		}

		[Test]
		public void AddRecordToFiltered_AddedToMaster()
		{
			IRecordList<SimpleIntTestClass> recordList11to20 =
					RecordListManager.GetListOfTypeFilteredFurther(Filter11to20, SortHelper);
			recordList11to20.Add(new SimpleIntTestClass(15));
			Assert.AreEqual(51, _sourceRecords.Count);
		}

		[Test]
		public void AddRecordToFiltered_AddedToRelevantRecordLists()
		{
			IRecordList<SimpleIntTestClass> recordList11to20 =
					RecordListManager.GetListOfTypeFilteredFurther(Filter11to20, SortHelper);
			IRecordList<SimpleIntTestClass> recordList11to17 =
					RecordListManager.GetListOfTypeFilteredFurther(Filter11to17, SortHelper);
			recordList11to20.Add(new SimpleIntTestClass(15));
			Assert.AreEqual(8, recordList11to17.Count);
			Assert.AreEqual(11, recordList11to20.Count);
		}

		[Test]
		public void AddRecordToMaster_AddedToRelevantRecordLists()
		{
			IRecordList<SimpleIntTestClass> recordList11to20 =
					RecordListManager.GetListOfTypeFilteredFurther(Filter11to20, SortHelper);
			IRecordList<SimpleIntTestClass> recordList11to17 =
					RecordListManager.GetListOfTypeFilteredFurther(Filter11to17, SortHelper);
			_sourceRecords.Add(new SimpleIntTestClass(15));
			Assert.AreEqual(8, recordList11to17.Count);
			Assert.AreEqual(11, recordList11to20.Count);
		}

		[Test]
		public void AddRecordToFiltered_DoesNotMeetFilterCriteria_AddedToMasterOnly()
		{
			IRecordList<SimpleIntTestClass> recordList11to20 =
					RecordListManager.GetListOfTypeFilteredFurther(Filter11to20, SortHelper);
			recordList11to20.Add(new SimpleIntTestClass(50));
			Assert.AreEqual(10, recordList11to20.Count);
			Assert.AreEqual(51, _sourceRecords.Count);
		}

		[Test]
		public void AddRecordToFiltered_AlreadyExists_NotAdded()
		{
			IRecordList<SimpleIntTestClass> recordList11to20 =
					RecordListManager.GetListOfTypeFilteredFurther(Filter11to20, SortHelper);
			int index12 = _sourceRecords.Find(SimpleIntTestClass.IPropertyDescriptor, 12);

			recordList11to20.Add(_sourceRecords[index12]);
			Assert.AreEqual(10, recordList11to20.Count);
			Assert.AreEqual(50, _sourceRecords.Count);
		}

		[Test]
		public void RemoveRecordFromMaster_RemovedFromFilteredRecordLists()
		{
			IRecordList<SimpleIntTestClass> recordList10to19 =
					RecordListManager.GetListOfTypeFilteredFurther(Filter10to19, SortHelper);
			IRecordList<SimpleIntTestClass> recordList11to17 =
					RecordListManager.GetListOfTypeFilteredFurther(Filter11to17, SortHelper);
			_sourceRecords.Remove(recordList10to19[1]);
			Assert.AreEqual(49, _sourceRecords.Count);

			int masterRecordsCount11 = CountMatching(_sourceRecords,
													 delegate(SimpleIntTestClass item) { return item.I == 11; });
			Assert.AreEqual(0, masterRecordsCount11);

			Assert.AreEqual(6, recordList11to17.Count);
			int recordList11to17Count11 = CountMatching(recordList11to17,
														delegate(SimpleIntTestClass item) { return item.I == 11; });
			Assert.AreEqual(0, recordList11to17Count11);

			Assert.AreEqual(9, recordList10to19.Count);
			int recordList10to19Count11 = CountMatching(recordList10to19,
														delegate(SimpleIntTestClass item) { return item.I == 11; });
			Assert.AreEqual(0, recordList10to19Count11);
		}

		[Test]
		public void RemoveRecordFromFiltered_RemovedFromFilteredRecordListsAndMaster()
		{
			IRecordList<SimpleIntTestClass> recordList10to19 =
					RecordListManager.GetListOfTypeFilteredFurther(Filter10to19, SortHelper);
			IRecordList<SimpleIntTestClass> recordList11to17 =
					RecordListManager.GetListOfTypeFilteredFurther(Filter11to17, SortHelper);
			recordList10to19.Remove(recordList10to19[1]);
			Assert.AreEqual(6, recordList11to17.Count);
			int recordList11to17Count11 = CountMatching(recordList11to17,
														delegate(SimpleIntTestClass item) { return item.I == 11; });
			Assert.AreEqual(0, recordList11to17Count11);
			Assert.AreEqual(9, recordList10to19.Count);
			int recordList10to19Count11 = CountMatching(recordList10to19,
														delegate(SimpleIntTestClass item) { return item.I == 11; });
			Assert.AreEqual(0, recordList10to19Count11);
			Assert.AreEqual(49, _sourceRecords.Count);
			int masterRecordsCount11 = CountMatching(_sourceRecords,
													 delegate(SimpleIntTestClass item) { return item.I == 11; });
			Assert.AreEqual(0, masterRecordsCount11);
		}

		[Test]
		public void RemoveRecord_Notified()
		{
			_recordListManager.DataDeleted += new EventHandler<DeletedItemEventArgs>(_recordListManager_DataDeleted);
			IRecordList<SimpleIntTestClass> recordList10to19 =
					RecordListManager.GetListOfTypeFilteredFurther(Filter10to19, SortHelper);
			SimpleIntTestClass i = recordList10to19[1];
			recordList10to19.Remove(i);
			Assert.AreEqual(i, _lastDeletedItem);
		}

		private SimpleIntTestClass _lastDeletedItem;

		private void _recordListManager_DataDeleted(object sender, DeletedItemEventArgs e)
		{
			_lastDeletedItem = (SimpleIntTestClass) e.ItemDeleted;
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void GetListOfTypeFilteredFurther_NullFilter_Throws()
		{
			RecordListManager.GetListOfTypeFilteredFurther(null, SortHelper);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void GetListOfTypeFilteredFurther_NullSortHelper_Throws()
		{
			RecordListManager.GetListOfTypeFilteredFurther(Filter10to19, null);
		}
	}
}