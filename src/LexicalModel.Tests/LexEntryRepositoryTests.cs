using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using LiftIO.Parsing;
using NUnit.Framework;
using Palaso.Text;
using WeSay.Data;
using WeSay.Foundation;
using WeSay.LexicalModel.Db4oSpecific;

namespace WeSay.LexicalModel.Tests
{
	[TestFixture]
	public class LexEntryRepositoryTests
	{
		private string _filePath;
		private LexEntryRepository _lexEntryRepository;
		private WritingSystem _headwordWritingSystem;
		private Db4oLexEntryRepository _db4oRepository;

		[SetUp]
		public void Setup()
		{
			_filePath = Path.GetTempFileName();
			CycleDatabase();
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
		public void GetEntriesByApproximateLexicalFormShouldNotContainMatchesFromOtherWritingSystems
				()
		{
			const string entryInOtherWritingSystem = "foo2";
			MakeTestLexEntry("v", "foo1");
			MakeTestLexEntry("en", entryInOtherWritingSystem);
			MakeTestLexEntry("v", "foo3");
			WritingSystem ws = new WritingSystem("v", SystemFonts.DefaultFont);

			ResultSet<LexEntry> matches =
					_lexEntryRepository.GetEntriesWithSimilarLexicalForm("foo",
																		 ws,
																		 ApproximateMatcherOptions.
																				 IncludePrefixedForms);
			Assert.AreEqual(2, matches.Count);
			Assert.AreNotEqual(entryInOtherWritingSystem, matches[1]["Form"]);
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
			_db4oRepository.Database.Set(new LanguageForm("en", "findme", new MultiText()));
			CycleDatabase();
			WritingSystem writingSystem = new WritingSystem("en", SystemFonts.DefaultFont);
			ResultSet<LexEntry> list =
					_lexEntryRepository.GetEntriesWithMatchingLexicalForm("findme", writingSystem);
			Assert.AreEqual(1, list.Count);
		}

		[Test]
		public void FindEntryFromGuid()
		{
			Guid g = SetupEntryWithGuid();

			LexEntry found = _lexEntryRepository.GetLexEntryWithMatchingGuid(g);
			Assert.AreEqual("hello", found.LexicalForm["en"]);
		}

		private Guid SetupEntryWithGuid()
		{
			CycleDatabase();
			AddEntryWithGloss("hello");

			Guid g = Guid.NewGuid();
			CreateEntryWithGuid(g);

			AddEntryWithGloss("world");
			CycleDatabase();
			return g;
		}

		private void CreateEntryWithGuid(Guid g)
		{
			Extensible extensible = new Extensible();
			extensible.Guid = g;
			LexEntry entry = _lexEntryRepository.CreateItem(extensible);
			entry.LexicalForm["en"] = "hello";
			_lexEntryRepository.SaveItem(entry);
		}

		[Test]
		public void CannotFindEntryFromGuid()
		{
			SetupEntryWithGuid();

			LexEntry found = _lexEntryRepository.GetLexEntryWithMatchingGuid(Guid.NewGuid());
			Assert.IsNull(found);
		}

		[Test]
		[ExpectedException(typeof (ApplicationException))]
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
			_db4oRepository.Database.Set(glossLanguageForm);

			WritingSystem writingSystem = new WritingSystem("en", SystemFonts.DefaultFont);
			ResultSet<LexEntry> list =
					_lexEntryRepository.GetEntriesWithMatchingGlossSortedByLexicalForm(
							glossLanguageForm, writingSystem);
			Assert.AreEqual(1, list.Count);
		}

		private void AddEntryWithGloss(string gloss)
		{
			LexEntry entry = _lexEntryRepository.CreateItem();
			LexSense sense = new LexSense();
			entry.Senses.Add(sense);
			sense.Gloss["en"] = gloss;
			_lexEntryRepository.SaveItem(entry);
		}

		private void CycleDatabase()
		{
			if (_lexEntryRepository != null)
			{
				_lexEntryRepository.Dispose();
			}
			_db4oRepository = new Db4oLexEntryRepository(_filePath);
			_lexEntryRepository = new LexEntryRepository(_db4oRepository);
		}

		[Test]
		public void GetHomographNumber_OnlyOneEntry_Returns0()
		{
			LexEntry entry1 = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			Assert.AreEqual(0,
							_lexEntryRepository.GetHomographNumber(entry1, _headwordWritingSystem));
		}

		[Test, Ignore("Homograph order is not well defined CJP")]
		public void GetHomographNumber_FirstEntryWithFollowingHomograph_Returns1()
		{
			LexEntry entry1 = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			Assert.AreEqual(1,
							_lexEntryRepository.GetHomographNumber(entry1, _headwordWritingSystem));
		}

		[Test, Ignore("Homograph order is not well defined CJP")]
		public void GetHomographNumber_SecondEntry_Returns2()
		{
			MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			LexEntry entry2 = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			Assert.AreEqual(2,
							_lexEntryRepository.GetHomographNumber(entry2, _headwordWritingSystem));
		}

		[Test]
		public void GetHomographNumber_AssignesUniqueNumbers()
		{
			LexEntry entryOther = MakeEntryWithLexemeForm("en", "blue");
			Assert.AreNotEqual("en", _headwordWritingSystem.Id);
			LexEntry[] entries = new LexEntry[3];
			entries[0] = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			entries[1]= MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			entries[2] = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			List<int> ids = new List<int>(entries.Length);
			foreach (LexEntry entry in entries)
			{
				int homographNumber = _lexEntryRepository.GetHomographNumber(entry, _headwordWritingSystem);
				Assert.IsFalse(ids.Contains(homographNumber));
				ids.Add(homographNumber);
			}
		}

