using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using LiftIO.Parsing;
using NUnit.Framework;
using Palaso.TestUtilities;
using WeSay.Foundation;
using WeSay.Project;
using System.Linq;
using TemporaryFolder=WeSay.Foundation.Tests.TestHelpers.TemporaryFolder;

namespace WeSay.LexicalModel.Tests
{
	[TestFixture]
	public class LiftRoundTripTests
	{
		private LiftExporter _exporter;
		private StringBuilder _stringBuilder;
		private LexEntryFromLiftBuilder _builder;
		private LiftRepository _repository;
		private string _tempFile;
		private TemporaryFolder _tempFolder;

		[SetUp]
		public void Setup()
		{
			WeSayWordsProject.InitializeForTests();
			_stringBuilder = new StringBuilder();
			_tempFolder = new TemporaryFolder();
			_tempFile = _tempFolder.GetTemporaryFile();
			_exporter = new LiftExporter(_stringBuilder, false);
			_repository = new LiftRepository(_tempFile);
			_builder = new LexEntryFromLiftBuilder(_repository);
		}

		[TearDown]
		public void TearDown()
		{
			_builder.Dispose();
			_repository.Dispose();
			_tempFolder.Delete();
		}

		private LexEntry MakeSimpleEntry()
		{
			Extensible extensibleInfo = new Extensible();
			return _builder.GetOrMakeEntry(extensibleInfo, 0);
		}

		private static LiftMultiText MakeBasicLiftMultiText()
		{
			LiftMultiText forms = new LiftMultiText();
			forms.Add("ws-one", "uno");
			forms.Add("ws-two", "dos");
			return forms;
		}

		private void AssertHasAtLeastOneMatch(string xpath)
		{
			AssertThatXmlIn.String(_stringBuilder.ToString()).
				HasAtLeastOneMatchForXpath(xpath);

//            XmlDocument doc = new XmlDocument();
//            doc.LoadXml(_stringBuilder.ToString());
//            XmlNode node = doc.SelectSingleNode(xpath);
//            if (node == null)
//            {
//                XmlWriterSettings settings = new XmlWriterSettings();
//                settings.Indent = true;
//                settings.ConformanceLevel = ConformanceLevel.Fragment;
//                XmlWriter writer = XmlWriter.Create(Console.Out, settings);
//                doc.WriteContentTo(writer);
//                writer.Flush();
//            }
//            Assert.IsNotNull(node, "Not matched: " + xpath);
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
			_builder.GetOrMakeSubsense(sense, new Extensible(), xml);
			_builder.FinishEntry(e);
			_exporter.Add(e);
			_exporter.End();
			AssertHasAtLeastOneMatch("//entry/sense/subsense[@id='opon_1b' and @order='2']/gloss");
			AssertHasAtLeastOneMatch("//entry/sense/subsense/grammatical-info");
		}

		[Test]
		public void FieldOnEntry_ContentPreserved()
		{
			LexEntry e = MakeSimpleEntry();

			_builder.MergeInField(e,
								 "color",
								 default(DateTime),
								 default(DateTime),
								 MakeBasicLiftMultiText(),
								 null);
			_builder.FinishEntry(e);

			_exporter.Add(e);
			_exporter.End();
			AssertHasAtLeastOneMatch("//entry/field[@type='color']/form[@lang='ws-one']");
			AssertHasAtLeastOneMatch("//entry/field[@type='color']/form[@lang='ws-two']");
		}

		[Test, Ignore("apparently not possible in LIFT?")]
		public void LexicalUnit_HasTrait_TraitRoundTripped()
		{

		}


		[Test, Ignore("Need to wait for LiftIO API on this")]
		public void Note_HasTrait_TraitRoundTripped()
		{
#if notyet
			TestTraitRoundTripped("//entry/note",
								  (e, traits) =>
									  {
										  WeSayDataObject note = _builder.MergeInNote(e, "color",
																		  new LiftMultiText("v", "hello world"), traits);
										 traits.ForEach(t =>_builder.MergeInTrait(note, t));
									  });
#endif
		}

		[Test]
		public void Field_HasTrait_TraitRoundTripped()
		{
			TestTraitRoundTripped("//entry/field",
				(e, traits)=> _builder.MergeInField(e, "color", default(DateTime), default(DateTime), new LiftMultiText("v", "hello world"), traits));

		}

		 public delegate void Proc<A0, A1>(A0 a0, A1 a1);

		private void TestTraitRoundTripped(string xpathToOwningElement, Proc<WeSayDataObject, List<Trait>> p)
		{
			LexEntry e = MakeSimpleEntry();
			List<Trait> traits = new List<Trait>();
			traits.Add(new Trait("one", "1"));
			traits.Add(new Trait("two", "2"));
			p.Invoke(e, traits);
			_builder.FinishEntry(e);
			_exporter.Add(e);
			_exporter.End();
			AssertHasAtLeastOneMatch(xpathToOwningElement + "[trait[@name='one' and @value='1'] and trait[@name='two' and @value='2']]");
		}


