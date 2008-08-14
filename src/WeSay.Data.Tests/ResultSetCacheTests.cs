using System;
using System.Collections.Generic;
using WeSay.Data;
using NUnit.Framework;

namespace WeSay.Data.Tests
{
	[TestFixture]
	public class ResultSetCacheTestsWithSingleRecordTokensFromQuery
	{
		private MemoryRepository<TestItem> _repository;
		private SortDefinition[] _sortDefinitions;
		private ResultSet<TestItem> _results;
		private Query _queryToCache;

		[SetUp]
		public void Setup()
		{
			_repository = new MemoryRepository<TestItem>();
			PopulateRepositoryWithItemsForQuerying(_repository);
			_queryToCache = new Query(typeof(TestItem));
			_queryToCache = _queryToCache.Show("StoredString");
			_results = _repository.GetItemsMatching(_queryToCache);
			_sortDefinitions = new SortDefinition[1];
			_sortDefinitions[0] = new SortDefinition("StoredString", Comparer<string>.Default);
		}

		[TearDown]
		public void Teardown()
		{
			_repository.Dispose();
		}

		private void PopulateRepositoryWithItemsForQuerying(MemoryRepository<TestItem> repository)
		{
			TestItem[] items = new TestItem[3];
			items[0] = repository.CreateItem();
			items[0].StoredString = "Item 3";
			repository.SaveItem(items[0]);
			items[1] = repository.CreateItem();
			items[1].StoredString = "Item 0";
			repository.SaveItem(items[1]);
			items[2] = repository.CreateItem();
			items[2].StoredString = "Item 2";
			repository.SaveItem(items[2]);
		}

