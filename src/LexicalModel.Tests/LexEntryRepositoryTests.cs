using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using LiftIO.Parsing;
using NUnit.Framework;
using Palaso.Text;
using WeSay.Data;
using WeSay.Foundation;
using WeSay.Language;

namespace WeSay.LexicalModel.Tests
{
	[TestFixture]
	public class LexEntryRepositoryTests
	{
		private string _filePath;
		private LexEntryRepository _lexEntryRepository;
		private WritingSystem _headwordWritingSystem;

		[SetUp]
		public void Setup()
		{
			_filePath = Path.GetTempFileName();
			_lexEntryRepository = new LexEntryRepository(_filePath);
			_headwordWritingSystem = new WritingSystem();
			_headwordWritingSystem.Id = "primary";
		}

		[TearDown]
		public void TearDown()
		{
			_lexEntryRepository.Dispose();
			File.Delete(_filePath);
		}

		private void MakeTestLexEntry(string writingSystemId, string lexicalForm)
		{
			LexEntry entry = _lexEntryRepository.CreateItem();
			entry.LexicalForm.SetAlternative(writingSystemId, lexicalForm);
			_lexEntryRepository.SaveItem(entry);
			return;
		}

		[Test]
		public void GetEntriesByApproximateLexicalFormShouldNotContainMatchesFromOtherWritingSystems()
		{
			const string entryInOtherWritingSystem = "foo2";
			MakeTestLexEntry("v","foo1");
			MakeTestLexEntry("en", entryInOtherWritingSystem);
			MakeTestLexEntry("v","foo3");
			WritingSystem ws = new WritingSystem("v", SystemFonts.DefaultFont);

			IList<RecordToken> matches = _lexEntryRepository.GetEntriesWithSimilarLexicalForm("foo", ws, ApproximateMatcherOptions.IncludePrefixedForms);
			Assert.AreEqual(2, matches.Count);
			Assert.AreNotEqual(entryInOtherWritingSystem, matches[1].DisplayString);
		}

		[Test]
		public void FindEntriesFromLexemeForm()
		{
			CycleDatabase();
			LexEntry entry = _lexEntryRepository.CreateItem();
			entry.LexicalForm["en"] = "findme";
			_lexEntryRepository.SaveItem(entry);
			CycleDatabase();
			//don't want to find this one
			_lexEntryRepository.Db4oDataSource.Data.Set(new LanguageForm("en", "findme", new MultiText()));
			CycleDatabase();
			WritingSystem writingSystem = new WritingSystem("en", SystemFonts.DefaultFont);
			List<RecordToken> list = _lexEntryRepository.GetEntriesWithMatchingLexicalForm("findme", writingSystem);
			Assert.AreEqual(1, list.Count);
		}

		[Test]
		public void FindEntryFromGuid()
		{
			Guid g = SetupEntryWithGuid();

			LexEntry found = _lexEntryRepository.GetLexEntryWithMatchingGuid(g);
			Assert.AreEqual("hello", found.LexicalForm["en"]);
		}

		private Guid SetupEntryWithGuid() {
			CycleDatabase();
			AddEntryWithGloss("hello");

			Guid g = Guid.NewGuid();
			CreateEntryWithGuid(g);

			AddEntryWithGloss("world");
			CycleDatabase();
			return g;
		}

		private void CreateEntryWithGuid(Guid g) {
			Extensible extensible = new Extensible();
			extensible.Guid = g;
			LexEntry entry = this._lexEntryRepository.CreateItem(extensible);
			entry.LexicalForm["en"] = "hello";
			this._lexEntryRepository.SaveItem(entry);
		}

		[Test]
		public void CannotFindEntryFromGuid()
		{
			SetupEntryWithGuid();

			LexEntry found = _lexEntryRepository.GetLexEntryWithMatchingGuid(new Guid());
			Assert.IsNull(found);
		}

		[Test, ExpectedException(typeof(ApplicationException))]
		public void MultipleGuidMatchesThrows()
		{
			Guid g = SetupEntryWithGuid();
			CreateEntryWithGuid(g);
			CycleDatabase();
		   _lexEntryRepository.GetLexEntryWithMatchingGuid(g);
		}

		[Test]
		public void FindEntriesFromGloss()
		{
			CycleDatabase();
			string gloss = "ant";
			AddEntryWithGloss(gloss);
			CycleDatabase();
			//don't want to find this one
			LanguageForm glossLanguageForm = new LanguageForm("en", gloss, new MultiText());
			_lexEntryRepository.Db4oDataSource.Data.Set(glossLanguageForm);

			WritingSystem writingSystem = new WritingSystem("en", SystemFonts.DefaultFont);
			List<RecordToken> list = _lexEntryRepository.GetEntriesWithMatchingGlossSortedByLexicalForm(glossLanguageForm, writingSystem);
			Assert.AreEqual(1, list.Count);
		}

		private void AddEntryWithGloss(string gloss)
		{
			LexEntry entry = _lexEntryRepository.CreateItem();
			LexSense sense = (LexSense)entry.Senses.AddNew();
			sense.Gloss["en"] = gloss;
			_lexEntryRepository.SaveItem(entry);
		}

		private void CycleDatabase()
		{
			if (_lexEntryRepository != null)
			{
				_lexEntryRepository.Dispose();
			}
			_lexEntryRepository = new LexEntryRepository(_filePath);
		}

		[Test]
		public void GetHomographNumber_OnlyOneEntry_Returns0()
		{
			LexEntry entry1 = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			Assert.AreEqual(0, _lexEntryRepository.GetHomographNumber(entry1, _headwordWritingSystem));
		}



		[Test]
		public void GetHomographNumber_FirstEntryWithFollowingHomograph_Returns1()
		{
			LexEntry entry1 = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			Assert.AreEqual(1, _lexEntryRepository.GetHomographNumber(entry1, _headwordWritingSystem));
		}



		[Test]
		public void GetHomographNumber_SecondEntry_Returns2()
		{
			MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			LexEntry entry2 = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			Assert.AreEqual(2, _lexEntryRepository.GetHomographNumber(entry2, _headwordWritingSystem));
		}



		[Test]
		public void GetHomographNumber_ThirdEntry_Returns3()
		{
			MakeEntryWithLexemeForm("en", "blue");
			MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			LexEntry entry3 = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			Assert.AreEqual(3, _lexEntryRepository.GetHomographNumber(entry3, _headwordWritingSystem));
		}

		[Test]
		public void GetHomographNumber_3SameLexicalForms_Returns123()
		{
			LexEntry entry1 = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			LexEntry entry2 = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			LexEntry entry3 = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			Assert.AreEqual(1, _lexEntryRepository.GetHomographNumber(entry1, _headwordWritingSystem));
			Assert.AreEqual(3, _lexEntryRepository.GetHomographNumber(entry3, _headwordWritingSystem));
			Assert.AreEqual(2, _lexEntryRepository.GetHomographNumber(entry2, _headwordWritingSystem));
		}

		[Test, Ignore("not implemented")]
		public void GetHomographNumber_HonorsOrderAttribute()
		{
		}

		private LexEntry MakeEntryWithLexemeForm(string writingSystemId, string lexicalUnit)
		{
			LexEntry entry = _lexEntryRepository.CreateItem();
			entry.LexicalForm.SetAlternative(writingSystemId, lexicalUnit);
			_lexEntryRepository.SaveItem(entry);
			return entry;
		}
	}
}