		[Test, Ignore("Homograph order is not well defined CJP")]
		public void GetHomographNumber_ThirdEntry_Returns3()
		{
			LexEntry entryOther = MakeEntryWithLexemeForm("en", "blue");
			Assert.AreNotEqual("en", _headwordWritingSystem.Id);
			LexEntry entry1 = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			LexEntry entry2 = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			LexEntry entry3 = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			Console.WriteLine("ID0: {0}", _lexEntryRepository.GetId(entryOther));
			Console.WriteLine("ID1: {0}", _lexEntryRepository.GetId(entry1));
			Console.WriteLine("ID2: {0}", _lexEntryRepository.GetId(entry2));
			Console.WriteLine("ID3: {0}", _lexEntryRepository.GetId(entry3));
			Assert.AreEqual(3, _lexEntryRepository.GetHomographNumber(entry3, _headwordWritingSystem));
			Assert.AreEqual(2, _lexEntryRepository.GetHomographNumber(entry2, _headwordWritingSystem));
			Assert.AreEqual(1, _lexEntryRepository.GetHomographNumber(entry1, _headwordWritingSystem));
		}

		[Test, Ignore("Homograph order is not well defined CJP")]
		public void GetHomographNumber_3SameLexicalForms_Returns123()
		{
			LexEntry entry1 = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			LexEntry entry2 = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			LexEntry entry3 = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			Assert.AreEqual(1,
							_lexEntryRepository.GetHomographNumber(entry1, _headwordWritingSystem));
			Assert.AreEqual(3,
							_lexEntryRepository.GetHomographNumber(entry3, _headwordWritingSystem));
			Assert.AreEqual(2,
							_lexEntryRepository.GetHomographNumber(entry2, _headwordWritingSystem));
		}

		[Test, Ignore("Homograph order is not well defined CJP")]
		public void GetHomographNumber_3SameLexicalFormsAnd3OtherLexicalForms_Returns123()
		{
			LexEntry red1 = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "red");
			//Thread.Sleep(1100);
			LexEntry blue1 = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			//Thread.Sleep(1100);
			LexEntry red2 = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "red");
			//Thread.Sleep(1100);
			LexEntry blue2 = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			//Thread.Sleep(1100);
			LexEntry red3 = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "red");
			//Thread.Sleep(1100);
			LexEntry blue3 = MakeEntryWithLexemeForm(_headwordWritingSystem.Id, "blue");
			Assert.AreEqual(1,
							_lexEntryRepository.GetHomographNumber(blue1, _headwordWritingSystem));
			Assert.AreEqual(3,
							_lexEntryRepository.GetHomographNumber(blue3, _headwordWritingSystem));
			Assert.AreEqual(2,
							_lexEntryRepository.GetHomographNumber(blue2, _headwordWritingSystem));
			Assert.AreEqual(1,
							_lexEntryRepository.GetHomographNumber(red1, _headwordWritingSystem));
			Assert.AreEqual(3,
							_lexEntryRepository.GetHomographNumber(red3, _headwordWritingSystem));
			Assert.AreEqual(2,
							_lexEntryRepository.GetHomographNumber(red2, _headwordWritingSystem));
		}

		[Test]
		[Ignore("not implemented")]
		public void GetHomographNumber_HonorsOrderAttribute() {}

		private LexEntry MakeEntryWithLexemeForm(string writingSystemId, string lexicalUnit)
		{
			LexEntry entry = _lexEntryRepository.CreateItem();
			entry.LexicalForm.SetAlternative(writingSystemId, lexicalUnit);
			_lexEntryRepository.SaveItem(entry);
			return entry;
		}

		[Test]
		public void GetAllEntriesSortedByHeadword_3EntriesWithLexemeForms_TokensAreSorted()
		{
			LexEntry e1 = _lexEntryRepository.CreateItem();
			e1.LexicalForm.SetAlternative(_headwordWritingSystem.Id, "bank");
			_lexEntryRepository.SaveItem(e1);
			RepositoryId bankId = _lexEntryRepository.GetId(e1);

			LexEntry e2 = _lexEntryRepository.CreateItem();
			e2.LexicalForm.SetAlternative(_headwordWritingSystem.Id, "apple");
			_lexEntryRepository.SaveItem(e2);
			RepositoryId appleId = _lexEntryRepository.GetId(e2);

			LexEntry e3 = _lexEntryRepository.CreateItem();
			e3.LexicalForm.SetAlternative(_headwordWritingSystem.Id, "xa");
			//has to be something low in the alphabet to test a bug we had
			_lexEntryRepository.SaveItem(e3);
			RepositoryId xaId = _lexEntryRepository.GetId(e3);

			ResultSet<LexEntry> list =
					_lexEntryRepository.GetAllEntriesSortedByHeadword(_headwordWritingSystem);

			Assert.AreEqual(3, list.Count);
			Assert.AreEqual(appleId, list[0].Id);
			Assert.AreEqual(bankId, list[1].Id);
			Assert.AreEqual(xaId, list[2].Id);
		}
	}
}