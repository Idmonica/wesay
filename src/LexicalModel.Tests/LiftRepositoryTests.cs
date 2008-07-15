using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using WeSay.Data.Tests;
using WeSay.Data;
using WeSay.LexicalModel;

namespace WeSay.LexicalModel.Tests
{
	internal static class LiftFileInitializer
	{
		public static string MakeFile()
		{
			string liftfileName = Path.GetTempFileName();
			File.WriteAllText(liftfileName,
				@"<?xml version='1.0' encoding='utf-8'?>
				<lift
					version='0.12'
					producer='WeSay 1.0.0.0'>
					<entry
						id='Sonne_c753f6cc-e07c-4bb1-9e3c-013d09629111'
						dateCreated='2008-07-01T06:29:23Z'
						dateModified='2008-07-01T06:29:57Z'
						guid='c753f6cc-e07c-4bb1-9e3c-013d09629111'>
						<lexical-unit>
							<form
								lang='v'>
								<text>Sonne</text>
							</form>
						</lexical-unit>
						<sense
							id='33d60091-ba96-4204-85fe-9d15a24bd5ff'>
							<trait
								name='SemanticDomainDdp4'
								value='1 Universe, creation' />
						</sense>
					</entry>
				</lift>");
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
			this._persistedFilePath = Path.GetRandomFileName();
			_persistedFilePath = Path.GetFullPath(_persistedFilePath);
			this.RepositoryUnderTest = new LiftRepository(this._persistedFilePath);
		}

		[TearDown]
		public void Teardown()
		{
			RepositoryUnderTest.Dispose();
			File.Delete(this._persistedFilePath);
		}

		[Test]
		public void Constructor_FileDoesNotExist_EmptyLiftFileIsCreated()
		{
			string nonExistentFileToBeCreated = Path.GetTempPath() + Path.GetRandomFileName();
			LiftRepository testRepoitory = new LiftRepository(nonExistentFileToBeCreated);
			string fileContent = File.ReadAllText(nonExistentFileToBeCreated);
			const string emptyLiftFileContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<lift version=""0.12"" producer=""WeSay 1.0.0.0"" />";
			Assert.AreEqual(emptyLiftFileContent, fileContent);
		}

		[Test]
		public void Constructor_FileIsEmpty_MakeFileAnEmptyLiftFile()
		{
			string emptyFileToBeFilled = Path.GetTempFileName();
			LiftRepository testRepoitory = new LiftRepository(emptyFileToBeFilled);
			string fileContent = File.ReadAllText(emptyFileToBeFilled);
			const string emptyLiftFileContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<lift version=""0.12"" producer=""WeSay 1.0.0.0"" />";
			Assert.AreEqual(emptyLiftFileContent, fileContent);
		}
	}

	[TestFixture]
	public class LiftRepositoryCreatedFromPersistedData : IRepositoryPopulateFromPersistedTests<LexEntry>
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

		[Test]
		public override void LastModified_IsSetToMostRecentLexentryInPersistedDatasLastModifiedTime()
		{
			SetState();
			Assert.AreEqual(Item.ModificationTime, RepositoryUnderTest.LastModified);
		}

		protected override void CreateNewRepositoryFromPersistedData()
		{
			RepositoryUnderTest.Dispose();
			RepositoryUnderTest = new LiftRepository(_persistedFilePath);
		}
	}

	[TestFixture]
	public class LiftRepositoryCreateItemTransitionTests : IRepositoryCreateItemTransitionTests<LexEntry>
	{
		private string _persistedFilePath;
		[SetUp]
		public void Setup()
		{
			this._persistedFilePath = Path.GetRandomFileName();
			_persistedFilePath = Path.GetFullPath(_persistedFilePath);
			this.RepositoryUnderTest = new LiftRepository(this._persistedFilePath);
		}

		[TearDown]
		public void Teardown()
		{
			RepositoryUnderTest.Dispose();
			File.Delete(this._persistedFilePath);
		}

		protected override void CreateNewRepositoryFromPersistedData()
		{
			RepositoryUnderTest.Dispose();
			RepositoryUnderTest = new LiftRepository(_persistedFilePath);
		}
	}

	[TestFixture]
	public class LiftRepositoryDeleteItemTransitionTests : IRepositoryDeleteItemTransitionTests<LexEntry>
	{
		private string _persistedFilePath;
		[SetUp]
		public void Setup()
		{
			this._persistedFilePath = Path.GetRandomFileName();
			_persistedFilePath = Path.GetFullPath(_persistedFilePath);
			this.RepositoryUnderTest = new LiftRepository(this._persistedFilePath);
		}

		[TearDown]
		public void Teardown()
		{
			RepositoryUnderTest.Dispose();
			File.Delete(this._persistedFilePath);
		}

		protected override void CreateNewRepositoryFromPersistedData()
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
			this._persistedFilePath = Path.GetRandomFileName();
			_persistedFilePath = Path.GetFullPath(_persistedFilePath);
			this.RepositoryUnderTest = new LiftRepository(this._persistedFilePath);
		}

		[TearDown]
		public void Teardown()
		{
			RepositoryUnderTest.Dispose();
			File.Delete(this._persistedFilePath);
		}

		protected override void CreateNewRepositoryFromPersistedData()
		{
			RepositoryUnderTest.Dispose();
			RepositoryUnderTest = new LiftRepository(_persistedFilePath);
		}
	}
}