		[Test]
		public void Constructor_IsPassedUnsortedResultSet_GetResultSetReturnsSorted()
		{
			ResultSetCache<TestItem> resultSetCacheUnderTest = new ResultSetCache<TestItem>(_repository, _results, _queryToCache, _sortDefinitions);
			Assert.AreEqual(3, resultSetCacheUnderTest.GetResultSet().Count);
			Assert.AreEqual("Item 0", resultSetCacheUnderTest.GetResultSet()[0]["StoredString"]);
			Assert.AreEqual("Item 2", resultSetCacheUnderTest.GetResultSet()[1]["StoredString"]);
			Assert.AreEqual("Item 3", resultSetCacheUnderTest.GetResultSet()[2]["StoredString"]);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void UpdateItemInCache_Null_Throws()
		{
			ResultSetCache<TestItem> resultSetCacheUnderTest = new ResultSetCache<TestItem>(_repository, _results, _queryToCache, _sortDefinitions);
			resultSetCacheUnderTest.UpdateItemInCache(null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void UpdateItemInCache_ItemDoesNotExistInRepository_Throws()
		{
			ResultSetCache<TestItem> resultSetCacheUnderTest = new ResultSetCache<TestItem>(_repository, _results, _queryToCache, _sortDefinitions);
			TestItem itemNotInRepository = new TestItem();
			resultSetCacheUnderTest.UpdateItemInCache(itemNotInRepository);
		}

		[Test]
		public void UpdateItemInCache_ItemDoesNotExistInCacheButDoesInRepository_ItemIsAddedToResultSetAndSortedCorrectly()
		{
			ResultSetCache<TestItem> resultSetCacheUnderTest = new ResultSetCache<TestItem>(_repository, _results, _queryToCache, _sortDefinitions);

			TestItem itemCreatedAfterCache = _repository.CreateItem();
			itemCreatedAfterCache.StoredString = "Item 1";
			_repository.SaveItem(itemCreatedAfterCache);
			resultSetCacheUnderTest.UpdateItemInCache(itemCreatedAfterCache);

			Assert.AreEqual(4, resultSetCacheUnderTest.GetResultSet().Count);
			Assert.AreEqual("Item 0", resultSetCacheUnderTest.GetResultSet()[0]["StoredString"]);
			Assert.AreEqual("Item 1", resultSetCacheUnderTest.GetResultSet()[1]["StoredString"]);
			Assert.AreEqual("Item 2", resultSetCacheUnderTest.GetResultSet()[2]["StoredString"]);
			Assert.AreEqual("Item 3", resultSetCacheUnderTest.GetResultSet()[3]["StoredString"]);
		}

		[Test]
		public void UpdateItemInCache_ItemExists_ResultSetIsUpdatedAndSorted()
		{
			TestItem itemToModify = _repository.CreateItem();
			itemToModify.StoredString = "Item 5";
			_repository.SaveItem(itemToModify);

			_results = _repository.GetItemsMatching(_queryToCache);
			ResultSetCache<TestItem> resultSetCacheUnderTest = new ResultSetCache<TestItem>(_repository, _results, _queryToCache, _sortDefinitions);

			itemToModify.StoredString = "Item 1";
			_repository.SaveItem(itemToModify);
			resultSetCacheUnderTest.UpdateItemInCache(itemToModify);

			Assert.AreEqual(4, resultSetCacheUnderTest.GetResultSet().Count);
			Assert.AreEqual("Item 0", resultSetCacheUnderTest.GetResultSet()[0]["StoredString"]);
			Assert.AreEqual("Item 1", resultSetCacheUnderTest.GetResultSet()[1]["StoredString"]);
			Assert.AreEqual("Item 2", resultSetCacheUnderTest.GetResultSet()[2]["StoredString"]);
			Assert.AreEqual("Item 3", resultSetCacheUnderTest.GetResultSet()[3]["StoredString"]);
		}

		[Test]
		public void UpdateItemInCache_ItemHasNotChanged_ResultSetIsNotChanged()
		{
			TestItem unmodifiedItem = _repository.CreateItem();
			unmodifiedItem.StoredString = "Item 1";
			_repository.SaveItem(unmodifiedItem);

			_results = _repository.GetItemsMatching(_queryToCache);
			ResultSetCache<TestItem> resultSetCacheUnderTest = new ResultSetCache<TestItem>(_repository, _results, _queryToCache, _sortDefinitions);
			resultSetCacheUnderTest.UpdateItemInCache(unmodifiedItem);

			//Would be a better test but ResultSets don't support equality checks
			//Assert.AreEqual(resultsBeforeUpdate, resultsAfterUpdate);

			Assert.AreEqual(4, resultSetCacheUnderTest.GetResultSet().Count);
			Assert.AreEqual("Item 0", resultSetCacheUnderTest.GetResultSet()[0]["StoredString"]);
			Assert.AreEqual("Item 1", resultSetCacheUnderTest.GetResultSet()[1]["StoredString"]);
			Assert.AreEqual("Item 2", resultSetCacheUnderTest.GetResultSet()[2]["StoredString"]);
			Assert.AreEqual("Item 3", resultSetCacheUnderTest.GetResultSet()[3]["StoredString"]);
		}

		[Test]
		public void DeleteItemFromCache_Item_ItemIsNoLongerInResultSet()
		{
			TestItem itemToDelete = _repository.CreateItem();
			itemToDelete.StoredString = "Item 1";
			_repository.SaveItem(itemToDelete);

			_results = _repository.GetItemsMatching(_queryToCache);
			ResultSetCache<TestItem> resultSetCacheUnderTest = new ResultSetCache<TestItem>(_repository, _results, _queryToCache, _sortDefinitions);
			resultSetCacheUnderTest.DeleteItemFromCache(itemToDelete);

			//Would be a better test but ResultSets don't support equality checks
			//Assert.AreEqual(resultsBeforeUpdate, resultsAfterUpdate);

			Assert.AreEqual(3, resultSetCacheUnderTest.GetResultSet().Count);
			Assert.AreEqual("Item 0", resultSetCacheUnderTest.GetResultSet()[0]["StoredString"]);
			Assert.AreEqual("Item 2", resultSetCacheUnderTest.GetResultSet()[1]["StoredString"]);
			Assert.AreEqual("Item 3", resultSetCacheUnderTest.GetResultSet()[2]["StoredString"]);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void DeleteItemFromCache_Null_Throws()
		{
			_results = _repository.GetItemsMatching(_queryToCache);
			ResultSetCache<TestItem> resultSetCacheUnderTest = new ResultSetCache<TestItem>(_repository, _results, _queryToCache, _sortDefinitions);
			resultSetCacheUnderTest.DeleteItemFromCache(null);
		}
	}

	[TestFixture]
	public class ResultSetCacheTestsWithMultipleRecordTokensFromQuery
	{
		private MemoryRepository<TestItem> _repository;
		private SortDefinition[] _sortDefinitions;
		private ResultSet<TestItem> _results;
		private Query _queryToCache = null;

		[SetUp]
		public void Setup()
		{
			_repository = new MemoryRepository<TestItem>();
			PopulateRepositoryWithItemsForQuerying(_repository);
			_queryToCache = new Query(typeof(TestItem));
			_queryToCache = _queryToCache.ShowEach("StoredList");
			_results = _repository.GetItemsMatching(_queryToCache);
			_sortDefinitions = new SortDefinition[1];
			_sortDefinitions[0] = new SortDefinition("StoredList", Comparer<string>.Default);
		}

		[TearDown]
		public void Teardown()
		{
			_repository.Dispose();
		}

		private void PopulateRepositoryWithItemsForQuerying(MemoryRepository<TestItem> repository)
		{
			TestItem[] items = new TestItem[2];
			items[0] = repository.CreateItem();
			items[0].StoredList = PopulateListWith("Item 1", "Item 3");
			repository.SaveItem(items[0]);

			items[1] = repository.CreateItem();
			items[1].StoredList = PopulateListWith("Item 2", "Item 0");
			repository.SaveItem(items[1]);
		}

		public List<string> PopulateListWith(string string1, string string2)
		{
			List<string> listTopopulate = new List<string>();
			listTopopulate.Add(string1);
			listTopopulate.Add(string2);
			return listTopopulate;
		}

		[Test]
		public void Constructor_IsPassedUnsortedResultSet_GetResultSetReturnsSorted()
		{
			ResultSetCache<TestItem> resultSetCacheUnderTest = new ResultSetCache<TestItem>(_repository, _results, _queryToCache, _sortDefinitions);
			Assert.AreEqual(4, resultSetCacheUnderTest.GetResultSet().Count);
			Assert.AreEqual("Item 0", resultSetCacheUnderTest.GetResultSet()[0]["StoredList"]);
			Assert.AreEqual("Item 1", resultSetCacheUnderTest.GetResultSet()[1]["StoredList"]);
			Assert.AreEqual("Item 2", resultSetCacheUnderTest.GetResultSet()[2]["StoredList"]);
			Assert.AreEqual("Item 3", resultSetCacheUnderTest.GetResultSet()[3]["StoredList"]);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void UpdateItemInCache_Null_Throws()
		{
			ResultSetCache<TestItem> resultSetCacheUnderTest = new ResultSetCache<TestItem>(_repository, _results, _queryToCache, _sortDefinitions);
			resultSetCacheUnderTest.UpdateItemInCache(null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void UpdateItemInCache_ItemDoesNotExistInRepository_Throws()
		{
			ResultSetCache<TestItem> resultSetCacheUnderTest = new ResultSetCache<TestItem>(_repository, _results, _queryToCache, _sortDefinitions);
			TestItem itemNotInRepository = new TestItem();
			resultSetCacheUnderTest.UpdateItemInCache(itemNotInRepository);
		}

		[Test]
		public void UpdateItemInCache_ItemDoesNotExistInCacheButDoesInRepository_ItemIsAddedToResultSetAndSortedCorrectly()
		{
			ResultSetCache<TestItem> resultSetCacheUnderTest = new ResultSetCache<TestItem>(_repository, _results, _queryToCache, _sortDefinitions);

			TestItem itemCreatedAfterCache = _repository.CreateItem();
			itemCreatedAfterCache.StoredList = PopulateListWith("Item 5", "Item 4");
			_repository.SaveItem(itemCreatedAfterCache);
			resultSetCacheUnderTest.UpdateItemInCache(itemCreatedAfterCache);

			Assert.AreEqual(6, resultSetCacheUnderTest.GetResultSet().Count);
			Assert.AreEqual("Item 0", resultSetCacheUnderTest.GetResultSet()[0]["StoredList"]);
			Assert.AreEqual("Item 1", resultSetCacheUnderTest.GetResultSet()[1]["StoredList"]);
			Assert.AreEqual("Item 2", resultSetCacheUnderTest.GetResultSet()[2]["StoredList"]);
			Assert.AreEqual("Item 3", resultSetCacheUnderTest.GetResultSet()[3]["StoredList"]);
			Assert.AreEqual("Item 4", resultSetCacheUnderTest.GetResultSet()[4]["StoredList"]);
			Assert.AreEqual("Item 5", resultSetCacheUnderTest.GetResultSet()[5]["StoredList"]);
		}

		[Test]
		public void UpdateItemInCache_ItemExists_ResultSetIsUpdatedAndSorted()
		{
			TestItem itemToModify = _repository.CreateItem();
			itemToModify.StoredList = PopulateListWith("Change Me!", "Me 2!");
			_repository.SaveItem(itemToModify);

			_results = _repository.GetItemsMatching(_queryToCache);
			ResultSetCache<TestItem> resultSetCacheUnderTest = new ResultSetCache<TestItem>(_repository, _results, _queryToCache, _sortDefinitions);

			itemToModify.StoredList = PopulateListWith("Item 5", "Item 4");
			_repository.SaveItem(itemToModify);
			resultSetCacheUnderTest.UpdateItemInCache(itemToModify);

			Assert.AreEqual(6, resultSetCacheUnderTest.GetResultSet().Count);
			Assert.AreEqual("Item 0", resultSetCacheUnderTest.GetResultSet()[0]["StoredList"]);
			Assert.AreEqual("Item 1", resultSetCacheUnderTest.GetResultSet()[1]["StoredList"]);
			Assert.AreEqual("Item 2", resultSetCacheUnderTest.GetResultSet()[2]["StoredList"]);
			Assert.AreEqual("Item 3", resultSetCacheUnderTest.GetResultSet()[3]["StoredList"]);
			Assert.AreEqual("Item 4", resultSetCacheUnderTest.GetResultSet()[4]["StoredList"]);
			Assert.AreEqual("Item 5", resultSetCacheUnderTest.GetResultSet()[5]["StoredList"]);
		}

		[Test]
		public void UpdateItemInCache_ItemHasNotChanged_ResultSetIsNotChanged()
		{
			TestItem unmodifiedItem = _repository.CreateItem();
			unmodifiedItem.StoredList = PopulateListWith("Item 5", "Item 4");
			_repository.SaveItem(unmodifiedItem);

			_results = _repository.GetItemsMatching(_queryToCache);
			ResultSetCache<TestItem> resultSetCacheUnderTest = new ResultSetCache<TestItem>(_repository, _results, _queryToCache, _sortDefinitions);

			resultSetCacheUnderTest.UpdateItemInCache(unmodifiedItem);

			//Would be a better test but ResultSets don't support equality checks
			//Assert.AreEqual(resultsBeforeUpdate, resultsAfterUpdate);

			Assert.AreEqual(6, resultSetCacheUnderTest.GetResultSet().Count);
			Assert.AreEqual("Item 0", resultSetCacheUnderTest.GetResultSet()[0]["StoredList"]);
			Assert.AreEqual("Item 1", resultSetCacheUnderTest.GetResultSet()[1]["StoredList"]);
			Assert.AreEqual("Item 2", resultSetCacheUnderTest.GetResultSet()[2]["StoredList"]);
			Assert.AreEqual("Item 3", resultSetCacheUnderTest.GetResultSet()[3]["StoredList"]);
			Assert.AreEqual("Item 4", resultSetCacheUnderTest.GetResultSet()[4]["StoredList"]);
			Assert.AreEqual("Item 5", resultSetCacheUnderTest.GetResultSet()[5]["StoredList"]);
		}

	}

}
