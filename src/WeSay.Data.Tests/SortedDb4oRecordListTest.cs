using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Db4objects.Db4o;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Query;
using NUnit.Framework;
using WeSay.Data.Tests.IEnumerableTests;

namespace WeSay.Data.Tests
{
	class TestSortHelper : IDb4oSortHelper<int, SimpleIntTestClass>
	{
		IExtObjectContainer _database;
		public TestSortHelper(IExtObjectContainer database)
		{
			if (database == null)
			{
				throw new ArgumentNullException();
			}
			_database = database;
		}
		#region IDb4oSortHelper<int,SimpleIntTestClass> Members

		public IComparer<int> KeyComparer
		{
			get
			{
				return Comparer<int>.Default;
			}
		}

		public System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, long>> GetKeyIdPairs()
		{
			IQuery query = _database.Query();
			query.Constrain(typeof(SimpleIntTestClass));
			IObjectSet simpleIntTests = query.Execute();

			List<KeyValuePair<int, long>> result = new List<KeyValuePair<int, long>>();

			foreach (SimpleIntTestClass simpleIntTest in simpleIntTests)
			{
				result.Add(new KeyValuePair<int, long>(simpleIntTest.I, _database.GetID(simpleIntTest)));
			}

			return result;
		}

		public System.Collections.Generic.IEnumerable<int> GetKeys(SimpleIntTestClass item)
		{
			List<int> result = new List<int>();
			result.Add(item.I);
			return result;
		}

		public string Name
		{
			get
			{
				return "identity";
			}
		}
		#endregion
	}

	[TestFixture]
	public class CachedSortedDb4oListIEnumerableTests : IEnumerableBaseTest<int>
	{
		Db4oDataSource _dataSource;
		CachedSortedDb4oList<int, SimpleIntTestClass> _recordList;
		string _FilePath;
		Db4oRecordListManager _manager;

		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			_FilePath = System.IO.Path.GetTempFileName();
			this._dataSource = new Db4oDataSource(_FilePath);
			this._manager = new Db4oRecordListManager(new DoNothingModelConfiguration(), this._FilePath);
			this._recordList = this._manager.GetSortedList<int, SimpleIntTestClass>(new TestSortHelper(_dataSource.Data.Ext()));

