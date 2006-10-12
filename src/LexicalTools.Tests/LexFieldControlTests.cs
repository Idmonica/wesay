using System;
using NUnit.Framework;
using WeSay.Data;
using WeSay.LexicalModel;
using WeSay.UI;

namespace WeSay.LexicalTools.Tests
{
	[TestFixture]
	public class LexFieldControlTests
	{
		InMemoryRecordListManager _recordListManager;
		IRecordList<LexEntry> _missingTranslationRecordList;
		FieldInventory _fieldInventory;
		private MissingTranslationFilter _missingTranslation;

		public class MissingTranslationFilter:IFilter<LexEntry>
		{

			private bool IsMissingTranslation(LexEntry entry)
			{
				if (entry == null)
				{
					return false;
				}
				bool hasSense = false;
				bool hasExample = false;
				foreach (LexSense sense in entry.Senses)
				{
					hasSense = true;
					foreach (LexExampleSentence exampleSentence in sense.ExampleSentences)
					{
						hasExample = true;
						if (exampleSentence.Translation[BasilProject.Project.WritingSystems.AnalysisWritingSystemDefaultId].Length == 0)
						{
							return true;
						}
					}
				}
				return !(hasSense && hasExample);
			}

			public Predicate<LexEntry> Inquire
			{
				get
				{
					return IsMissingTranslation;
				}
			}

			public string Key
			{
				get
				{
					return "MissingTranslationFilter";
				}
			}
		}
		[SetUp]
		public void SetUp()
		{
			BasilProject.InitializeForTests();
			_recordListManager = new InMemoryRecordListManager();
			this._missingTranslation = new MissingTranslationFilter();
			_recordListManager.Register<LexEntry>(this._missingTranslation);
			_missingTranslationRecordList = _recordListManager.Get<LexEntry>(this._missingTranslation);
			_missingTranslationRecordList.Add(CreateTestEntry("apple", "red thing", "An apple a day keeps the doctor away."));
			_missingTranslationRecordList.Add(CreateTestEntry("banana", "yellow food", "Monkeys like to eat bananas."));
			_missingTranslationRecordList.Add(CreateTestEntry("car", "small motorized vehicle", "Watch out for cars when you cross the street."));
			_missingTranslationRecordList.Add(CreateTestEntry("bike", "vehicle with two wheels", "He rides his bike to school."));

			string[] analysisWritingSystemIds = new string[] { BasilProject.Project.WritingSystems.AnalysisWritingSystemDefaultId };
			string[] vernacularWritingSystemIds = new string[] { BasilProject.Project.WritingSystems.VernacularWritingSystemDefaultId };
			this._fieldInventory = new FieldInventory();
			this._fieldInventory.Add(new Field(Field.FieldNames.EntryLexicalForm.ToString(), vernacularWritingSystemIds));
			this._fieldInventory.Add(new Field(Field.FieldNames.SenseGloss.ToString(), analysisWritingSystemIds));
			this._fieldInventory.Add(new Field(Field.FieldNames.ExampleSentence.ToString(), vernacularWritingSystemIds));
			this._fieldInventory.Add(new Field(Field.FieldNames.ExampleTranslation.ToString(), analysisWritingSystemIds));

		}

		private static LexEntry CreateTestEntry(string lexicalForm, string gloss, string exampleSentence)
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm[BasilProject.Project.WritingSystems.VernacularWritingSystemDefaultId] = lexicalForm;
			LexSense sense = (LexSense)entry.Senses.AddNew();
			sense.Gloss[BasilProject.Project.WritingSystems.AnalysisWritingSystemDefaultId] = gloss;
			LexExampleSentence example = (LexExampleSentence)sense.ExampleSentences.AddNew();
			example.Sentence[BasilProject.Project.WritingSystems.VernacularWritingSystemDefaultId] = exampleSentence;
			return entry;
		}