		[Test]
		public void Entry_Order()
		{
			Extensible extensibleInfo = new Extensible();
			LexEntry entry4 = this._builder.GetOrMakeEntry(extensibleInfo, 4);
			this._builder.FinishEntry(entry4);
			LexEntry entry1 = this._builder.GetOrMakeEntry(extensibleInfo, 1);
			this._builder.FinishEntry(entry1);
			LexEntry entry2 = this._builder.GetOrMakeEntry(extensibleInfo, 2);
			this._builder.FinishEntry(entry2);
			_exporter.Add(entry4, 3);
			_exporter.Add(entry1, 1);
			_exporter.Add(entry2, 2);

			_exporter.End();
			AssertHasAtLeastOneMatch("//entry[@order='1']");
			AssertHasAtLeastOneMatch("//entry[@order='2']");
			AssertHasAtLeastOneMatch("//entry[@order='3']");
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
			_builder.MergeInTranslationForm(ex, "", translation, "bogus raw xml");
			_builder.FinishEntry(e);
			_exporter.Add(e);
			_exporter.End();
			AssertHasAtLeastOneMatch("//entry/sense/example/translation[not(@type)]/form[@lang='aa']");
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
			_builder.MergeInTranslationForm(ex, "", translation, "bogus raw xml");
			LiftMultiText t2 = new LiftMultiText();
			t2.Add("aa", "type2translation");
			_builder.MergeInTranslationForm(ex,
										   "type2",
										   t2,
										   "<translation type='type2'><bogus/></translation>");
			_builder.FinishEntry(e);

			_exporter.Add(e);
			_exporter.End();
			AssertHasAtLeastOneMatch(
					"//entry/sense/example/translation[not(@type)]/form[@lang='aa']/text[text()='unmarked translation']");
			AssertHasAtLeastOneMatch("//entry/sense/example/translation[@type='type2']/bogus");
		}

		[Test]
		public void ExampleTranslations_UnmarkedFollowedByFree()
		{
			LexEntry e = MakeSimpleEntry();
			LexSense sense = new LexSense();
			e.Senses.Add(sense);
			LexExampleSentence ex = new LexExampleSentence();
			sense.ExampleSentences.Add(ex);

			LiftMultiText translation = new LiftMultiText();
			translation.Add("aa", "unmarked translation");
			_builder.MergeInTranslationForm(ex, "", translation, "bogus raw xml");
			LiftMultiText t2 = new LiftMultiText();
			t2.Add("aa", "freestuff");
			_builder.MergeInTranslationForm(ex,
										   "free",
										   t2,
										   "<translation type='free'><bogus/></translation>");
			_builder.FinishEntry(e);

			_exporter.Add(e);
			_exporter.End();
			AssertHasAtLeastOneMatch(
					"//entry/sense/example/translation[not(@type)]/form[@lang='aa']/text[text()='unmarked translation']");
			AssertHasAtLeastOneMatch("//entry/sense/example/translation[@type='free']/bogus");
		}

		[Test]
		public void ExampleTranslations_FreeFollowedByUnmarked()
		{
			LexEntry e = MakeSimpleEntry();
			LexSense sense = new LexSense();
			e.Senses.Add(sense);
			LexExampleSentence ex = new LexExampleSentence();
			sense.ExampleSentences.Add(ex);

			LiftMultiText t2 = new LiftMultiText();
			t2.Add("aa", "freestuff");
			_builder.MergeInTranslationForm(ex,
										   "Free translation",
										   t2,
										   "<translation type='free'><bogus/></translation>");
			LiftMultiText translation = new LiftMultiText();
			translation.Add("aa", "unmarked translation");
			_builder.MergeInTranslationForm(ex,
										   "",
										   translation,
										   "<translation><bogusUnmarked/></translation>");
			_builder.FinishEntry(e);

			_exporter.Add(e);
			_exporter.End();
			AssertHasAtLeastOneMatch("//entry/sense/example/translation[not(@type)]/bogusUnmarked");
			AssertHasAtLeastOneMatch(
					"//entry/sense/example/translation[@type='Free translation']/form/text[text()='freestuff']");
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

			_builder.MergeInVariant(e, MakeBasicLiftMultiText(), xml1);
			_builder.MergeInVariant(e, MakeBasicLiftMultiText(), xml2);
			_builder.FinishEntry(e);
			_exporter.Add(e);
			_exporter.End();
			AssertHasAtLeastOneMatch("//entry/variant/trait[@name='dialects' and @ value='Ratburi']");
			AssertHasAtLeastOneMatch("//entry/variant[@ref='2']/form/text[text()='glob']");
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

			_builder.MergeInEtymology(e, null, "proto", null, null, xml);
			_builder.FinishEntry(e);

			_exporter.Add(e);
			_exporter.End();
			AssertHasAtLeastOneMatch("//entry/etymology[@type='proto']/form/text");
			AssertHasAtLeastOneMatch("//entry/etymology[@type='proto']/gloss/form/text");
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
			_builder.MergeInReversal(sense, null, null, null, xml);
			_builder.FinishEntry(e);

			_exporter.Add(e);
			_exporter.End();
			AssertHasAtLeastOneMatch("//entry/sense/reversal/main/form/text[text()='vegetable']");
			AssertHasAtLeastOneMatch("//entry/sense/reversal/form/text[text()='mushroom']");
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
			_builder.MergeInPronunciation(e, MakeBasicLiftMultiText(), xml);
			_builder.FinishEntry(e);

			_exporter.Add(e);
			_exporter.End();
			AssertHasAtLeastOneMatch("//entry/pronunciation/form[@lang='v']/text[text()='pronounceme']");
			AssertHasAtLeastOneMatch("//entry/pronunciation/media[@href='blah.mp3']");
			AssertHasAtLeastOneMatch(
					"//entry/pronunciation/field[@type='cvPattern']/form/text[text()='acvpattern']");
			AssertHasAtLeastOneMatch("//entry/pronunciation/field[@type='tone']/form/text");
		}
	}
}