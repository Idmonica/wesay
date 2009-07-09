using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Palaso.Data;

namespace WeSay.Data.Tests // review cp Move to Palaso when we get rid of QueryAdapter
{
	public abstract class IRepositoryStateUnitializedTests<T> where T : class, new()
	{
		private IDataMapper<T> dataMapperUnderTest;
		private readonly QueryAdapter<T> _query = new QueryAdapter<T>();

		public IDataMapper<T> DataMapperUnderTest
		{
			get
			{
				if (dataMapperUnderTest == null)
				{
					throw new InvalidOperationException(
							"DataMapperUnderTest must be set before the tests are run.");
				}
				return dataMapperUnderTest;
			}
			set { dataMapperUnderTest = value; }
		}

		[SetUp]
		public abstract void SetUp();

		[TearDown]
		public abstract void TearDown();

		[Test]
		public void CreateItem_NotNull()
		{
			Assert.IsNotNull(DataMapperUnderTest.CreateItem());
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void DeleteItem_Null_Throws()
		{
			DataMapperUnderTest.DeleteItem((T) null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void DeleteItem_ItemDoesNotExist_Throws()
		{
			T item = new T();
			DataMapperUnderTest.DeleteItem(item);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void DeleteItemById_Null_Throws()
		{
			DataMapperUnderTest.DeleteItem((RepositoryId) null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void DeleteItemById_ItemDoesNotExist_Throws()
		{
			MyRepositoryId id = new MyRepositoryId();
			DataMapperUnderTest.DeleteItem(id);
		}

		[Test]
		public void DeleteAllItems_NothingInRepository_StillNothingInRepository()
		{
			DataMapperUnderTest.DeleteAllItems();
			Assert.AreEqual(0, DataMapperUnderTest.CountAllItems());
		}

		[Test]
		public void CountAllItems_NoItemsInTheRepostory_ReturnsZero()
		{
			Assert.AreEqual(0, DataMapperUnderTest.CountAllItems());
		}

		[Test]
		public void GetAllItems_ReturnsEmptyArray()
		{
			Assert.IsEmpty(DataMapperUnderTest.GetAllItems());
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void GetId_ItemNotInRepository_Throws()
		{
			T item = new T();
			DataMapperUnderTest.GetId(item);
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void GetItem_IdNotInRepository_Throws()
		{
			MyRepositoryId id = new MyRepositoryId();
			DataMapperUnderTest.GetItem(id);
		}

		[Test]
		[ExpectedException(typeof (NotSupportedException))]
		public void GetItemsMatchingQuery_CanQueryIsFalse_Throws()
		{
			if (!DataMapperUnderTest.CanQuery)
			{
				DataMapperUnderTest.GetItemsMatching(_query);
			}
			else
			{
				Assert.Ignore("Test not relevant. This repository supports queries.");
			}
		}

		[Test]
		public void GetItemMatchingQuery_Query_ReturnsEmpty()
		{
			if (DataMapperUnderTest.CanQuery)
			{
				Assert.AreEqual(0, DataMapperUnderTest.GetItemsMatching(_query).Count);
			}
			else
			{
				Assert.Ignore("Repository does not support queries.");
			}
		}

		[Test]
		public void LastModified_ReturnsMinimumPossibleTime()
		{
			Assert.AreEqual(new DateTime(DateTime.MinValue.Ticks, DateTimeKind.Utc), DataMapperUnderTest.LastModified);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void Save_Null_Throws()
		{
			DataMapperUnderTest.SaveItem(null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void Save_ItemDoesNotExist_Throws()
		{
			T item = new T();
			DataMapperUnderTest.SaveItem(item);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void SaveItems_Null_Throws()
		{
			DataMapperUnderTest.SaveItems(null);
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void SaveItems_ItemDoesNotExist_Throws()
		{
			T item = new T();
			List<T> itemsToSave = new List<T>();
			itemsToSave.Add(item);
			DataMapperUnderTest.SaveItems(itemsToSave);
		}

		[Test]
		public void SaveItems_ListIsEmpty_DoNotChangeLastModified()
		{
			List<T> itemsToSave = new List<T>();
			DateTime modifiedTimePreTestedStateSwitch = DataMapperUnderTest.LastModified;
			DataMapperUnderTest.SaveItems(itemsToSave);
			Assert.AreEqual(modifiedTimePreTestedStateSwitch, DataMapperUnderTest.LastModified);
		}

		private class MyRepositoryId: RepositoryId
		{
			public override int CompareTo(RepositoryId other)
			{
				return 0;
			}

			public override bool Equals(RepositoryId other)
			{
				return true;
			}
		}
	}

	public abstract class IRepositoryCreateItemTransitionTests<T> where T : class, new()
	{
		private IDataMapper<T> dataMapperUnderTest;
		private T item;
		private RepositoryId id;

		public IDataMapper<T> DataMapperUnderTest
		{
			get
			{
				if (dataMapperUnderTest == null)
				{
					throw new InvalidOperationException(
							"DataMapperUnderTest must be set before the tests are run.");
				}
				return dataMapperUnderTest;
			}
			set { dataMapperUnderTest = value; }
		}

		protected T Item
		{
			get { return item; }
			set { item = value; }
		}

		protected RepositoryId Id
		{
			get { return id; }
			set { id = value; }
		}

		public void SetState()
		{
			Item = DataMapperUnderTest.CreateItem();
			Id = DataMapperUnderTest.GetId(Item);
		}

		//This method is used to test whether data has been persisted.
		//This method should dispose of the current repository and reload it from persisted data
		//For repositories that don't support persistence this method should do nothing
		protected abstract void CreateNewRepositoryFromPersistedData();

		[SetUp]
		public abstract void SetUp();

		[TearDown]
		public abstract void TearDown();

		[Test]
		public void CreateItem_ReturnsUniqueItem()
		{
			SetState();
			Assert.AreNotEqual(Item, DataMapperUnderTest.CreateItem());
		}

		[Test]
		public void CreatedItemHasBeenPersisted()
		{
			SetState();
			if (!DataMapperUnderTest.CanPersist) {}
			else
			{
				CreateNewRepositoryFromPersistedData();
				RepositoryId[] listOfItems = DataMapperUnderTest.GetAllItems();
				Assert.AreEqual(1, listOfItems.Length);
				//Would be nice if this worked.. but it doesn't because we have equals for LexEntry is still by reference
				//T itemFromPersistedData = DataMapperUnderTest.GetItem(listOfItems[0]);
				//Assert.AreEqual(item, itemFromPersistedData);
			}
		}

		[Test]
		public void CountAllItems_ReturnsOne()
		{
			SetState();
			Assert.AreEqual(1, DataMapperUnderTest.CountAllItems());
		}

		[Test]
		public void GetAllItems_ReturnsIdItem()
		{
			SetState();
			Assert.AreEqual(DataMapperUnderTest.GetId(Item), DataMapperUnderTest.GetAllItems()[0]);
		}

		[Test]
		public void GetAllItems_ReturnsCorrectNumberOfExistingItems()
		{
			SetState();
			Assert.AreEqual(1, DataMapperUnderTest.GetAllItems().Length);
		}

		[Test]
		public void GetId_CalledTwiceWithSameItem_ReturnsSameId()
		{
			SetState();
			Assert.AreEqual(DataMapperUnderTest.GetId(Item), DataMapperUnderTest.GetId(Item));
		}

		[Test]
		public void GetId_Item_ReturnsIdOfItem()
		{
			SetState();
			Assert.AreEqual(Id, DataMapperUnderTest.GetId(Item));
		}

		[Test]
		public void GetItem_Id_ReturnsItemWithId()
		{
			SetState();
			Assert.AreSame(Item, DataMapperUnderTest.GetItem(Id));
		}

		[Test]
		public void GetItem_CalledTwiceWithSameId_ReturnsSameItem()
		{
			SetState();
			Assert.AreSame(DataMapperUnderTest.GetItem(Id), DataMapperUnderTest.GetItem(Id));
		}

		[Test]
		public void GetItemsMatchingQuery_QueryWithOutShow_ReturnsNoItems()
		{
			QueryAdapter<T> queryWithoutShow = new QueryAdapter<T>();
			SetState();
			if (DataMapperUnderTest.CanQuery)
			{
				ResultSet<T> resultSet = DataMapperUnderTest.GetItemsMatching(queryWithoutShow);
				Assert.AreEqual(0, resultSet.Count);
			}
			else
			{
				Assert.Ignore("Repository does not support queries.");
			}
		}

		[Test]
		public void GetItemsMatchingQuery_QueryWithShow_ReturnsAllItemsAndFieldsMatchingQuery()
		{
			SetState();
			GetItemsMatchingQuery_QueryWithShow_ReturnAllItemsMatchingQuery_v();
		}

		protected virtual void GetItemsMatchingQuery_QueryWithShow_ReturnAllItemsMatchingQuery_v()
		{
			if (DataMapperUnderTest.CanQuery)
			{
				Assert.Fail(
					@"This Test is highly dependant on the type of objects that are
							being managed by the repository and as such should be overridden.");
			}
			else
			{
				Assert.Ignore("Repository does not support queries.");
			}
		}

		[Test]
		public void SaveItem_LastModifiedIsChangedToLaterTime()
		{
			SetState();
			DateTime modifiedTimePreTestedStateSwitch = DataMapperUnderTest.LastModified;
			DataMapperUnderTest.SaveItem(Item);
			Assert.Greater(DataMapperUnderTest.LastModified, modifiedTimePreTestedStateSwitch);
		}

		[Test]
		public void SaveItem_LastModifiedIsSetInUTC()
		{
			SetState();
			DataMapperUnderTest.SaveItem(Item);
			Assert.AreEqual(DateTimeKind.Utc, DataMapperUnderTest.LastModified.Kind);
		}

		[Test]
		public void SaveItem_ItemHasBeenPersisted()
		{
			SetState();
			if (!DataMapperUnderTest.CanPersist)
			{
				Assert.Ignore("Repository can not be persisted");
			}
			else
			{
				DataMapperUnderTest.SaveItem(Item);
				CreateNewRepositoryFromPersistedData();
				Assert.AreEqual(1, DataMapperUnderTest.CountAllItems());
			}
		}

		[Test]
		public void SaveItems_LastModifiedIsChangedToLaterTime()
		{
			SetState();
			List<T> itemsToSave = new List<T>();
			itemsToSave.Add(Item);
			DateTime modifiedTimePreTestedStateSwitch = DataMapperUnderTest.LastModified;
			DataMapperUnderTest.SaveItems(itemsToSave);
			Assert.Greater(DataMapperUnderTest.LastModified, modifiedTimePreTestedStateSwitch);
		}

		[Test]
		public void SaveItems_LastModifiedIsSetInUTC()
		{
			SetState();
			List<T> itemsToSave = new List<T>();
			itemsToSave.Add(Item);
			Thread.Sleep(50);
			DataMapperUnderTest.SaveItems(itemsToSave);
			Assert.AreEqual(DateTimeKind.Utc, DataMapperUnderTest.LastModified.Kind);
		}

		[Test]
		public void SaveItems_ItemHasBeenPersisted()
		{
			SetState();
			if (!DataMapperUnderTest.CanPersist)
			{
				Assert.Ignore("Repository can not be persisted");
			}
			else
			{
				List<T> itemsToBeSaved = new List<T>();
				itemsToBeSaved.Add(Item);
				DataMapperUnderTest.SaveItems(itemsToBeSaved);
				CreateNewRepositoryFromPersistedData();
				Assert.AreEqual(1, DataMapperUnderTest.CountAllItems());
			}
		}
	}

	public abstract class IRepositoryPopulateFromPersistedTests<T> where T : class, new()
	{
		private IDataMapper<T> dataMapperUnderTest;
		private T item;
		private RepositoryId id;

		public IDataMapper<T> DataMapperUnderTest
		{
			get
			{
				if (dataMapperUnderTest == null)
				{
					throw new InvalidOperationException(
							"DataMapperUnderTest must be set before the tests are run.");
				}
				return dataMapperUnderTest;
			}
			set { dataMapperUnderTest = value; }
		}

		protected T Item
		{
			get { return item; }
			set { item = value; }
		}

		protected RepositoryId Id
		{
			get { return id; }
			set { id = value; }
		}

		public void SetState()
		{
			RepositoryId[] idsFrompersistedData = DataMapperUnderTest.GetAllItems();
			Id = idsFrompersistedData[0];
			Item = DataMapperUnderTest.GetItem(Id);
		}

		//This method is used to test whether data has been persisted.
		//This method should dispose of the current repository and reload it from persisted data
		//For repositories that don't support persistence this method should do nothing
		protected abstract void CreateNewRepositoryFromPersistedData();

		[SetUp]
		public abstract void SetUp();

		[TearDown]
		public abstract void TearDown();

		[Test]
		public void CreateItem_ReturnsUniqueItem()
		{
			SetState();
			Assert.AreNotEqual(Item, DataMapperUnderTest.CreateItem());
		}

		[Test]
		public void CreatedItemHasBeenPersisted()
		{
			SetState();
			if (!DataMapperUnderTest.CanPersist) {}
			else
			{
				CreateNewRepositoryFromPersistedData();
				RepositoryId[] listOfItems = DataMapperUnderTest.GetAllItems();
				Assert.AreEqual(1, listOfItems.Length);
				//Would be nice if this worked.. but it doesn't because we have equals for LexEntry is still by reference
				//T itemFromPersistedData = DataMapperUnderTest.GetItem(listOfItems[0]);
				//Assert.AreEqual(item, itemFromPersistedData);
			}
		}

		[Test]
		public void CountAllItems_ReturnsOne()
		{
			SetState();
			Assert.AreEqual(1, DataMapperUnderTest.CountAllItems());
		}

		[Test]
		public void GetAllItems_ReturnsIdItem()
		{
			SetState();
			Assert.AreEqual(DataMapperUnderTest.GetId(Item), DataMapperUnderTest.GetAllItems()[0]);
		}

		[Test]
		public void GetAllItems_ReturnsCorrectNumberOfExistingItems()
		{
			SetState();
			Assert.AreEqual(1, DataMapperUnderTest.GetAllItems().Length);
		}

		[Test]
		public void GetId_CalledTwiceWithSameItem_ReturnsSameId()
		{
			SetState();
			Assert.AreEqual(DataMapperUnderTest.GetId(Item), DataMapperUnderTest.GetId(Item));
		}

		[Test]
		public void GetId_Item_ReturnsIdOfItem()
		{
			SetState();
			Assert.AreEqual(Id, DataMapperUnderTest.GetId(Item));
		}

		[Test]
		public void GetItem_Id_ReturnsItemWithId()
		{
			SetState();
			Assert.AreSame(Item, DataMapperUnderTest.GetItem(Id));
		}

		[Test]
		public void GetItem_CalledTwiceWithSameId_ReturnsSameItem()
		{
			SetState();
			Assert.AreSame(DataMapperUnderTest.GetItem(Id), DataMapperUnderTest.GetItem(Id));
		}

		[Test]
		public void GetItemMatchingQuery_QueryWithOutShow_ReturnsNoItems()
		{
			QueryAdapter<T> queryWithoutShow = new QueryAdapter<T>();
			SetState();
			if (DataMapperUnderTest.CanQuery)
			{
				ResultSet<T> resultSet = DataMapperUnderTest.GetItemsMatching(queryWithoutShow);
				Assert.AreEqual(0, resultSet.Count);
			}
			else
			{
				Assert.Ignore("Repository does not support queries.");
			}
		}

		[Test]
		public void GetItemMatchingQuery_QueryWithShow_ReturnsAllItemsAndFieldsMatchingQuery()
		{
			SetState();
			GetItemMatchingQuery_QueryWithShow_ReturnsAllItemsAndFieldsMatchingQuery_v();
		}

		protected virtual void GetItemMatchingQuery_QueryWithShow_ReturnsAllItemsAndFieldsMatchingQuery_v()
		{
			if (DataMapperUnderTest.CanQuery)
			{
				Assert.Fail(
					@"This Test is highly dependant on the type of objects that are
							being managed by the repository and as such should be tested elsewhere.");
			}
			else
			{
				Assert.Ignore("Repository does not support queries.");
			}
		}

		[Test]
		public void LastModified_IsSetToMostRecentItemInPersistedDatasLastModifiedTime()
		{
			CreateNewRepositoryFromPersistedData();
			SetState();
			LastModified_IsSetToMostRecentItemInPersistedDatasLastModifiedTime_v();
		}

		protected virtual void LastModified_IsSetToMostRecentItemInPersistedDatasLastModifiedTime_v()
		{
			if (!DataMapperUnderTest.CanPersist)
			{
				Assert.Ignore("Repository can not be persisted");
			}
			else
			{
				Assert.Fail(
					"This test is dependant on how you are persisting your data, please override this test.");
			}
		}

		[Test]
		public void LastModified_IsSetInUTC()
		{
			SetState();
			Assert.AreEqual(DateTimeKind.Utc, DataMapperUnderTest.LastModified.Kind);
		}

		//This test is virtual because LexEntryRepository needs a special implementation
		[Test]
		public virtual void SaveItem_LastModifiedIsChangedToLaterTime()
		{
			SetState();
			DateTime modifiedTimePreSave = DataMapperUnderTest.LastModified;
			DataMapperUnderTest.SaveItem(Item);
			Assert.Greater(DataMapperUnderTest.LastModified, modifiedTimePreSave);
		}

		//This test is virtual because LexEntryRepository needs a special implementation
		[Test]
		public virtual void SaveItem_LastModifiedIsSetInUTC()
		{
			SetState();
			DataMapperUnderTest.SaveItem(Item);
			Assert.AreEqual(DateTimeKind.Utc, DataMapperUnderTest.LastModified.Kind);
		}

		//This test is virtual because LexEntryRepository needs a special implementation
		[Test]
		public virtual void SaveItems_LastModifiedIsChangedToLaterTime()
		{
			SetState();
			List<T> itemsToSave = new List<T>();
			itemsToSave.Add(Item);
			DateTime modifiedTimePreSave = DataMapperUnderTest.LastModified;
			DataMapperUnderTest.SaveItems(itemsToSave);
			Assert.Greater(DataMapperUnderTest.LastModified, modifiedTimePreSave);
		}

		[Test]
		public void SaveItems_LastModifiedIsSetInUTC()
		{
			SetState();
			List<T> itemsToSave = new List<T>();
			itemsToSave.Add(Item);
			Thread.Sleep(50);
			DataMapperUnderTest.SaveItems(itemsToSave);
			Assert.AreEqual(DateTimeKind.Utc, DataMapperUnderTest.LastModified.Kind);
		}
	}

	public abstract class IRepositoryDeleteItemTransitionTests<T> where T : class, new()
	{
		private IDataMapper<T> dataMapperUnderTest;
		private T item;
		private RepositoryId id;
		private readonly QueryAdapter<T> query = new QueryAdapter<T>();

		public IDataMapper<T> DataMapperUnderTest
		{
			get
			{
				if (dataMapperUnderTest == null)
				{
					throw new InvalidOperationException(
							"DataMapperUnderTest must be set before the tests are run.");
				}
				return dataMapperUnderTest;
			}
			set { dataMapperUnderTest = value; }
		}

		public T Item
		{
			get { return item; }
		}

		//This method is used to test whether data has been persisted.
		//This method should dispose of the current repository and reload it from persisted data
		//For repositories that don't support persistence this method should do nothing
		protected abstract void CreateNewRepositoryFromPersistedData();

		[SetUp]
		public abstract void SetUp();

		[TearDown]
		public abstract void TearDown();

		public void SetState()
		{
			CreateInitialItem();
			DeleteItem();
		}

		private void DeleteItem()
		{
			DataMapperUnderTest.DeleteItem(Item);
		}

		private void CreateInitialItem()
		{
			this.item = DataMapperUnderTest.CreateItem();
			this.id = DataMapperUnderTest.GetId(Item);
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void DeleteItem_ItemDoesNotExist_Throws()
		{
			SetState();
			DataMapperUnderTest.DeleteItem(Item);
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void DeleteItem_HasBeenPersisted()
		{
			SetState();
			if (!DataMapperUnderTest.CanPersist)
			{
				Assert.Ignore("Repository can not be persisted.");
			}
			else
			{
				CreateNewRepositoryFromPersistedData();
				DataMapperUnderTest.GetItem(id);
			}
		}

		[Test]
		public void CountAllItems_ReturnsZero()
		{
			SetState();
			Assert.AreEqual(0, DataMapperUnderTest.CountAllItems());
		}

		[Test]
		public void GetAllItems_ReturnsEmptyArray()
		{
			SetState();
			Assert.IsEmpty(DataMapperUnderTest.GetAllItems());
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void GetId_DeletedItemWithId_Throws()
		{
			SetState();
			DataMapperUnderTest.GetId(Item);
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void GetItem_DeletedItem_Throws()
		{
			SetState();
			DataMapperUnderTest.GetItem(id);
		}

		[Test]
		public void GetItemMatchingQuery_Query_ReturnsEmpty()
		{
			SetState();
			if (DataMapperUnderTest.CanQuery)
			{
				Assert.AreEqual(0, DataMapperUnderTest.GetItemsMatching(query).Count);
			}
			else
			{
				Assert.Ignore("Repository does not support queries.");
			}
		}

		[Test]
		public void LastModified_IsChangedToLaterTime()
		{
			CreateInitialItem();
			DateTime modifiedTimePreTestedStateSwitch = DataMapperUnderTest.LastModified;
			DeleteItem();
			Assert.Greater(DataMapperUnderTest.LastModified, modifiedTimePreTestedStateSwitch);
		}

		[Test]
		public void LastModified_IsSetInUTC()
		{
			SetState();
			Assert.AreEqual(DateTimeKind.Utc, DataMapperUnderTest.LastModified.Kind);
		}

		//This test is virtual because LexEntryRepository needs to override it
		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public virtual void SaveItem_ItemDoesNotExist_Throws()
		{
			SetState();
			DataMapperUnderTest.SaveItem(Item);
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void SaveItems_ItemDoesNotExist_Throws()
		{
			SetState();
			T itemNotInRepository = new T();
			List<T> itemsToSave = new List<T>();
			itemsToSave.Add(itemNotInRepository);
			DataMapperUnderTest.SaveItems(itemsToSave);
		}
	}

	public abstract class IRepositoryDeleteIdTransitionTests<T> where T : class, new()
	{
		private IDataMapper<T> dataMapperUnderTest;
		private T item;
		private RepositoryId id;
		private readonly QueryAdapter<T> query = new QueryAdapter<T>();

		public IDataMapper<T> DataMapperUnderTest
		{
			get
			{
				if (dataMapperUnderTest == null)
				{
					throw new InvalidOperationException(
							"DataMapperUnderTest must be set before the tests are run.");
				}
				return dataMapperUnderTest;
			}
			set { dataMapperUnderTest = value; }
		}

		public T Item
		{
			get { return item; }
		}

		//This method is used to test whether data has been persisted.
		//This method should dispose of the current repository and reload it from persisted data
		//For repositories that don't support persistence this method should do nothing
		protected abstract void CreateNewRepositoryFromPersistedData();

		[SetUp]
		public abstract void SetUp();

		[TearDown]
		public abstract void TearDown();

		public void SetState()
		{
			CreateItemToTest();
			DeleteItem();
		}

		private void DeleteItem()
		{
			DataMapperUnderTest.DeleteItem(this.id);
		}

		private void CreateItemToTest()
		{
			this.item = DataMapperUnderTest.CreateItem();
			this.id = DataMapperUnderTest.GetId(Item);
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void DeleteItem_ItemDoesNotExist_Throws()
		{
			SetState();
			DataMapperUnderTest.DeleteItem(id);
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void DeleteItem_HasBeenPersisted()
		{
			SetState();
			if (!DataMapperUnderTest.CanPersist)
			{
				Assert.Ignore("Repository can not be persisted.");
			}
			else
			{
				CreateNewRepositoryFromPersistedData();
				DataMapperUnderTest.GetItem(id);
			}
		}

		[Test]
		public void CountAllItems_ReturnsZero()
		{
			SetState();
			Assert.AreEqual(0, DataMapperUnderTest.CountAllItems());
		}

		[Test]
		public void GetAllItems_ReturnsEmptyArray()
		{
			SetState();
			Assert.IsEmpty(DataMapperUnderTest.GetAllItems());
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void GetId_DeletedItemWithId_Throws()
		{
			SetState();
			DataMapperUnderTest.GetId(Item);
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void GetItem_DeletedItem_Throws()
		{
			SetState();
			DataMapperUnderTest.GetItem(id);
		}

		[Test]
		public void GetItemMatchingQuery_Query_ReturnsEmpty()
		{
			SetState();
			if (DataMapperUnderTest.CanQuery)
			{
				Assert.AreEqual(0, DataMapperUnderTest.GetItemsMatching(query).Count);
			}
			else
			{
				Assert.Ignore("Repository does not support queries.");
			}
		}

		[Test]
		public void LastModified_ItemIsDeleted_IsChangedToLaterTime()
		{
			CreateItemToTest();
			DateTime modifiedTimePreTestedStateSwitch = DataMapperUnderTest.LastModified;
			DeleteItem();
			Assert.Greater(DataMapperUnderTest.LastModified, modifiedTimePreTestedStateSwitch);
		}

		[Test]
		public void LastModified_ItemIsDeleted_IsSetInUTC()
		{
			SetState();
			Assert.AreEqual(DateTimeKind.Utc, DataMapperUnderTest.LastModified.Kind);
		}

		//This test is virtual because LexEntryRepository needs to override it
		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public virtual void SaveItem_ItemDoesNotExist_Throws()
		{
			SetState();
			DataMapperUnderTest.SaveItem(Item);
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void SaveItems_ItemDoesNotExist_Throws()
		{
			SetState();
			T itemNotInRepository = new T();
			List<T> itemsToSave = new List<T>();
			itemsToSave.Add(itemNotInRepository);
			DataMapperUnderTest.SaveItems(itemsToSave);
		}
	}

	public abstract class IRepositoryDeleteAllItemsTransitionTests<T> where T : class, new()
	{
		private IDataMapper<T> dataMapperUnderTest;
		private T item;
		private RepositoryId id;
		private readonly QueryAdapter<T> query = new QueryAdapter<T>();

		public IDataMapper<T> DataMapperUnderTest
		{
			get
			{
				if (dataMapperUnderTest == null)
				{
					throw new InvalidOperationException(
							"DataMapperUnderTest must be set before the tests are run.");
				}
				return dataMapperUnderTest;
			}
			set { dataMapperUnderTest = value; }
		}

		//This method is used to test whether data has been persisted.
		//This method should dispose of the current repository and reload it from persisted data
		//For repositories that don't support persistence this method should do nothing
		protected abstract void RepopulateRepositoryFromPersistedData();

		[SetUp]
		public abstract void SetUp();

		[TearDown]
		public abstract void TearDown();

		public void SetState()
		{
			CreateInitialItem();
			DeleteAllItems();
		}

		private void DeleteAllItems()
		{
			DataMapperUnderTest.DeleteAllItems();
		}

		private void CreateInitialItem()
		{
			this.item = DataMapperUnderTest.CreateItem();
			this.id = DataMapperUnderTest.GetId(this.item);
		}

		[Test]
		public void DeleteAllItems_ItemDoesNotExist_DoesNotThrow()
		{
			SetState();
			DataMapperUnderTest.DeleteAllItems();
		}

		[Test]
		public void DeleteAllItems_HasBeenPersisted()
		{
			SetState();
			if (!DataMapperUnderTest.CanPersist)
			{
				Assert.Ignore("Repository can not be persisted.");
			}
			else
			{
				RepopulateRepositoryFromPersistedData();
				Assert.IsEmpty(DataMapperUnderTest.GetAllItems());
			}
		}

		[Test]
		public void CountAllItems_ReturnsZero()
		{
			SetState();
			Assert.AreEqual(0, DataMapperUnderTest.CountAllItems());
		}

		[Test]
		public void GetAllItems_ReturnsEmptyArray()
		{
			SetState();
			Assert.IsEmpty(DataMapperUnderTest.GetAllItems());
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void GetId_DeletedItemWithId_Throws()
		{
			SetState();
			DataMapperUnderTest.GetId(item);
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void GetItem_DeletedItem_Throws()
		{
			SetState();
			DataMapperUnderTest.GetItem(id);
		}

		[Test]
		public void GetItemMatchingQuery_Query_ReturnsEmpty()
		{
			SetState();
			if (DataMapperUnderTest.CanQuery)
			{
				Assert.AreEqual(0, DataMapperUnderTest.GetItemsMatching(query).Count);
			}
			else
			{
				Assert.Ignore("Repository does not support queries.");
			}
		}

		[Test]
		public void LastModified_IsChangedToLaterTime()
		{
			CreateInitialItem();
			DateTime modifiedTimePreTestedStateSwitch = DataMapperUnderTest.LastModified;
			DeleteAllItems();
			Assert.Greater(DataMapperUnderTest.LastModified, modifiedTimePreTestedStateSwitch);
		}

		[Test]
		public void LastModified_IsSetInUTC()
		{
			SetState();
			Assert.AreEqual(DateTimeKind.Utc, DataMapperUnderTest.LastModified.Kind);
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void Save_ItemDoesNotExist_Throws()
		{
			SetState();
			DataMapperUnderTest.SaveItem(item);
		}

		[Test]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void SaveItems_ItemDoesNotExist_Throws()
		{
			T itemNotInRepository = new T();
			List<T> itemsToSave = new List<T>();
			itemsToSave.Add(itemNotInRepository);
			DataMapperUnderTest.SaveItems(itemsToSave);
		}
	}
}