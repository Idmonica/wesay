using System;
using System.IO;
using System.Text;
using System.Xml;
using LiftIO.Parsing;
using NUnit.Framework;
using WeSay.Project;

namespace WeSay.LexicalModel.Tests
{
	[TestFixture]
	public class LiftRoundTripTests
	{
		private LiftExporter _exporter;
		private StringBuilder _stringBuilder;
		private LiftMerger _merger;
		private LiftRepository _repository;
		private string _tempFile;

		[SetUp]
		public void Setup()
		{
			WeSayWordsProject.InitializeForTests();
			_stringBuilder = new StringBuilder();

			_tempFile = Path.GetTempFileName();
			_exporter = new LiftExporter(_stringBuilder, false);
			_repository = new LiftRepository(_tempFile);
			_merger = new LiftMerger(_repository);
		}

		[TearDown]
		public void TearDown()
		{
			_merger.Dispose();
			_repository.Dispose();
			File.Delete(_tempFile);
		}

		private LexEntry MakeSimpleEntry()
		{
			Extensible extensibleInfo = new Extensible();
			return _merger.GetOrMakeEntry(extensibleInfo, 0);
		}

		private static LiftMultiText MakeBasicLiftMultiText()
		{
			LiftMultiText forms = new LiftMultiText();
			forms.Add("ws-one", "uno");
			forms.Add("ws-two", "dos");
			return forms;
		}

		private void AssertXPathNotNull(string xpath)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(_stringBuilder.ToString());
			XmlNode node = doc.SelectSingleNode(xpath);
			if (node == null)
			{
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.ConformanceLevel = ConformanceLevel.Fragment;
				XmlWriter writer = XmlWriter.Create(Console.Out, settings);
				doc.WriteContentTo(writer);
				writer.Flush();
			}
			Assert.IsNotNull(node, "Not matched: " + xpath);
		}

		[Test]
		public void Subsense()
		{
			LexEntry e = MakeSimpleEntry();
			string xml =
					@"  <entry id='flob'>
				  <sense id='opon_1' order='1'>
					  <subsense id='opon_1a' order='1'>
						<grammatical-info value='n'/>
						<gloss lang='en'>
						  <text>grand kin</text>
						</gloss>
						<definition>
						  <form lang='en'>
							<text>
							  grandparent, grandchild; reciprocal term of
							  plus or minus two generations
							</text>
						  </form>
						</definition>
					  </subsense>
					  <subsense id='opon_1b' order='2'>
						<grammatical-info value='n'/>
						<gloss lang='en'>
						  <text>ancestor</text>
						</gloss>
					  </subsense>
					</sense>
			</entry>";

			LexSense sense = new LexSense();
			e.Senses.Add(sense);
			_merger.GetOrMakeSubsense(sense, new Extensible(), xml);
			_merger.FinishEntry(e);
			_exporter.Add(e);
			_exporter.End();
			AssertXPathNotNull("//entry/sense/subsense[@id='opon_1b' and @order='2']/gloss");
			AssertXPathNotNull("//entry/sense/subsense/grammatical-info");
		}

		[Test]
		public void FieldOnEntry_ContentPreserved()
		{
			LexEntry e = MakeSimpleEntry();

			_merger.MergeInField(e,
								 "color",
								 default(DateTime),
								 default(DateTime),
								 MakeBasicLiftMultiText(),
								 null);
			_merger.FinishEntry(e);

			_exporter.Add(e);
			_exporter.End();
			AssertXPathNotNull("//entry/field[@type='color']/form[@lang='ws-one']");
			AssertXPathNotNull("//entry/field[@type='color']/form[@lang='ws-two']");
		}