		private static void AddTranslationToEntry(LexEntry entry, string translation)
		{
			LexSense sense = (LexSense)entry.Senses[0];
			LexExampleSentence example = (LexExampleSentence)sense.ExampleSentences[0];
			example.Translation[BasilProject.Project.WritingSystems.AnalysisWritingSystemDefaultId] = translation;
		}

		[TearDown]
		public void TearDown()
		{
			_recordListManager.Dispose();
		}


		[Test]
		public void Create()
		{
			LexFieldControl lexFieldControl = new LexFieldControl(_missingTranslationRecordList, _fieldInventory, _missingTranslation.Inquire);
			Assert.IsNotNull(lexFieldControl);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Create_NullRecords_Throws()
		{
			new LexFieldControl(null, _fieldInventory, _missingTranslation.Inquire);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Create_NullFieldInventory_Throws()
		{
			new LexFieldControl(_missingTranslationRecordList, null, _missingTranslation.Inquire);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Create_NullFilter_Throws()
		{
			new LexFieldControl(_missingTranslationRecordList, _fieldInventory, null);
		}

		[Test]
		public void CurrentRecord_InitializedToFirst()
		{
			LexFieldControl lexFieldControl = new LexFieldControl(_missingTranslationRecordList, _fieldInventory, _missingTranslation.Inquire);
			Assert.AreEqual(_missingTranslationRecordList[0], lexFieldControl.CurrentRecord);
		}

		[Test]
		public void SetCurrentRecordToPrevious_AtFirst_StaysAtFirst()
		{
			LexFieldControl lexFieldControl = new LexFieldControl(_missingTranslationRecordList, _fieldInventory, _missingTranslation.Inquire);
			lexFieldControl.SetCurrentRecordToPrevious();
			Assert.AreEqual(_missingTranslationRecordList[0], lexFieldControl.CurrentRecord);
		}

		[Test]
		public void SetCurrentRecordToNextThenPrevious_SamePlace()
		{
			LexFieldControl lexFieldControl = new LexFieldControl(_missingTranslationRecordList, _fieldInventory, _missingTranslation.Inquire);
			lexFieldControl.SetCurrentRecordToNext();
			lexFieldControl.SetCurrentRecordToPrevious();
			Assert.AreEqual(_missingTranslationRecordList[0], lexFieldControl.CurrentRecord);
		}

		[Test]
		public void SetCurrentRecordToNext_AtLast_StaysAtLast()
		{
			LexFieldControl lexFieldControl = new LexFieldControl(_missingTranslationRecordList, _fieldInventory, _missingTranslation.Inquire);
			int count = _missingTranslationRecordList.Count;
			for (int i = 0; i <= count; i++)
			{
				lexFieldControl.SetCurrentRecordToNext();
			}
			Assert.AreEqual(_missingTranslationRecordList[count - 1], lexFieldControl.CurrentRecord);
		}

		[Test]
		public void SetCurrentRecordToNext_GoesToNext()
		{
			LexFieldControl lexFieldControl = new LexFieldControl(_missingTranslationRecordList, _fieldInventory, _missingTranslation.Inquire);
			lexFieldControl.SetCurrentRecordToNext();
			Assert.AreEqual(_missingTranslationRecordList[1], lexFieldControl.CurrentRecord);
		}

		[Test]
		public void SetCurrentRecordToPrevious_AfterChangedSoNoLongerMeetsFilter_StaysAtFirst()
		{
			LexFieldControl lexFieldControl = new LexFieldControl(_missingTranslationRecordList, _fieldInventory, _missingTranslation.Inquire);
			AddTranslationToEntry(lexFieldControl.CurrentRecord,"a bogus translation of example");
			lexFieldControl.SetCurrentRecordToPrevious();
			Assert.AreEqual(_missingTranslationRecordList[0], lexFieldControl.CurrentRecord);
		}

		[Test]
		public void SetCurrentRecordToPrevious_AfterChangedSoNoLongerMeetsFilter_GoesToEntryBeforeChangedOne()
		{
			LexFieldControl lexFieldControl = new LexFieldControl(_missingTranslationRecordList, _fieldInventory, _missingTranslation.Inquire);
			lexFieldControl.SetCurrentRecordToNext();
			lexFieldControl.SetCurrentRecordToNext();
			AddTranslationToEntry(lexFieldControl.CurrentRecord, "a bogus translation of example");
			lexFieldControl.SetCurrentRecordToPrevious();
			Assert.AreEqual(_missingTranslationRecordList[1], lexFieldControl.CurrentRecord);
		}

		[Test]
		public void SetCurrentRecordToNextThenPrevious_AfterChangedSoNoLongerMeetsFilter_SamePlace()
		{
			LexFieldControl lexFieldControl = new LexFieldControl(_missingTranslationRecordList, _fieldInventory, _missingTranslation.Inquire);
			lexFieldControl.SetCurrentRecordToNext();
			AddTranslationToEntry(lexFieldControl.CurrentRecord, "a bogus translation of example");
			lexFieldControl.SetCurrentRecordToPrevious();
			Assert.AreEqual(_missingTranslationRecordList[0], lexFieldControl.CurrentRecord);
		}

		[Test]
		public void SetCurrentRecordToPrevious_AtLast_AfterChangedSoNoLongerMeetsFilter_StaysAtLast()
		{
			LexFieldControl lexFieldControl = new LexFieldControl(_missingTranslationRecordList, _fieldInventory, _missingTranslation.Inquire);
			int count = _missingTranslationRecordList.Count;
			for (int i = 0; i < count; i++)
			{
				lexFieldControl.SetCurrentRecordToNext();
			}
			AddTranslationToEntry(lexFieldControl.CurrentRecord, "a bogus translation of example");
			lexFieldControl.SetCurrentRecordToPrevious();
			Assert.AreEqual(_missingTranslationRecordList[count - 2], lexFieldControl.CurrentRecord);
		}

		[Test]
		public void SetCurrentRecordToPrevious_AtSecondToLast_AfterChangedSoNoLongerMeetsFilter_GoesToEntryBeforeChangedOne()
		{
			LexFieldControl lexFieldControl = new LexFieldControl(_missingTranslationRecordList, _fieldInventory, _missingTranslation.Inquire);
			int count = _missingTranslationRecordList.Count;
			for (int i = 0; i < count-2; i++)
			{
				lexFieldControl.SetCurrentRecordToNext();
			}
			AddTranslationToEntry(lexFieldControl.CurrentRecord, "a bogus translation of example");
			lexFieldControl.SetCurrentRecordToPrevious();
			Assert.AreEqual(_missingTranslationRecordList[count - 3], lexFieldControl.CurrentRecord);
		}


		[Test]
		public void SetCurrentRecordToNext_AtLast_AfterChangedSoNoLongerMeetsFilter_StaysAtLast()
		{
			LexFieldControl lexFieldControl = new LexFieldControl(_missingTranslationRecordList, _fieldInventory, _missingTranslation.Inquire);
			int count = _missingTranslationRecordList.Count;
			for (int i = 0; i < count; i++)
			{
				lexFieldControl.SetCurrentRecordToNext();
			}
			AddTranslationToEntry(lexFieldControl.CurrentRecord, "a bogus translation of example");
			lexFieldControl.SetCurrentRecordToNext();
			Assert.AreEqual(_missingTranslationRecordList[count - 2], lexFieldControl.CurrentRecord);
		}

		[Test]
		public void SetCurrentRecordToNext__AfterChangedSoNoLongerMeetsFilter_GoesToNext()
		{
			LexFieldControl lexFieldControl = new LexFieldControl(_missingTranslationRecordList, _fieldInventory, _missingTranslation.Inquire);
			lexFieldControl.SetCurrentRecordToNext();
			AddTranslationToEntry(lexFieldControl.CurrentRecord, "a bogus translation of example");
			lexFieldControl.SetCurrentRecordToNext();
			Assert.AreEqual(_missingTranslationRecordList[1], lexFieldControl.CurrentRecord);
		}


	}
}