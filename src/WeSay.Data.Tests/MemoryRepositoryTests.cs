using NUnit.Framework;

namespace WeSay.Data.Tests
{
	[TestFixture]
	public class MemoryRepositoryStateUnitializedTests: IRepositoryStateUnitializedTests<TestItem>
	{
		[SetUp]
		public override void SetUp()
		{
			RepositoryUnderTest = new MemoryRepository<TestItem>();
		}

		[TearDown]
		public override void TearDown()
		{
			RepositoryUnderTest.Dispose();
		}
	}

	[TestFixture]
	public class MemoryRepositoryCreateItemTransitionTests:
			IRepositoryCreateItemTransitionTests<TestItem>
	{
		[SetUp]
		public override void SetUp()
		{
			RepositoryUnderTest = new MemoryRepository<TestItem>();
		}

		[TearDown]
		public override void TearDown()
		{
			RepositoryUnderTest.Dispose();
		}

		[Test]
		protected override void  GetItemsMatchingQuery_QueryWithShow_ReturnAllItemsMatchingQuery_v()
		{
			Item.StoredInt = 123;
			Item.StoredString = "I was stored!";
			QueryAdapter<TestItem> query = new QueryAdapter<TestItem>();
			query.Show("StoredInt").Show("StoredString");
			ResultSet<TestItem> resultsOfQuery = RepositoryUnderTest.GetItemsMatching(query);
			Assert.AreEqual(1, resultsOfQuery.Count);
			Assert.AreEqual(123, resultsOfQuery[0]["StoredInt"]);
			Assert.AreEqual("I was stored!", resultsOfQuery[0]["StoredString"]);
		}

		protected override void CreateNewRepositoryFromPersistedData()
		{
			//Do nothing.
		}
	}

	[TestFixture]
	public class MemoryRepositoryDeleteItemTransitionTests:
			IRepositoryDeleteItemTransitionTests<TestItem>
	{
		[SetUp]
		public override void SetUp()
		{
			RepositoryUnderTest = new MemoryRepository<TestItem>();
		}

		[TearDown]
		public override void TearDown()
		{
			RepositoryUnderTest.Dispose();
		}

		protected override void CreateNewRepositoryFromPersistedData()
		{
			//Do nothing.
		}
	}

	[TestFixture]
	public class MemoryRepositoryDeleteIdTransitionTests:
			IRepositoryDeleteIdTransitionTests<TestItem>
	{
		[SetUp]
		public override void SetUp()
		{
			RepositoryUnderTest = new MemoryRepository<TestItem>();
		}

		[TearDown]
		public override void TearDown()
		{
			RepositoryUnderTest.Dispose();
		}

		protected override void CreateNewRepositoryFromPersistedData()
		{
			//Do nothing.
		}
	}

	[TestFixture]
	public class MemoryRepositoryDeleteAllItemsTransitionTests:
			IRepositoryDeleteAllItemsTransitionTests<TestItem>
	{
		[SetUp]
		public override void SetUp()
		{
			RepositoryUnderTest = new MemoryRepository<TestItem>();
		}

		[TearDown]
		public override void TearDown()
		{
			RepositoryUnderTest.Dispose();
		}

		protected override void RepopulateRepositoryFromPersistedData()
		{
			//Do nothing.
		}
	}
}