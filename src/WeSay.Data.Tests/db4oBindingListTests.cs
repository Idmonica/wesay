using System;
using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;



namespace WeSay.Data.Tests
{
	[TestFixture]
	public class Db4oBindingListIEnumerableTests : IEnumerableBaseTest<TestItem>
	{
		Db4oDataSource _dataSource;
		Db4oBindingList<TestItem> _bindingList;
		string _FilePath;

		[TestFixtureSetUp]
		public void FixtureSetUp()
		{

			_FilePath = System.IO.Path.GetTempFileName();
			this._dataSource = new Db4oDataSource(_FilePath);
			this._bindingList = new Db4oBindingList<TestItem>(new Db4oBindingListConfiguration<TestItem>(this._dataSource));

			this._enumerable = this._bindingList;
			this._bindingList.Add(new TestItem("Jared", 1, new DateTime(2003, 7, 10)));
			this._bindingList.Add(new TestItem("Gianna", 2, new DateTime(2006, 7, 17)));
			this._itemCount = 2;
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			this._dataSource.Dispose();
			System.IO.File.Delete(_FilePath);
		}
	}

	[TestFixture]
	public class Db4oBindingListIEnumerableWithNoDataTests : IEnumerableBaseTest<TestItem>
	{
		Db4oDataSource _dataSource;
		Db4oBindingList<TestItem> _bindingList;
		string _FilePath;

		[TestFixtureSetUp]
		public void FixtureSetUp()
		{

			_FilePath = System.IO.Path.GetTempFileName();
			this._dataSource = new Db4oDataSource(_FilePath);
			this._bindingList = new Db4oBindingList<TestItem>(new Db4oBindingListConfiguration<TestItem>(this._dataSource));

			this._enumerable = this._bindingList;
			this._itemCount = 0;
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			this._dataSource.Dispose();
			System.IO.File.Delete(_FilePath);
		}
	}


	[TestFixture]
	public class Db4oBindingListICollectionTest : ICollectionBaseTest<TestItem>
	{
		Db4oDataSource _dataSource;
		Db4oBindingList<TestItem> _bindingList;
		string _FilePath;

		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			_FilePath = System.IO.Path.GetTempFileName();
			this._dataSource = new Db4oDataSource(_FilePath);
			this._bindingList = new Db4oBindingList<TestItem>(new Db4oBindingListConfiguration<TestItem>(this._dataSource));

