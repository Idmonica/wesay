using NUnit.Framework;
using WeSay.LexicalModel;
using WeSay.LexicalModel.Tests;
using WeSay.UI;
using System.Windows.Forms;

namespace WeSay.LexicalTools.Tests
{
	[TestFixture]
	public class TestFieldControl
	{
		LexEntry empty;
		LexEntry apple;
		LexEntry banana;
		LexEntry car;
		LexEntry bike;

		[SetUp]
		public void SetUp()
		{
			BasilProject.InitializeForTests();

			empty = CreateTestEntry("", "", "");
			apple = CreateTestEntry("apple", "red thing", "An apple a day keeps the doctor away.");
			banana = CreateTestEntry("banana", "yellow food", "Monkeys like to eat bananas.");
			car = CreateTestEntry("car", "small motorized vehicle", "Watch out for cars when you cross the street.");
			bike = CreateTestEntry("bike", "vehicle with two wheels", "He rides his bike to school.");
		}

		[Test]
		public void Create()
		{
			LexFieldControl lexFieldControl = new LexFieldControl();
			Assert.IsNotNull(lexFieldControl);
		}

		[Test]
		public void CreateWithFilter()
		{
			LexFieldControl lexFieldControl = new LexFieldControl(delegate(string s)
																	{
																		return true;
																	});
			Assert.IsNotNull(lexFieldControl);
		}

		[Test]
		public void NullDataSource_ShowsEmpty()
		{
			LexFieldControl lexFieldControl = CreateForm(null);
			Assert.AreEqual(string.Empty, lexFieldControl.ControlFormattedView.Text);
		}

		[Test]
		public void FormattedView_ShowsCurrentEntry()
		{
		   TestEntryShows(apple);
		   TestEntryShows(banana);
		}

		private static void TestEntryShows(LexEntry entry)
		{
			LexFieldControl lexFieldControl = CreateForm(entry);
			Assert.IsTrue(lexFieldControl.ControlFormattedView.Text.Contains(GetLexicalForm(entry)));
			Assert.IsTrue(lexFieldControl.ControlFormattedView.Text.Contains(GetGloss(entry)));
			Assert.IsTrue(lexFieldControl.ControlFormattedView.Text.Contains(GetExampleSentence(entry)));
		}

		[Test, Ignore("For now, we also show the ghost field in this situation.")]
		public void EditField_SingleControl()
		{
			LexFieldControl lexFieldControl = CreateFilteredForm(apple, "Gloss");
			Assert.AreEqual(1, lexFieldControl.ControlEntryDetail.Count);
		}

		[Test]
		public void EditField_SingleControlWithGhost()
		{
			LexFieldControl lexFieldControl = CreateFilteredForm(apple, "Gloss GhostGloss");
			Assert.AreEqual(2, lexFieldControl.ControlEntryDetail.Count);
		}

		[Test]
		public void EditField_MapsToLexicalForm()
		{
			TestEditFieldMapsToLexicalForm(car);
			TestEditFieldMapsToLexicalForm(bike);
		}

		private static void TestEditFieldMapsToLexicalForm(LexEntry entry)
		{
			LexFieldControl lexFieldControl = CreateFilteredForm(entry, "Gloss");
			EntryDetailControl entryDetailControl = lexFieldControl.ControlEntryDetail;
			Control referenceControl = entryDetailControl.GetControlOfRow(0);
			Label labelControl = entryDetailControl.GetLabelControlFromReferenceControl(referenceControl);
			Assert.AreEqual("Meaning 1", labelControl.Text);
			Control editControl = entryDetailControl.GetEditControlFromReferenceControl(referenceControl);
			Assert.IsTrue(editControl.Text.Contains(GetGloss(entry)));
		}

		[Test]
		public void EditField_Change_DisplayedInFormattedView()
		{
			LexFieldControl lexFieldControl = CreateFilteredForm(apple, "LexicalForm");
			EntryDetailControl entryDetailControl = lexFieldControl.ControlEntryDetail;
			Control referenceControl = entryDetailControl.GetControlOfRow(0);
			Control editControl = entryDetailControl.GetEditControlFromReferenceControl(referenceControl);
			editControl.Text = "test";
			Assert.IsTrue(lexFieldControl.ControlFormattedView.Text.Contains("test"));
	   }