		[Test]
		public void Entry_Order()
		{
			Extensible extensibleInfo = new Extensible();
			LexEntry entry4 = this._merger.GetOrMakeEntry(extensibleInfo, 4);
			this._merger.FinishEntry(entry4);
			LexEntry entry1 = this._merger.GetOrMakeEntry(extensibleInfo, 1);
			this._merger.FinishEntry(entry1);
			LexEntry entry2 = this._merger.GetOrMakeEntry(extensibleInfo, 2);
			this._merger.FinishEntry(entry2);
			_exporter.Add(entry4, 3);
			_exporter.Add(entry1, 1);
			_exporter.Add(entry2, 2);

			_exporter.End();
			AssertXPathNotNull("//entry[@order='1']");
			AssertXPathNotNull("//entry[@order='2']");
			AssertXPathNotNull("//entry[@order='3']");
		}

		[Test]
		public void ExampleTranslation_OneWithNoType()
		{
			LexEntry e = MakeSimpleEntry();
			LexSense sense = new LexSense();
			e.Senses.Add(sense);
			LexExampleSentence ex = new LexExampleSentence();
			sense.ExampleSentences.Add(ex);
			LiftMultiText translation = new LiftMultiText();
			translation.Add("aa", "aaaa");
			_merger.MergeInTranslationForm(ex, "", translation, "bogus raw xml");
			_merger.FinishEntry(e);
			_exporter.Add(e);
			_exporter.End();
			AssertXPathNotNull("//entry/sense/example/translation[not(@type)]/form[@lang='aa']");
		}

		[Test]
		public void ExampleTranslations_MultipleTypes()
		{
			LexEntry e = MakeSimpleEntry();
			LexSense sense = new LexSense();
			e.Senses.Add(sense);
			LexExampleSentence ex = new LexExampleSentence();
			sense.ExampleSentences.Add(ex);
			LiftMultiText translation = new LiftMultiText();
			translation.Add("aa", "unmarked translation");
			_merger.MergeInTranslationForm(ex, "", translation, "bogus raw xml");
			LiftMultiText t2 = new LiftMultiText();
			t2.Add("aa", "type2translation");
			_merger.MergeInTranslationForm(ex,
										   "type2",
										   t2,
										   "<translation type='type2'><bogus/></translation>");
			_merger.FinishEntry(e);

			_exporter.Add(e);
			_exporter.End();
			AssertXPathNotNull(
					"//entry/sense/example/translation[not(@type)]/form[@lang='aa']/text[text()='unmarked translation']");
			AssertXPathNotNull("//entry/sense/example/translation[@type='type2']/bogus");
		}

		[Test]
		public void ExampleTranslations_UnmarkedThenFree()
		{
			LexEntry e = MakeSimpleEntry();
			LexSense sense = new LexSense();
			e.Senses.Add(sense);
			LexExampleSentence ex = new LexExampleSentence();
			sense.ExampleSentences.Add(ex);

			LiftMultiText translation = new LiftMultiText();
			translation.Add("aa", "unmarked translation");
			_merger.MergeInTranslationForm(ex, "", translation, "bogus raw xml");
			LiftMultiText t2 = new LiftMultiText();
			t2.Add("aa", "freestuff");
			_merger.MergeInTranslationForm(ex,
										   "free",
										   t2,
										   "<translation type='free'><bogus/></translation>");
			_merger.FinishEntry(e);

			_exporter.Add(e);
			_exporter.End();
			AssertXPathNotNull(
					"//entry/sense/example/translation[not(@type)]/form[@lang='aa']/text[text()='unmarked translation']");
			AssertXPathNotNull("//entry/sense/example/translation[@type='free']/bogus");
		}

		[Test]
		public void ExampleTranslations_FreeThenUnmarked()
		{
			LexEntry e = MakeSimpleEntry();
			LexSense sense = new LexSense();
			e.Senses.Add(sense);
			LexExampleSentence ex = new LexExampleSentence();
			sense.ExampleSentences.Add(ex);

			LiftMultiText t2 = new LiftMultiText();
			t2.Add("aa", "freestuff");
			_merger.MergeInTranslationForm(ex,
										   "free",
										   t2,
										   "<translation type='free'><bogus/></translation>");
			LiftMultiText translation = new LiftMultiText();
			translation.Add("aa", "unmarked translation");
			_merger.MergeInTranslationForm(ex,
										   "",
										   translation,
										   "<translation><bogusUnmarked/></translation>");
			_merger.FinishEntry(e);

			_exporter.Add(e);
			_exporter.End();
			AssertXPathNotNull("//entry/sense/example/translation[not(@type)]/bogusUnmarked");
			AssertXPathNotNull(
					"//entry/sense/example/translation[@type='free']/form/text[text()='freestuff']");
		}