			this._collection = this._bindingList;
			this._bindingList.Add(new TestItem("Jared", 1, new DateTime(2003, 7, 10)));
			this._bindingList.Add(new TestItem("Gianna", 2, new DateTime(2006, 7, 17)));
			this._itemCount = 2;
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			this._dataSource.Dispose();
			System.IO.File.Delete(_FilePath);
		}
	}


	[TestFixture]
	public class Db4oBindingListICollectionWithNoDataTest : ICollectionBaseTest<TestItem>
	{
		Db4oDataSource _dataSource;
		Db4oBindingList<TestItem> _bindingList;
		string _FilePath;

		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			_FilePath = System.IO.Path.GetTempFileName();
			this._dataSource = new Db4oDataSource(_FilePath);
			this._bindingList = new Db4oBindingList<TestItem>(new Db4oBindingListConfiguration<TestItem>(this._dataSource));

			this._collection = this._bindingList;
			this._itemCount = 0;
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			this._dataSource.Dispose();
			System.IO.File.Delete(_FilePath);
		}
	}

	[TestFixture]
	public class Db4oBindingListIListTest : IListVariableSizeReadWriteBaseTest<TestItem>
	{
		Db4oDataSource _dataSource;
		Db4oBindingList<TestItem> _bindingList;
		string _FilePath;

		[SetUp]
		public void SetUp()
		{
			_FilePath = System.IO.Path.GetTempFileName();
			this._dataSource = new Db4oDataSource(_FilePath);
			Db4oBindingListConfiguration<TestItem> config = new Db4oBindingListConfiguration<TestItem>(this._dataSource);

			this._bindingList = new Db4oBindingList<TestItem>(config);

			this._list = this._bindingList;
			this._bindingList.Add(new TestItem("Jared", 1, new DateTime(2003, 7, 10)));
			TestItem firstItem = new TestItem("Gianna", 2, new DateTime(2006, 7, 17));
			this._bindingList.Add(firstItem);
			this._firstItem = firstItem;
			this._newItem = new TestItem();
			this._isSorted = true;
		}

		[TearDown]
		public void TearDown()
		{
			this._dataSource.Dispose();
			System.IO.File.Delete(_FilePath);
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public new void InsertNotSupported()
		{
			base.InsertNotSupported();
		}

	}

	[TestFixture]
	public class Db4oBindingListIListWithNoDataTest : IListVariableSizeReadWriteBaseTest<TestItem>
	{
		Db4oDataSource _dataSource;
		Db4oBindingList<TestItem> _bindingList;
		string _FilePath;

		[SetUp]
		public void SetUp()
		{
			_FilePath = System.IO.Path.GetTempFileName();
			this._dataSource = new Db4oDataSource(_FilePath);
			this._bindingList = new Db4oBindingList<TestItem>(new Db4oBindingListConfiguration<TestItem>(this._dataSource));

			this._list = this._bindingList;
			this._firstItem = null;
			this._newItem = new TestItem();
			this._isSorted = true;
		}

		[TearDown]
		public void TearDown()
		{
			this._dataSource.Dispose();
			System.IO.File.Delete(_FilePath);
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public new void InsertNotSupported()
		{
			base.InsertNotSupported();
		}

	}

	[TestFixture]
	public class Db4oBindingListIBindingListTest : IBindingListBaseTest<TestItem, int>
	{

		Db4oDataSource _dataSource;
		Db4oBindingList<TestItem> _db4oBindingList;
		string _FilePath;

		[TestFixtureSetUp]
		public new void TestFixtureSetUp()
		{
			_FilePath = System.IO.Path.GetTempFileName();
			this._dataSource = new Db4oDataSource(_FilePath);
			this._db4oBindingList = new Db4oBindingList<TestItem>(new Db4oBindingListConfiguration<TestItem>(this._dataSource));
			this._bindingList = this._db4oBindingList;

			PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(typeof(TestItem));
			this._property = pdc.Find("StoredInt", false);

			this._newItem = new TestItem();
			this._key = 1;
			base.TestFixtureSetUp();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			this._dataSource.Dispose();
			System.IO.File.Delete(_FilePath);
		}

		[SetUp]
		public void SetUp()
		{
			this._db4oBindingList.Add(new TestItem("Jared", 1, new DateTime(2003, 7, 10)));
			this._db4oBindingList.Add(new TestItem("Gianna", 2, new DateTime(2006, 7, 17)));
			this.ResetListChanged();
		}

		[TearDown]
		public void TearDown()
		{
			this._db4oBindingList.Clear();
		}

		protected override void VerifySort()
		{
			base.VerifySort();
			Assert.AreEqual(1, _db4oBindingList[0].StoredInt);
			Assert.AreEqual(2, _db4oBindingList[1].StoredInt);
		}

		protected override void VerifyUnsorted()
		{
			base.VerifyUnsorted();
			//Assert.AreEqual(2, _db4oBindingList[0].StoredInt);
			//Assert.AreEqual(1, _db4oBindingList[1].StoredInt);
		}

		public override void ListChangedOnChange()
		{
		}

	}

	[TestFixture]
	public class Db4oBindingListIBindingListWithNoDataTest : IBindingListBaseTest<TestItem, int>
	{

		Db4oDataSource _dataSource;
		Db4oBindingList<TestItem> _db4oBindingList;
		string _FilePath;

		[TestFixtureSetUp]
		public new void TestFixtureSetUp()
		{
			_FilePath = System.IO.Path.GetTempFileName();
			this._dataSource = new Db4oDataSource(_FilePath);
			this._db4oBindingList = new Db4oBindingList<TestItem>(new Db4oBindingListConfiguration<TestItem>(this._dataSource));
			this._bindingList = this._db4oBindingList;

			this._newItem = new TestItem();
			this._key = 1;
			PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(typeof(TestItem));
			this._property = pdc.Find("StoredInt", false);
			base.TestFixtureSetUp();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			this._dataSource.Dispose();
			System.IO.File.Delete(_FilePath);
		}

		public override void ListChangedOnChange()
		{
		}
	}


	[TestFixture]
	public class Db4oBindingListSortedTest
	{

		Db4oDataSource _dataSource;
		Db4oBindingList<TestItem> _bindingList;
		string _filePath;
		TestItem _jared, _gianna, _eric, _allison;


		private bool _listChanged;
		private ListChangedEventArgs _listChangedEventArgs;

		public void _adaptor_ListChanged(object sender, ListChangedEventArgs e)
		{
			_listChanged = true;
			_listChangedEventArgs = e;
		}

		private void AssertListChanged()
		{
			Assert.IsTrue(_listChanged);
			Assert.AreEqual(ListChangedType.Reset, this._listChangedEventArgs.ListChangedType);
			ResetListChanged();
		}
		private void ResetListChanged()
		{
			this._listChanged = false;
			this._listChangedEventArgs = null;
		}

		[SetUp]
		public void SetUp()
		{
			_filePath = System.IO.Path.GetTempFileName();
			this._dataSource = new Db4oDataSource(_filePath);
			this._bindingList = new Db4oBindingList<TestItem>(new Db4oBindingListConfiguration<TestItem>(this._dataSource));
			this._bindingList.ListChanged += new ListChangedEventHandler(_adaptor_ListChanged);

			_eric = new TestItem("Eric", 1, new DateTime(2006, 2, 28));
			_allison = new TestItem("Allison", 2, new DateTime(2006, 1, 08));
			_jared = new TestItem("Jared", 3, new DateTime(2006, 7, 10));
			_gianna = new TestItem("Gianna", 4, new DateTime(2006, 7, 17));
			this._bindingList.Add(_jared);
			this._bindingList.Add(_gianna);
			this._bindingList.Add(_eric);
			this._bindingList.Add(_allison);
			ResetListChanged();
		}

		[TearDown]
		public void TestFixtureTearDown()
		{
			this._dataSource.Dispose();
			System.IO.File.Delete(_filePath);
		}

		class StoredItemIntComparer : Comparer<TestItem>
		{
			public override int Compare(TestItem x, TestItem y)
			{
				if (x == null)
				{
					return -1;
				}
				if (y == null)
				{
					return 1;
				}
				return -(x.StoredInt - y.StoredInt);
			}
		}

		class StoredItemStringComparer : Comparer<TestItem>
		{
			public override int Compare(TestItem x, TestItem y)
			{
				int i = string.Compare(x.StoredString, y.StoredString);
				return -i;
			}
		}

		class StoredItemDateComparer : Comparer<TestItem>
		{
			public override int Compare(TestItem x, TestItem y)
			{
				int i = DateTime.Compare(x.StoredDateTime, y.StoredDateTime);
				return -i;
			}
		}

		[Test]
		public void SortIntComparer()
		{
			Assert.IsFalse(_listChanged);
			this._bindingList.Sort = new StoredItemIntComparer();
			Assert.AreEqual(_eric, this._bindingList[0]);
			Assert.AreEqual(_allison, this._bindingList[1]);
			Assert.AreEqual(_jared, this._bindingList[2]);
			Assert.AreEqual(_gianna, this._bindingList[3]);
			AssertListChanged();
		}

		[Test]
		public void SortInt()
		{
			Assert.IsFalse(_listChanged);

			PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(typeof(TestItem));
			PropertyDescriptor pd = pdc.Find("StoredInt", false);
			((IBindingList)this._bindingList).ApplySort(pd, ListSortDirection.Descending);

			Assert.AreEqual(_eric, this._bindingList[0]);
			Assert.AreEqual(_allison, this._bindingList[1]);
			Assert.AreEqual(_jared, this._bindingList[2]);
			Assert.AreEqual(_gianna, this._bindingList[3]);
			AssertListChanged();
		}

		[Test]
		public void SortDateComparer()
		{
			Assert.IsFalse(_listChanged);
			this._bindingList.Sort = new StoredItemDateComparer();
			Assert.AreEqual(_allison, this._bindingList[0]);
			Assert.AreEqual(_eric, this._bindingList[1]);
			Assert.AreEqual(_jared, this._bindingList[2]);
			Assert.AreEqual(_gianna, this._bindingList[3]);
			AssertListChanged();
		}
		[Test]
		public void SortDate()
		{
			Assert.IsFalse(_listChanged);

			PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(typeof(TestItem));
			PropertyDescriptor pd = pdc.Find("StoredDateTime", false);
			((IBindingList)this._bindingList).ApplySort(pd, ListSortDirection.Descending);

			Assert.AreEqual(_allison, this._bindingList[0]);
			Assert.AreEqual(_eric, this._bindingList[1]);
			Assert.AreEqual(_jared, this._bindingList[2]);
			Assert.AreEqual(_gianna, this._bindingList[3]);
			AssertListChanged();
		}

		[Test]
		public void SortStringComparer()
		{
			Assert.IsFalse(_listChanged);
			this._bindingList.Sort = new StoredItemStringComparer();
			Assert.AreEqual(_allison, this._bindingList[0]);
			Assert.AreEqual(_eric, this._bindingList[1]);
			Assert.AreEqual(_gianna, this._bindingList[2]);
			Assert.AreEqual(_jared, this._bindingList[3]);
			AssertListChanged();
		}

		[Test]
		public void SortString()
		{
			Assert.IsFalse(_listChanged);

			PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(typeof(TestItem));
			PropertyDescriptor pd = pdc.Find("StoredString", false);
			((IBindingList)this._bindingList).ApplySort(pd, ListSortDirection.Descending);

			Assert.AreEqual(_allison, this._bindingList[0]);
			Assert.AreEqual(_eric, this._bindingList[1]);
			Assert.AreEqual(_gianna, this._bindingList[2]);
			Assert.AreEqual(_jared, this._bindingList[3]);
			AssertListChanged();
		}

		[Test]
		public void ChangeSortComparer()
		{
			SortIntComparer();
			SortDateComparer();
			SortStringComparer();
		}

		[Test]
		public void ChangeSort()
		{
			SortInt();
			SortDate();
			SortString();
		}
	}

	[TestFixture]
	public class Db4oBindingListUpdatesToDataTest
	{
		Db4oDataSource _dataSource;
		Db4oBindingList<TestItem> _bindingList;
		string _filePath;
		TestItem _jared, _gianna;

		[SetUp]
		public void SetUp()
		{
			_filePath = System.IO.Path.GetTempFileName();
			this._dataSource = new Db4oDataSource(_filePath);
			this._bindingList = new Db4oBindingList<TestItem>(new Db4oBindingListConfiguration<TestItem>(this._dataSource));

			_jared = new TestItem("Jared", 3, new DateTime(2006, 7, 10));
			_gianna = new TestItem("Gianna", 4, new DateTime(2006, 7, 17));
			this._bindingList.Add(_jared);
			this._bindingList.Add(_gianna);
		}

		[TearDown]
		public void TestFixtureTearDown()
		{
			this._dataSource.Dispose();
			System.IO.File.Delete(_filePath);
		}

		[Test]
		public void ChangeAfterAdd()
		{
			_jared.StoredInt = 1;

			Assert.AreEqual(_jared, _bindingList[1]);
			Assert.AreEqual(1, _bindingList[1].StoredInt);

			this._dataSource.Dispose();
			this._dataSource = new Db4oDataSource(_filePath);
			this._bindingList = new Db4oBindingList<TestItem>(new Db4oBindingListConfiguration<TestItem>(this._dataSource));

			Assert.AreEqual(_jared, _bindingList[1]);
			Assert.AreEqual(1, _bindingList[1].StoredInt);
		}

		[Test]
		public void ChangeAfterGet()
		{
			TestItem item = _bindingList[0];
			item.StoredInt = 1;
			item = _bindingList[1];
			item.StoredInt = 2;

			this._dataSource.Dispose();
			this._dataSource = new Db4oDataSource(_filePath);
			this._bindingList = new Db4oBindingList<TestItem>(new Db4oBindingListConfiguration<TestItem>(this._dataSource));

			Assert.AreEqual(1, _bindingList[0].StoredInt);
			Assert.AreEqual(2, _bindingList[1].StoredInt);
		}

		[Test]
		public void ChangeAfterAddNew()
		{
			TestItem item = (TestItem)((IBindingList)_bindingList).AddNew();
			int index = _bindingList.IndexOf(item);
			Assert.AreEqual(0, item.StoredInt);
			item.StoredInt = 11;

			this._dataSource.Dispose();
			this._dataSource = new Db4oDataSource(_filePath);
			this._bindingList = new Db4oBindingList<TestItem>(new Db4oBindingListConfiguration<TestItem>(this._dataSource));

			Assert.AreEqual(item, _bindingList[index]);
			Assert.AreEqual(11, _bindingList[index].StoredInt);
		}

	}

}
