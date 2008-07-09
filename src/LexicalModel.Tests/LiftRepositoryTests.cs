using System.IO;
using NUnit.Framework;
using WeSay.Data.Tests;
using WeSay.LexicalModel;

namespace WeSay.LexicalModel.Tests
{
	internal static class LiftFileInitializer
	{
		public static string MakeFile()
		{
			string liftfileName = Path.GetTempFileName();
			File.WriteAllText(liftfileName, @"<?xml version='1.0'?><lift version='0.12'></lift>");
			return liftfileName;
		}
	}

	[TestFixture]
	public class LiftRepositoryStateUnitializedTests:IRepositoryStateUnitializedTests<LexEntry>
	{
		private string _persistedFilePath;
		[SetUp]
		public void Setup()
		{
			this._persistedFilePath = LiftFileInitializer.MakeFile();
			this.RepositoryUnderTest = new LiftRepository(this._persistedFilePath);
		}

		[TearDown]
		public void Teardown()
		{
			RepositoryUnderTest.Dispose();
			File.Delete(this._persistedFilePath);
		}
	}

	[TestFixture]
	public class LiftRepositoryCreateItemTransitionTests : IRepositoryCreateItemTransitionTests<LexEntry>
	{
		private string _persistedFilePath;
		[SetUp]
		public void Setup()
		{
			this._persistedFilePath = LiftFileInitializer.MakeFile();
			this.RepositoryUnderTest = new LiftRepository(this._persistedFilePath);
		}

		[TearDown]
		public void Teardown()
		{
			RepositoryUnderTest.Dispose();
			File.Delete(this._persistedFilePath);
		}

		protected override void RepopulateRepositoryFromPersistedData()
		{
			RepositoryUnderTest.Dispose();
			RepositoryUnderTest = new LiftRepository(_persistedFilePath);
		}

		public override void SaveItem_ItemHasBeenPersisted()
		{

		}

		public override void SaveItems_ItemHasBeenPersisted()
		{
			Assert.Fail();
		}
	}

	[TestFixture]
	public class LiftRepositoryDeleteItemTransitionTests : IRepositoryDeleteItemTransitionTests<LexEntry>
	{
		private string _persistedFilePath;
		[SetUp]
		public void Setup()
		{
			this._persistedFilePath = LiftFileInitializer.MakeFile();
			this.RepositoryUnderTest = new LiftRepository(this._persistedFilePath);
		}

		[TearDown]
		public void Teardown()
		{
			RepositoryUnderTest.Dispose();
			File.Delete(this._persistedFilePath);
		}

		protected override void RepopulateRepositoryFromPersistedData()
		{
			RepositoryUnderTest.Dispose();
			RepositoryUnderTest = new LiftRepository(_persistedFilePath);
		}
	}

	[TestFixture]
	public class LiftRepositoryDeleteIdTransitionTests : IRepositoryDeleteIdTransitionTests<LexEntry>
	{
		private string _persistedFilePath;
		[SetUp]
		public void Setup()
		{
			this._persistedFilePath = LiftFileInitializer.MakeFile();
			this.RepositoryUnderTest = new LiftRepository(this._persistedFilePath);
		}

		[TearDown]
		public void Teardown()
		{
			RepositoryUnderTest.Dispose();
			File.Delete(this._persistedFilePath);
		}

		protected override void RepopulateRepositoryFromPersistedData()
		{
			RepositoryUnderTest.Dispose();
			RepositoryUnderTest = new LiftRepository(_persistedFilePath);
		}
	}
}