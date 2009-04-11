using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using WeSay.Data;
using WeSay.Foundation;
using Palaso.TestUtilities;

namespace WeSay.LexicalModel.Tests
{
	[TestFixture]
	public class LexEntryRepositoryCachingTests
	{
		private TemporaryFolder _tempfolder;
		private LexEntryRepository _repository;
		private ResultSet<LexEntry> firstResults;
		private bool _afterEntryDeletedCalled;
		private bool _afterEntrySavedCalled;
		private bool _afterEntryModifiedCalled;

		[SetUp]
		public void Setup()
		{
			_tempfolder = new TemporaryFolder();
			string persistedFilePath = _tempfolder.GetTemporaryFile();
			_repository = new LexEntryRepository(persistedFilePath);

			_afterEntryDeletedCalled = _afterEntrySavedCalled = _afterEntryModifiedCalled = false;
		}

		[TearDown]
		public void Teardown()
		{
			_repository.Dispose();
		}

		[Test]
		public void GetAllEntriesSortedByHeadWord_CreateItemAfterFirstCall_EntryIsReturnedAndSortedInResultSet()
		{
			LexEntry entryBeforeFirstQuery = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 1");

			_repository.GetAllEntriesSortedByHeadword(new WritingSystem("de", SystemFonts.DefaultFont));

			LexEntry entryAfterFirstQuery = _repository.CreateItem();

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByHeadword(new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(2, results.Count);
			Assert.AreEqual(null, results[0]["Form"]);
			Assert.AreEqual("word 1", results[1]["Form"]);
		}

		private LexEntry CreateEntryWithLexicalFormBeforeFirstQuery(string writingSystem, string lexicalForm)
		{
			LexEntry entryBeforeFirstQuery = _repository.CreateItem();
			entryBeforeFirstQuery.LexicalForm.SetAlternative(writingSystem, lexicalForm);
			_repository.SaveItem(entryBeforeFirstQuery);
			return entryBeforeFirstQuery;
		}

		[Test]
		public void GetAllEntriesSortedByHeadWord_ModifyAndSaveAfterFirstCall_EntryIsModifiedAndSortedInResultSet()
		{
			LexEntry entryBeforeFirstQuery = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByHeadword(new WritingSystem("de", SystemFonts.DefaultFont));

			entryBeforeFirstQuery.LexicalForm.SetAlternative("de", "word 1");
			_repository.SaveItem(entryBeforeFirstQuery);

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByHeadword(new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(1, results.Count);
			Assert.AreEqual("word 1", results[0]["Form"]);
		}

		[Test]
		public void GetAllEntriesSortedByHeadWord_ModifyAndSaveMultipleAfterFirstCall_EntriesModifiedAndSortedInResultSet()
		{
			List<LexEntry> entriesToModify = new List<LexEntry>();
			entriesToModify.Add(CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0"));
			entriesToModify.Add(CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 1"));

			_repository.GetAllEntriesSortedByHeadword(new WritingSystem("de", SystemFonts.DefaultFont));

			entriesToModify[0].LexicalForm["de"] = "word 3";
			entriesToModify[1].LexicalForm["de"] = "word 2";
			_repository.SaveItems(entriesToModify);

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByHeadword(new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(2, results.Count);
			Assert.AreEqual("word 2", results[0]["Form"]);
			Assert.AreEqual("word 3", results[1]["Form"]);

		}

		[Test]
		public void GetAllEntriesSortedByHeadWord_DeleteAfterFirstCall_EntryIsDeletedInResultSet()
		{
			LexEntry entrytoBeDeleted = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByHeadword(new WritingSystem("de", SystemFonts.DefaultFont));

			_repository.DeleteItem(entrytoBeDeleted);

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByHeadword(new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(0, results.Count);
		}

		[Test]
		public void GetAllEntriesSortedByHeadWord_DeleteByIdAfterFirstCall_EntryIsDeletedInResultSet()
		{
			LexEntry entrytoBeDeleted = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByHeadword(new WritingSystem("de", SystemFonts.DefaultFont));

			_repository.DeleteItem(_repository.GetId(entrytoBeDeleted));

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByHeadword(new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(0, results.Count);
		}

		[Test]
		public void GetAllEntriesSortedByHeadWord_DeleteAllItemsAfterFirstCall_EntryIsDeletedInResultSet()
		{
			LexEntry entrytoBeDeleted = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByHeadword(new WritingSystem("de", SystemFonts.DefaultFont));

			_repository.DeleteAllItems();

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByHeadword(new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(0, results.Count);
		}

		[Test]
		public void GetAllEntriesSortedByLexicalForm_CreateItemAfterFirstCall_EntryIsReturnedAndSortedInResultSet()
		{
			LexEntry entryBeforeFirstQuery = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 1");

			_repository.GetAllEntriesSortedByLexicalFormOrAlternative(new WritingSystem("de", SystemFonts.DefaultFont));

			LexEntry entryAfterFirstQuery = _repository.CreateItem();

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByLexicalFormOrAlternative(new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(2, results.Count);
			Assert.AreEqual(null, results[0]["Form"]);
			Assert.AreEqual("word 1", results[1]["Form"]);
		}

		[Test]
		public void GetAllEntriesSortedByLexicalForm_ModifyAndSaveAfterFirstCall_EntryIsModifiedAndSortedInResultSet()
		{
			LexEntry entryBeforeFirstQuery = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByLexicalFormOrAlternative(new WritingSystem("de", SystemFonts.DefaultFont));

			entryBeforeFirstQuery.LexicalForm.SetAlternative("de", "word 1");
			_repository.SaveItem(entryBeforeFirstQuery);

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByLexicalFormOrAlternative(new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(1, results.Count);
			Assert.AreEqual("word 1", results[0]["Form"]);
		}

		[Test]
		public void GetAllEntriesSortedByLexicalForm_ModifyAndSaveMultipleAfterFirstCall_EntriesModifiedAndSortedInResultSet()
		{
			List<LexEntry> entriesToModify = new List<LexEntry>();
			entriesToModify.Add(CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0"));
			entriesToModify.Add(CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 1"));

			_repository.GetAllEntriesSortedByLexicalFormOrAlternative(new WritingSystem("de", SystemFonts.DefaultFont));

			entriesToModify[0].LexicalForm["de"] = "word 3";
			entriesToModify[1].LexicalForm["de"] = "word 2";
			_repository.SaveItems(entriesToModify);

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByLexicalFormOrAlternative(new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(2, results.Count);
			Assert.AreEqual("word 2", results[0]["Form"]);
			Assert.AreEqual("word 3", results[1]["Form"]);

		}

		[Test]
		public void GetAllEntriesSortedByLexicalForm_DeleteAfterFirstCall_EntryIsDeletedInResultSet()
		{
			LexEntry entrytoBeDeleted = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByLexicalFormOrAlternative(new WritingSystem("de", SystemFonts.DefaultFont));

			_repository.DeleteItem(entrytoBeDeleted);

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByLexicalFormOrAlternative(new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(0, results.Count);
		}

		[Test]
		public void GetAllEntriesSortedByLexicalForm_DeleteByIdAfterFirstCall_EntryIsDeletedInResultSet()
		{
			LexEntry entrytoBeDeleted = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByLexicalFormOrAlternative(new WritingSystem("de", SystemFonts.DefaultFont));

			_repository.DeleteItem(_repository.GetId(entrytoBeDeleted));

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByLexicalFormOrAlternative(new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(0, results.Count);
		}

		[Test]
		public void GetAllEntriesSortedByLexicalForm_DeleteAllItemsAfterFirstCall_EntryIsDeletedInResultSet()
		{
			LexEntry entrytoBeDeleted = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByLexicalFormOrAlternative(new WritingSystem("de", SystemFonts.DefaultFont));

			_repository.DeleteAllItems();

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByLexicalFormOrAlternative(new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(0, results.Count);
		}



		[Test]
		public void NotifyThatLexEntryHasBeenUpdated_LexEntry_CachesAreUpdated()
		{
			LexEntry entryToUpdate = _repository.CreateItem();
			entryToUpdate.LexicalForm.SetAlternative("de", "word 0");
			_repository.SaveItem(entryToUpdate);
			CreateCaches();
			entryToUpdate.LexicalForm.SetAlternative("de", "word 1");
			_repository.NotifyThatLexEntryHasBeenUpdated(entryToUpdate);
			WritingSystem writingSystemToMatch = new WritingSystem("de", SystemFonts.DefaultFont);
			ResultSet<LexEntry> headWordResults = _repository.GetAllEntriesSortedByHeadword(writingSystemToMatch);
			ResultSet<LexEntry> lexicalFormResults = _repository.GetAllEntriesSortedByLexicalFormOrAlternative(writingSystemToMatch);
			Assert.AreEqual("word 1", headWordResults[0]["Form"]);
			Assert.AreEqual("word 1", lexicalFormResults[0]["Form"]);
		}

		private void CreateCaches()
		{
			WritingSystem writingSystemToMatch = new WritingSystem("de", SystemFonts.DefaultFont);
			_repository.GetAllEntriesSortedByHeadword(writingSystemToMatch);
			_repository.GetAllEntriesSortedByLexicalFormOrAlternative(writingSystemToMatch);
			//_repository.GetAllEntriesSortedByDefinitionOrGloss(writingSystemToMatch);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void NotifyThatLexEntryHasBeenUpdated_Null_Throws()
		{
			_repository.NotifyThatLexEntryHasBeenUpdated(null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void NotifyThatLexEntryHasBeenUpdated_LexEntryDoesNotExistInRepository_Throws()
		{
			LexEntry entryToUpdate = new LexEntry();
			_repository.NotifyThatLexEntryHasBeenUpdated(entryToUpdate);
		}

		[Test]
		public void GetEntriesWithMissingFieldSortedByLexicalUnit_CreateItemAfterFirstCall_EntryIsReturnedAndSortedInResultSet()
		{
			Field fieldToFill = new Field(LexEntry.WellKnownProperties.Citation, "LexEntry", new string[] { "de" });
			LexEntry entryBeforeFirstQuery = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 1");

			_repository.GetEntriesWithMissingFieldSortedByLexicalUnit(fieldToFill, null, new WritingSystem("de", SystemFonts.DefaultFont));

			LexEntry entryAfterFirstQuery = _repository.CreateItem();

			ResultSet<LexEntry> results = _repository.GetEntriesWithMissingFieldSortedByLexicalUnit(fieldToFill, null, new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(2, results.Count);
			Assert.AreEqual(null, results[0]["Form"]);
			Assert.AreEqual("word 1", results[1]["Form"]);
		}

		[Test]
		public void GetEntriesWithMissingFieldSortedByLexicalUnit_ModifyAndSaveAfterFirstCall_EntryIsModifiedAndSortedInResultSet()
		{
			Field fieldToFill = new Field(LexEntry.WellKnownProperties.Citation, "LexEntry", new string[] { "de" });

			LexEntry entryBeforeFirstQuery = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0");

			_repository.GetEntriesWithMissingFieldSortedByLexicalUnit(fieldToFill, null, new WritingSystem("de", SystemFonts.DefaultFont));

			entryBeforeFirstQuery.LexicalForm.SetAlternative("de", "word 1");
			_repository.SaveItem(entryBeforeFirstQuery);

			ResultSet<LexEntry> results = _repository.GetEntriesWithMissingFieldSortedByLexicalUnit(fieldToFill, null, new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(1, results.Count);
			Assert.AreEqual("word 1", results[0]["Form"]);
		}

		[Test]
		public void GetEntriesWithMissingFieldSortedByLexicalUnit_ModifyAndSaveMultipleAfterFirstCall_EntriesModifiedAndSortedInResultSet()
		{
			Field fieldToFill = new Field(LexEntry.WellKnownProperties.Citation, "LexEntry", new string[] { "de" });

			List<LexEntry> entriesToModify = new List<LexEntry>();
			entriesToModify.Add(CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0"));
			entriesToModify.Add(CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 1"));

			_repository.GetEntriesWithMissingFieldSortedByLexicalUnit(fieldToFill, null, new WritingSystem("de", SystemFonts.DefaultFont));

			entriesToModify[0].LexicalForm["de"] = "word 3";
			entriesToModify[1].LexicalForm["de"] = "word 2";
			_repository.SaveItems(entriesToModify);

			ResultSet<LexEntry> results = _repository.GetEntriesWithMissingFieldSortedByLexicalUnit(fieldToFill, null, new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(2, results.Count);
			Assert.AreEqual("word 2", results[0]["Form"]);
			Assert.AreEqual("word 3", results[1]["Form"]);
		}

		[Test]
		public void GetEntriesWithMissingFieldSortedByLexicalUnit_DeleteAfterFirstCall_EntryIsDeletedInResultSet()
		{
			Field fieldToFill = new Field(LexEntry.WellKnownProperties.Citation, "LexEntry", new string[] { "de" });

			LexEntry entrytoBeDeleted = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0");

			_repository.GetEntriesWithMissingFieldSortedByLexicalUnit(fieldToFill, null, new WritingSystem("de", SystemFonts.DefaultFont));

			_repository.DeleteItem(entrytoBeDeleted);

			ResultSet<LexEntry> results = _repository.GetEntriesWithMissingFieldSortedByLexicalUnit(fieldToFill, null, new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(0, results.Count);
		}

		[Test]
		public void GetEntriesWithMissingFieldSortedByLexicalUnit_DeleteByIdAfterFirstCall_EntryIsDeletedInResultSet()
		{
			Field fieldToFill = new Field(LexEntry.WellKnownProperties.Citation, "LexEntry", new string[] { "de" });

			LexEntry entrytoBeDeleted = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0");

			_repository.GetEntriesWithMissingFieldSortedByLexicalUnit(fieldToFill, null, new WritingSystem("de", SystemFonts.DefaultFont));

			_repository.DeleteItem(_repository.GetId(entrytoBeDeleted));

			ResultSet<LexEntry> results = _repository.GetEntriesWithMissingFieldSortedByLexicalUnit(fieldToFill, null, new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(0, results.Count);
		}

		[Test]
		public void GetEntriesWithMissingFieldSortedByLexicalUnit_DeleteAllItemsAfterFirstCall_EntryIsDeletedInResultSet()
		{
			Field fieldToFill = new Field(LexEntry.WellKnownProperties.Citation, "LexEntry", new string[] { "de" });

			LexEntry entrytoBeDeleted = CreateEntryWithLexicalFormBeforeFirstQuery("de", "word 0");

			_repository.GetEntriesWithMissingFieldSortedByLexicalUnit(fieldToFill, null, new WritingSystem("de", SystemFonts.DefaultFont));

			_repository.DeleteAllItems();

			ResultSet<LexEntry> results = _repository.GetEntriesWithMissingFieldSortedByLexicalUnit(fieldToFill, null, new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(0, results.Count);
		}

		private LexEntry CreateEntryWithDefinitionBeforeFirstQuery(string writingSystem, string lexicalForm)
		{
			LexEntry entryBeforeFirstQuery = _repository.CreateItem();
			entryBeforeFirstQuery.Senses.Add(new LexSense());
			entryBeforeFirstQuery.Senses[0].Definition.SetAlternative(writingSystem, lexicalForm);
			_repository.SaveItem(entryBeforeFirstQuery);
			return entryBeforeFirstQuery;
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_CreateItemAfterFirstCall_EntryIsReturnedAndSortedInResultSet()
		{
			LexEntry entryBeforeFirstQuery = CreateEntryWithDefinitionBeforeFirstQuery("de", "word 1");

			_repository.GetAllEntriesSortedByDefinitionOrGloss(new WritingSystem("de", SystemFonts.DefaultFont));

			LexEntry entryAfterFirstQuery = CreateEntryWithDefinitionBeforeFirstQuery("de", "word 2");

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByDefinitionOrGloss(new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(2, results.Count);
			Assert.AreEqual("word 1", results[0]["Form"]);
			Assert.AreEqual("word 2", results[1]["Form"]);
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_ModifyAndSaveAfterFirstCall_EntryIsModifiedAndSortedInResultSet()
		{
			LexEntry entryBeforeFirstQuery = CreateEntryWithDefinitionBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByDefinitionOrGloss(new WritingSystem("de", SystemFonts.DefaultFont));

			entryBeforeFirstQuery.Senses[0].Definition.SetAlternative("de", "word 1");
			_repository.SaveItem(entryBeforeFirstQuery);

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByDefinitionOrGloss(new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(1, results.Count);
			Assert.AreEqual("word 1", results[0]["Form"]);
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_ModifyAndSaveMultipleAfterFirstCall_EntriesModifiedAndSortedInResultSet()
		{
			List<LexEntry> entriesToModify = new List<LexEntry>();
			entriesToModify.Add(CreateEntryWithDefinitionBeforeFirstQuery("de", "word 0"));
			entriesToModify.Add(CreateEntryWithDefinitionBeforeFirstQuery("de", "word 1"));

			_repository.GetAllEntriesSortedByDefinitionOrGloss(new WritingSystem("de", SystemFonts.DefaultFont));

			entriesToModify[0].Senses[0].Definition["de"] = "word 3";
			entriesToModify[1].Senses[0].Definition["de"] = "word 2";
			_repository.SaveItems(entriesToModify);

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByDefinitionOrGloss(new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(2, results.Count);
			Assert.AreEqual("word 2", results[0]["Form"]);
			Assert.AreEqual("word 3", results[1]["Form"]);
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DeleteAfterFirstCall_EntryIsDeletedInResultSet()
		{
			LexEntry entrytoBeDeleted = CreateEntryWithDefinitionBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByDefinitionOrGloss(new WritingSystem("de", SystemFonts.DefaultFont));

			_repository.DeleteItem(entrytoBeDeleted);

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByDefinitionOrGloss(new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(0, results.Count);
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DeleteByIdAfterFirstCall_EntryIsDeletedInResultSet()
		{
			LexEntry entrytoBeDeleted = CreateEntryWithDefinitionBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByDefinitionOrGloss(new WritingSystem("de", SystemFonts.DefaultFont));

			_repository.DeleteItem(_repository.GetId(entrytoBeDeleted));

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByDefinitionOrGloss(new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(0, results.Count);
		}

		[Test]
		public void GetAllEntriesSortedByDefinition_DeleteAllItemsAfterFirstCall_EntryIsDeletedInResultSet()
		{
			LexEntry entrytoBeDeleted = CreateEntryWithDefinitionBeforeFirstQuery("de", "word 0");

			_repository.GetAllEntriesSortedByDefinitionOrGloss(new WritingSystem("de", SystemFonts.DefaultFont));

			_repository.DeleteAllItems();

			ResultSet<LexEntry> results = _repository.GetAllEntriesSortedByDefinitionOrGloss(new WritingSystem("de", SystemFonts.DefaultFont));
			Assert.AreEqual(0, results.Count);
		}


	}
}