		[Test]
		public void FormattedView_FocusInControl_Displayed()
		{
			LexFieldControl lexFieldControl = CreateFilteredForm(apple, "LexicalForm");
			lexFieldControl.ControlFormattedView.Select();
			string rtfOriginal = lexFieldControl.ControlFormattedView.Rtf;

			EntryDetailControl entryDetailControl = lexFieldControl.ControlEntryDetail;
			Control referenceControl = entryDetailControl.GetControlOfRow(0);
			Control editControl = entryDetailControl.GetEditControlFromReferenceControl(referenceControl);
			editControl.Select();
			Assert.AreNotEqual(rtfOriginal, lexFieldControl.ControlFormattedView.Rtf);
		}

		[Test]
		public void FormattedView_ChangeRecordThenBack_NothingHighlighted()
		{
			LexFieldControl lexFieldControl = CreateFilteredForm(apple, "LexicalForm");
			lexFieldControl.ControlFormattedView.Select();
			string rtfAppleNothingHighlighted = lexFieldControl.ControlFormattedView.Rtf;

			EntryDetailControl entryDetailControl = lexFieldControl.ControlEntryDetail;
			Control referenceControl = entryDetailControl.GetControlOfRow(0);
			Control editControl = entryDetailControl.GetEditControlFromReferenceControl(referenceControl);
			editControl.Select();
			Assert.AreNotEqual(rtfAppleNothingHighlighted, lexFieldControl.ControlFormattedView.Rtf);

			lexFieldControl.DataSource = banana;
			lexFieldControl.DataSource = apple;
			Assert.AreEqual(rtfAppleNothingHighlighted, lexFieldControl.ControlFormattedView.Rtf);
		}

		[Test]
		public void FormattedView_EmptyField_StillHighlighted()
		{
			LexFieldControl lexFieldControl = CreateFilteredForm(empty, "LexicalForm");
			lexFieldControl.ControlFormattedView.Select();
			string rtfEmptyNothingHighlighted = lexFieldControl.ControlFormattedView.Rtf;

			EntryDetailControl entryDetailControl = lexFieldControl.ControlEntryDetail;
			Control referenceControl = entryDetailControl.GetControlOfRow(0);
			Control editControl = entryDetailControl.GetEditControlFromReferenceControl(referenceControl);
			editControl.Select();
			Assert.AreNotEqual(rtfEmptyNothingHighlighted, lexFieldControl.ControlFormattedView.Rtf);
		}

		private static LexFieldControl CreateForm(LexEntry entry)
		{
			LexFieldControl lexFieldControl = new LexFieldControl();
			lexFieldControl.DataSource = entry;

			return lexFieldControl;
		}


		private static LexFieldControl CreateFilteredForm(LexEntry entry, string s)
		{
			LexFieldControl lexFieldControl = new LexFieldControl(delegate(string fieldName)
														{
															return s.Contains(fieldName);
														});
			lexFieldControl.DataSource = entry;
			return lexFieldControl;
		}

		private static LexEntry CreateTestEntry(string lexicalForm, string gloss, string exampleSentence)
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm[BasilProject.Project.WritingSystems.VernacularWritingSystemDefault.Id] = lexicalForm;
			LexSense sense = (LexSense)entry.Senses.AddNew();
			sense.Gloss[BasilProject.Project.WritingSystems.AnalysisWritingSystemDefault.Id] = gloss;
			LexExampleSentence example = (LexExampleSentence)sense.ExampleSentences.AddNew();
			example.Sentence[BasilProject.Project.WritingSystems.VernacularWritingSystemDefault.Id] = exampleSentence;
			return entry;
		}

		private static string GetLexicalForm(LexEntry entry)
		{
			return entry.LexicalForm.GetFirstAlternative();
		}

		private static string GetGloss(LexEntry entry)
		{
			return ((LexSense)entry.Senses[0]).Gloss.GetFirstAlternative();
		}

		private static string GetExampleSentence(LexEntry entry)
		{
			return ((LexExampleSentence)((LexSense)entry.Senses[0]).ExampleSentences[0]).Sentence.GetFirstAlternative();
		}

	}
}