			this._enumerable = this._recordList;
			this._recordList.Add(new SimpleIntTestClass(12));
			this._recordList.Add(new SimpleIntTestClass(21));
			this._itemCount = 2;
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			this._manager.Dispose();
			this._dataSource.Dispose();
			System.IO.File.Delete(_FilePath);
		}
	}

	[TestFixture]
	public class CachedSortedDb4oListIEnumerableWithNoDataTests : IEnumerableBaseTest<int>
	{
		Db4oDataSource _dataSource;
		CachedSortedDb4oList<int, SimpleIntTestClass> _recordList;
		string _FilePath;
		Db4oRecordListManager _manager;

		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			_FilePath = System.IO.Path.GetTempFileName();
			this._dataSource = new Db4oDataSource(_FilePath);
			this._manager = new Db4oRecordListManager(new DoNothingModelConfiguration(), this._FilePath);
			this._recordList = this._manager.GetSortedList<int, SimpleIntTestClass>(new TestSortHelper(_dataSource.Data.Ext()));


			this._enumerable = this._recordList;
			this._itemCount = 0;
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			this._manager.Dispose();
			this._dataSource.Dispose();
			System.IO.File.Delete(_FilePath);
		}
	}


	[TestFixture]
	public class SortedDb4oRecordListTest
	{
		Db4oDataSource _dataSource;
		string _FilePath;
		Db4oRecordListManager _manager;
		CachedSortedDb4oList<int, SimpleIntTestClass> _sortedList;

		[SetUp]
		public void SetUp()
		{
			_FilePath = System.IO.Path.GetTempFileName();
			this._dataSource = new Db4oDataSource(_FilePath);

			for (int i = 50 - 1; i >= 0; i--)
			{
				this._dataSource.Data.Set(new SimpleIntTestClass(i));
			}
			this._dataSource.Data.Commit();

			this._manager = new Db4oRecordListManager(new DoNothingModelConfiguration(), this._FilePath);

			this._sortedList = this._manager.GetSortedList<int, SimpleIntTestClass>(new TestSortHelper(_dataSource.Data.Ext()));
		}

		[TearDown]
		public void TearDown()
		{
			_manager.Dispose();
			this._dataSource.Dispose();
			System.IO.File.Delete(_FilePath);
		}
		[Test]
		public void Construct()
		{
			CachedSortedDb4oList<int, SimpleIntTestClass> sortedList = new CachedSortedDb4oList<int, SimpleIntTestClass>(_manager, new TestSortHelper(_dataSource.Data.Ext()));
			sortedList.Dispose();
		}


		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Construct_NullRecordListManager_Throws()
		{
			new CachedSortedDb4oList<int, SimpleIntTestClass>(null, new TestSortHelper(_dataSource.Data.Ext()));
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Construct_NullSortHelper_Throws()
		{
			using (Db4oRecordListManager manager = new Db4oRecordListManager(new DoNothingModelConfiguration(), this._FilePath))
			{
				new CachedSortedDb4oList<int, SimpleIntTestClass>(manager, null);
			}
		}

		[Test]
		public void Add()
		{
			SimpleIntTestClass item = new SimpleIntTestClass(2);
			_sortedList.Add(item);
			Assert.AreEqual(2, _sortedList[2]);
			Assert.AreEqual(2, _sortedList[3]);
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void AddIndex()
		{
			_sortedList.AddIndex(null);
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void AddNew()
		{
			_sortedList.AddNew();
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void ApplySort()
		{
			_sortedList.ApplySort(null, System.ComponentModel.ListSortDirection.Ascending);
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void Find()
		{
			_sortedList.Find(null, null);
		}

		[Test]
		public void BinarySearch()
		{
			Assert.AreEqual(3, _sortedList.BinarySearch(3));
		}

		[Test]
		public void IsSorted()
		{
			Assert.IsTrue(_sortedList.IsSorted);
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void RemoveIndex()
		{
			_sortedList.RemoveIndex(null);
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void RemoveSort()
		{
			_sortedList.RemoveSort();
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void SortDirection()
		{
			ListSortDirection direction = _sortedList.SortDirection;
		}
		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void SortProperty()
		{
			PropertyDescriptor pd = _sortedList.SortProperty;
		}
		[Test]
		public void SupportsChangeNotification()
		{
			Assert.IsTrue(_sortedList.SupportsChangeNotification);
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void Clear()
		{
			_sortedList.Clear();
		}
		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void Contains()
		{
			_sortedList.Contains(null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void IndexOf_nonT_Throws()
		{
			_sortedList.IndexOf(3);
		}

		[Test]
		public void IndexOf_T()
		{
			SimpleIntTestClass s = _sortedList.GetValue(30);
			Assert.AreEqual(30, _sortedList.IndexOf(s));
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void Insert()
		{
			_sortedList.Insert(0, null);
		}
		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void Remove()
		{
			_sortedList.Remove(null);
		}
		[Test]
		public void RemoveAt()
		{
			_sortedList.RemoveAt(12);
			Assert.AreEqual(49, _sortedList.Count);
			Assert.AreEqual(13, _sortedList.GetKey(12));
		}

		[Test]
		public void Get_Indexer()
		{
			object o = _sortedList[12];
			Assert.AreEqual((object) 12, o);
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void Set_Indexer()
		{
			_sortedList[0] = new SimpleIntTestClass(1);
		}

		[Test]
		public void GetKey()
		{
			Assert.AreEqual(12, _sortedList.GetKey(12));
		}

		[Test]
		public void GetValue()
		{
			Assert.AreEqual(12, _sortedList.GetValue(12).I);
		}

		[Test]
		public void GetId()
		{
			long id = _sortedList.GetId(12);
			SimpleIntTestClass o = (SimpleIntTestClass)_dataSource.Data.Ext().GetByID(id);
			Assert.AreEqual(12, o.I);
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void CopyTo()
		{
			_sortedList.CopyTo(null, 0);
		}

		[Test]
		public void Count()
		{
			Assert.AreEqual(50, _sortedList.Count);
		}
		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void SyncRoot()
		{
			object o = _sortedList.SyncRoot;
		}
	}
}