		[Test]
		public void Variant()
		{
			LexEntry e = MakeSimpleEntry();
			string xml1 =
					@"
				 <variant>
					<trait name='dialects' value='Ratburi'/>
					<form lang='und-fonipa'><text>flub</text></form>
				  </variant>";
			String xml2 =
					@"
				 <variant ref='2'>
					<form lang='und-fonipa'><text>glob</text></form>
				  </variant>";

			_merger.MergeInVariant(e, MakeBasicLiftMultiText(), xml1);
			_merger.MergeInVariant(e, MakeBasicLiftMultiText(), xml2);
			_merger.FinishEntry(e);
			_exporter.Add(e);
			_exporter.End();
			AssertXPathNotNull("//entry/variant/trait[@name='dialects' and @ value='Ratburi']");
			AssertXPathNotNull("//entry/variant[@ref='2']/form/text[text()='glob']");
		}

		[Test]
		public void Etymology()
		{
			LexEntry e = MakeSimpleEntry();
			string xml =
					@"<etymology type='proto'>
					<form lang='x-proto-ind'><text>apuR</text></form>
					<gloss>
						 <form lang='eng'><text>lime, chalk</text></form>
					</gloss>
				  </etymology>";

			_merger.MergeInEtymology(e, null, null, null, xml);
			_merger.FinishEntry(e);

			_exporter.Add(e);
			_exporter.End();
			AssertXPathNotNull("//entry/etymology[@type='proto']/form/text");
			AssertXPathNotNull("//entry/etymology[@type='proto']/gloss/form/text");
		}

		[Test]
		public void Reversal_Complex()
		{
			LexEntry e = MakeSimpleEntry();
			string xml =
					@"  <entry id='utan'>
				<sense id='utan_'>
				  <grammatical-info value='n'/>
				  <reversal type='eng'>
					<form lang='en'>
					  <text>mushroom</text>
					</form>
					<main>
					  <form lang='en'>
						<text>vegetable</text>
					  </form>
					</main>
				  </reversal>
				</sense>
			  </entry>";

			LexSense sense = new LexSense();
			e.Senses.Add(sense);
			_merger.MergeInReversal(sense, null, null, null, xml);
			_merger.FinishEntry(e);

			_exporter.Add(e);
			_exporter.End();
			AssertXPathNotNull("//entry/sense/reversal/main/form/text[text()='vegetable']");
			AssertXPathNotNull("//entry/sense/reversal/form/text[text()='mushroom']");
		}

		[Test]
		public void Pronunciation_Complex()
		{
			LexEntry e = MakeSimpleEntry();
			string xml =
					@"  <pronunciation>
								  <form lang='v'>
									<text>pronounceme</text>
								  </form>
								  <media href='blah.mp3'>
									<form lang='v'>
									  <text>lable for the media</text>
									</form>
								  </media>
								  <field type='cvPattern'>
									<form lang='en'>
									  <text>acvpattern</text>
									</form>
								  </field>
								  <field type='tone'>
									<form lang='en'>
									  <text>atone</text>
									</form>
								  </field>
								</pronunciation>";
			_merger.MergeInPronunciation(e, MakeBasicLiftMultiText(), xml);
			_merger.FinishEntry(e);

			_exporter.Add(e);
			_exporter.End();
			AssertXPathNotNull("//entry/pronunciation/form[@lang='v']/text[text()='pronounceme']");
			AssertXPathNotNull("//entry/pronunciation/media[@href='blah.mp3']");
			AssertXPathNotNull(
					"//entry/pronunciation/field[@type='cvPattern']/form/text[text()='acvpattern']");
			AssertXPathNotNull("//entry/pronunciation/field[@type='tone']/form/text");
		}
	}
}