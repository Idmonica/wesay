using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using LiftIO.Validation;
using NUnit.Framework;
using WeSay.Data;
using WeSay.Foundation;
using WeSay.Foundation.Options;
using WeSay.Language;
using WeSay.LexicalModel;
using WeSay.LexicalModel.Db4o_Specific;
using WeSay.Project;

namespace WeSay.Project.Tests
{
	[TestFixture]
	public class LiftExportTests
	{
		private LiftExporter _exporter;
		private StringBuilder _stringBuilder;
		private Dictionary<string, string> _fieldToOptionListName;
		private string _filePath;
		private LexEntryRepository _lexEntryRepository;

		[SetUp]
		public void Setup()
		{
			Db4oLexModelHelper.InitializeForNonDbTests();
			WeSayWordsProject.InitializeForTests();
			_filePath = Path.GetTempFileName();
			_lexEntryRepository = new LexEntryRepository(_filePath);
			_fieldToOptionListName = new Dictionary<string, string>();
			_stringBuilder = new StringBuilder();
			PrepWriterForFragment();
		}

		private void PrepWriterForFragment()
		{
			_exporter = new LiftExporter(/*_fieldToOptionListName,*/ _stringBuilder, true, _lexEntryRepository);
		}

		private void PrepWriterForFullDocument()
		{
			_exporter = new LiftExporter(/*_fieldToOptionListName,*/ _stringBuilder, false, _lexEntryRepository);
		}


		[TearDown]
		public void TearDown()
		{
			_lexEntryRepository.Dispose();
			File.Delete(_filePath);
		}

		[Test]
		public void DocumentStart()
		{
			PrepWriterForFullDocument();
			//NOTE: the utf-16 here is an artifact of the xmlwriter when writing to a stringbuilder,
			//which is what we use for tests.  The file version puts out utf-8
			//CheckAnswer("<?xml version=\"1.0\" encoding=\"utf-16\"?><lift producer=\"WeSay.1Pt0Alpha\"/>");// xmlns:flex=\"http://fieldworks.sil.org\" />");
			_exporter.End();
			AssertXPathNotNull(string.Format("lift[@version='{0}']", Validator.LiftVersion));
			AssertXPathNotNull(string.Format("lift[@producer='{0}']", LiftExporter.ProducerString));
		}


		[Test]
		public void AddUsingWholeList_TwoEntries_HasTwoEntries()
		{
			PrepWriterForFullDocument();
			WriteTwoEntries();
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(_stringBuilder.ToString());
			Assert.AreEqual(2, doc.SelectNodes("lift/entry").Count);
		}


		private void MakeTestLexEntry(string lexicalForm)
		{
			LexEntry entry = _lexEntryRepository.CreateItem();
			entry.LexicalForm["test"] = lexicalForm;
			_lexEntryRepository.SaveItem(entry);
		}

		[Test]
		public void WriteToFile()
		{
			string filePath = Path.GetTempFileName();
			try
			{
				_exporter = new LiftExporter(/*_fieldToOptionListName,*/ filePath, _lexEntryRepository);
				WriteTwoEntries();
				XmlDocument doc = new XmlDocument();
				doc.Load(filePath);
				Assert.AreEqual(2, doc.SelectNodes("lift/entry").Count);
			}
			finally
			{
				File.Delete(filePath);
			}
		}

		private void WriteTwoEntries()
		{
			MakeTestLexEntry("sunset");
			MakeTestLexEntry("flower");

			_exporter.Add(_lexEntryRepository.GetAllEntriesSortedByHeadword(new WritingSystem("test", SystemFonts.DefaultFont)));
			_exporter.End();
		}


		[Test]
		public void MultiText()
		{
			MultiText text = new MultiText();
			text["blue"] = "ocean";
			text ["red"] = "sunset";
			_exporter.Add(null, text);
			CheckAnswer("<form lang=\"blue\"><text>ocean</text></form><form lang=\"red\"><text>sunset</text></form>");
		}


		[Test]
		public void LexemeForm_SingleWritingSystem()
		{
			LexEntry e = new LexEntry();
			e.LexicalForm["xx"] = "foo";
			_exporter.Add(e);
			_exporter.End();

			AssertXPathNotNull("//lexical-unit/form[@lang='xx']");
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
				XmlWriter writer = XmlTextWriter.Create(Console.Out, settings);
				doc.WriteContentTo(writer);
				writer.Flush();
			}
			Assert.IsNotNull(node);
		}

		[Test]
		public void LexSense_becomes_sense()
		{
			LexSense sense = new LexSense();
			_exporter.Add(sense);
			_exporter.End();
			Assert.IsTrue(_stringBuilder.ToString().StartsWith("<sense"));
		}


		[Test]
		public void LexicalUnit()
		{
			LexEntry e = new LexEntry();
			e.LexicalForm.SetAlternative("x", "orange");
			_exporter.Add(e);
			_exporter.End();
			AssertXPathNotNull("entry/lexical-unit/form[@lang='x']/text[text()='orange']");
			AssertXPathNotNull("entry/lexical-unit/form[@lang='x'][not(trait)]");
		}

		[Test]
		public void LexicalUnitWithStarredForm()
		{
			LexEntry e = new LexEntry();
			e.LexicalForm.SetAlternative("x", "orange");
			e.LexicalForm.SetAnnotationOfAlternativeIsStarred("x", true);
			_exporter.Add(e);
			_exporter.End();
			AssertXPathNotNull("entry/lexical-unit/form[@lang='x']/annotation[@name='flag' and @value='1']");
		}

		[Test]
		public void Citation()
		{
			LexEntry entry = new LexEntry();
			MultiText citation = entry.GetOrCreateProperty<MultiText>(LexEntry.WellKnownProperties.Citation);
			citation["zz"] = "orange";
			_exporter.Add(entry);
			_exporter.End();
			AssertXPathNotNull("entry/citation/form[@lang='zz']/text[text()='orange']");
			AssertXPathNotNull("entry/citation/form[@lang='zz'][not(trait)]");
			AssertXPathNotNull("entry[not(field)]");
		}


		[Test]
		public void CitationWithStarredForm()
		{
			LexEntry e = new LexEntry();
			MultiText citation = e.GetOrCreateProperty<MultiText>(LexEntry.WellKnownProperties.Citation);

			citation.SetAlternative("x", "orange");
			citation.SetAnnotationOfAlternativeIsStarred("x", true);
			_exporter.Add(e);
			_exporter.End();
			AssertXPathNotNull("entry/citation/form[@lang='x']/annotation[@name='flag' and @value='1']");
		}

		[Test]
		public void Gloss()
		{
			LexSense sense = new LexSense();
			sense.Gloss["blue"] = "ocean";
			_exporter.Add(sense);
			_exporter.End();
			AssertXPathNotNull("sense/gloss[@lang='blue']/text[text()='ocean']");
		}

		[Test]
		public void GlossWithStarredForm()
		{
			LexSense sense = new LexSense();
			sense.Gloss.SetAlternative("x", "orange");
			sense.Gloss.SetAnnotationOfAlternativeIsStarred("x", true);
			_exporter.Add(sense);
			_exporter.End();
			AssertXPathNotNull("sense/gloss[@lang='x']/annotation[@name='flag' and @value='1']");
		}

		[Test]
		public void Gloss_MultipleGlossesSplitIntoSeparateEntries()
		{
			LexSense sense = new LexSense();
			sense.Gloss["a"] = "aaa; bbb; ccc";
			sense.Gloss["x"] = "xx";
			_exporter.Add(sense);
			_exporter.End();
			AssertXPathNotNull("sense[count(gloss)=4]");
			AssertXPathNotNull("sense/gloss[@lang='a' and text='aaa']");
			AssertXPathNotNull("sense/gloss[@lang='a' and text='bbb']");
			AssertXPathNotNull("sense/gloss[@lang='a' and text='ccc']");
			AssertXPathNotNull("sense/gloss[@lang='x' and text='xx']");
		}

		[Test]
		public void Grammi()
		{
			LexSense sense = new LexSense();
			OptionRef o = sense.GetOrCreateProperty<OptionRef>(LexSense.WellKnownProperties.PartOfSpeech);
			o.Value = "orange";
			_exporter.Add(sense);
			_exporter.End();
			AssertXPathNotNull("sense/grammatical-info[@value='orange']");
			AssertXPathNotNull("sense[not(trait)]");
		}

		[Test]
		public void BlankGrammi()
		{
			LexSense sense = new LexSense();
			OptionRef o = sense.GetOrCreateProperty<OptionRef>(LexSense.WellKnownProperties.PartOfSpeech);
			o.Value = string.Empty;
			_exporter.Add(sense);
			_exporter.End();
			AssertXPathNotNull("sense[not(grammatical-info)]");
			AssertXPathNotNull("sense[not(trait)]");
		}


		[Test]
		public void GrammiWithStarredForm()
		{
			LexSense sense = new LexSense();
			OptionRef o = sense.GetOrCreateProperty<OptionRef>(LexSense.WellKnownProperties.PartOfSpeech);
			o.Value = "orange";
			o.IsStarred = true;
			_exporter.Add(sense);
			_exporter.End();
			AssertXPathNotNull("sense/grammatical-info[@value='orange']/annotation[@name='flag' and @value='1']");
		}


		[Test]
		public void GlossWithProblematicCharacters()
		{
			LexSense sense = new LexSense();
			sense.Gloss["blue"] = "LessThan<GreaterThan>Ampersan&";
			_exporter.Add(sense);
			CheckAnswer(GetSenseElement(sense)+"<gloss lang=\"blue\"><text>LessThan&lt;GreaterThan&gt;Ampersan&amp;</text></gloss></sense>");
		}
		private static string GetSenseElement(LexSense sense)
		{
			return  string.Format("<sense id=\"{0}\">", sense.GetOrCreateId());
		}

		[Test]
		public void AttributesWithProblematicCharacters()
		{
			LexSense sense = new LexSense();
			sense.Gloss["x\"y"] = "test";
			_exporter.Add(sense);
			CheckAnswer(GetSenseElement(sense)+"<gloss lang=\"x&quot;y\"><text>test</text></gloss></sense>");
		}

		[Test]
		public void BlankMultiText()
		{
			_exporter.Add(null, new MultiText());
			CheckAnswer("");
		}

		[Test]
		public void BlankExample()
		{
			_exporter.Add(new LexExampleSentence());
			CheckAnswer("<example />");
		}
		[Test]
		public void ExampleSourceAsAttribute()
		{
			LexExampleSentence ex = new LexExampleSentence();
			OptionRef z = ex.GetOrCreateProperty<OptionRef>(LexExampleSentence.WellKnownProperties.Source);
			z.Value = "hearsay";

			_exporter.Add(ex);
			_exporter.End();
			AssertXPathNotNull("example[@source='hearsay']");
		}


		[Test]
		public void EmptyExampleSource_NoAttribute()
		{
			LexExampleSentence ex = new LexExampleSentence();
			OptionRef z = ex.GetOrCreateProperty<OptionRef>(LexExampleSentence.WellKnownProperties.Source);
			_exporter.Add(ex);
			_exporter.End();
			AssertXPathNotNull("example[not(@source)]");
		}

		[Test]
		public void ExampleSentence()
		{
			LexExampleSentence example = new LexExampleSentence();
			example.Sentence["blue"] = "ocean's eleven";
			example.Sentence["red"] = "red sunset tonight";
			_exporter.Add(example);
			CheckAnswer("<example><form lang=\"blue\"><text>ocean's eleven</text></form><form lang=\"red\"><text>red sunset tonight</text></form></example>");
		}

		[Test]
		public void ExampleSentenceWithTranslation()
		{
			LexExampleSentence example = new LexExampleSentence();
			example.Sentence["blue"] = "ocean's eleven";
			example.Sentence["red"] = "red sunset tonight";
			example.Translation["green"] = "blah blah";
			_exporter.Add(example);
			CheckAnswer("<example><form lang=\"blue\"><text>ocean's eleven</text></form><form lang=\"red\"><text>red sunset tonight</text></form><translation><form lang=\"green\"><text>blah blah</text></form></translation></example>");
		}

		[Test]
		public void BlankSense()
		{
			LexSense sense = new LexSense();
			_exporter.Add(sense);
			CheckAnswer(string.Format("<sense id=\"{0}\" />", sense.GetOrCreateId()));
		}

		[Test]
		public void EntryGuid()
		{
			LexEntry entry = new LexEntry();
			_exporter.Add(entry);
			ShouldContain(string.Format("guid=\"{0}\"", entry.Guid));
		}

		[Test]
		public void LexEntry_becomes_entry()
		{
			LexEntry entry = new LexEntry();
			_exporter.Add(entry);
			_exporter.End();
			Assert.IsTrue(_stringBuilder.ToString().StartsWith("<entry"));
		}

		[Test]
		public void DeletedEntry()
		{
			LexEntry entry = new LexEntry();
			_exporter.AddDeletedEntry(entry);
			_exporter.End();
			Assert.IsNotNull(GetStringAttributeOfTopElement("dateDeleted"));
		}

		private string GetStringAttributeOfTopElement(string attribute)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(_stringBuilder.ToString());
			return doc.FirstChild.Attributes[attribute].ToString();
		}

		private void ShouldContain(string s)
		{
			_exporter.End();
			Assert.IsTrue(_stringBuilder.ToString().Contains(s), "\n'{0}' is not contained in\n'{1}'", s, _stringBuilder.ToString());
		}

		[Test]
		public void EntryHasDateCreated()
		{
			LexEntry entry = new LexEntry();
			_exporter.Add(entry);
			_exporter.End();
			ShouldContain(string.Format("dateCreated=\"{0}\"", entry.CreationTime.ToString("yyyy-MM-ddThh:mm:ssZ")));
		}

		[Test]
		public void EntryHasDateModified()
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm["test"] = "lexicalForm"; // make dateModified different than dateCreated
			_exporter.Add(entry);
			_exporter.End();
			ShouldContain(string.Format("dateModified=\"{0}\"", entry.ModificationTime.ToString("yyyy-MM-ddThh:mm:ssZ")));
		}

		[Test]
		public void Entry_HasId_RemembersId()
		{
			LexEntry entry = new LexEntry("my id", Guid.NewGuid(), 0);
			_exporter.Add(entry);
			_exporter.End();
			ShouldContain("id=\"my id\"");
		}

		[Test]
		public void Sense_HasId_RemembersId()
		{
			LexSense s = new LexSense();
			 s.Id = "my id";
			_exporter.Add(s);
			_exporter.End();
			ShouldContain("id=\"my id\"");
		}

		[Test]
		public void Entry_EntryHasIdWithInvalidXMLCharacters_CharactersEscaped()
		{
			// technically the only invalid characters in an attribute are & < and " (when surrounded by ")
			LexEntry entry = new LexEntry("<>&\"\'", Guid.NewGuid(), 0);
			_exporter.Add(entry);
			_exporter.End();
			ShouldContain("id=\"&lt;&gt;&amp;&quot;'\"");
		}


		[Test]
		public void Entry_NoId_GetsHumanReadableId()
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm["test"] = "lexicalForm"; // make dateModified different than dateCreated
			_exporter.Add(entry);
			_exporter.End();
			ShouldContain(string.Format("id=\"{0}\"", LiftExporter.GetHumanReadableId(entry, new Dictionary<string, int>())));
		}


		[Test]
		public void Sense_NoId_GetsId()
		{
			LexSense sense = new LexSense();
			_exporter.Add(sense);
			_exporter.End();
			ShouldContain(string.Format("id=\"{0}\"", sense.Id));
		}

		/* this is not relevant, as we are currently using form_guid as the id
		[Test]
		public void DuplicateFormsGetHomographNumbers()
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm["blue"] = "ocean";
			_exporter.Add(entry);
			_exporter.Add(entry);
			_exporter.Add(entry);
		  _exporter.End();
		  Assert.IsTrue(_stringBuilder.ToString().Contains("\"ocean\""), "ocean not contained in {0}", _stringBuilder.ToString());
		  Assert.IsTrue(_stringBuilder.ToString().Contains("ocean_2"), "ocean_2 not contained in {0}", _stringBuilder.ToString());
		  Assert.IsTrue(_stringBuilder.ToString().Contains("ocean_3"), "ocean_3 not contained in {0}", _stringBuilder.ToString());
		}
		*/

		[Test]
		public void GetHumanReadableId_EntryHasId_GivesId()
		{
			LexEntry entry = new LexEntry("my id", Guid.NewGuid(), 0);
			Assert.AreEqual("my id", LiftExporter.GetHumanReadableId(entry, new Dictionary<string, int>()));
		}

		/* this tests a particular implementation detail (idCounts), which isn't used anymore:
		[Test]
		public void GetHumanReadableId_EntryHasId_RegistersId()
		{
			LexEntry entry = new LexEntry("my id", Guid.NewGuid());
			Dictionary<string, int> idCounts = new Dictionary<string, int>();
			LiftExporter.GetHumanReadableId(entry, idCounts);
			Assert.AreEqual(1, idCounts["my id"]);
		}
		*/

		/* this is not relevant, as we are currently using form_guid as the id
[Test]
		public void GetHumanReadableId_EntryHasAlreadyUsedId_GivesIncrementedId()
		{
			LexEntry entry = new LexEntry("my id", Guid.NewGuid());
			Dictionary<string, int> idCounts = new Dictionary<string, int>();
			LiftExporter.GetHumanReadableId(entry, idCounts);
			Assert.AreEqual("my id_2", LiftExporter.GetHumanReadableId(entry, idCounts));
		}
*/
		/* this is not relevant, as we are currently using form_guid as the id
	  [Test]
		public void GetHumanReadableId_EntryHasAlreadyUsedId_IncrementsIdCount()
		{
			LexEntry entry = new LexEntry("my id", Guid.NewGuid());
			Dictionary<string, int> idCounts = new Dictionary<string, int>();
			LiftExporter.GetHumanReadableId(entry, idCounts);
			LiftExporter.GetHumanReadableId(entry, idCounts);
			Assert.AreEqual(2, idCounts["my id"]);
		}
*/

		/* this is not relevant, as we are currently using form_guid as the id
	  [Test]
		public void GetHumanReadableId_EntryHasNoIdAndNoLexicalForms_GivesDefaultId()
		{
			LexEntry entry = new LexEntry();
			Assert.AreEqual("NoForm", LiftExporter.GetHumanReadableId(entry, new Dictionary<string, int>()));
		}
		*/

/*      this is not currently relevant, as we are now using form_guid as the id
		[Test]
		public void GetHumanReadableId_EntryHasNoIdAndNoLexicalFormsButAlreadyUsedId_GivesIncrementedDefaultId()
		{
			LexEntry entry = new LexEntry();
			Dictionary<string, int> idCounts = new Dictionary<string, int>();
			LiftExporter.GetHumanReadableId(entry, idCounts);
			Assert.AreEqual("NoForm_2", LiftExporter.GetHumanReadableId(entry, idCounts));
		}
*/

		/*      this is not currently relevant, as we are now using form_guid as the id
		[Test]
		public void GetHumanReadableId_EntryHasNoId_GivesIdMadeFromFirstLexicalForm()
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm["green"] = "grass";
			entry.LexicalForm["blue"] = "ocean";

			Assert.AreEqual("grass", LiftExporter.GetHumanReadableId(entry, new Dictionary<string, int>()));
		}
		*/

		/*/*      this is not currently relevant, as we are now using form_guid as the id

		[Test]
		public void GetHumanReadableId_EntryHasNoId_RegistersIdMadeFromFirstLexicalForm()
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm["green"] = "grass";
			entry.LexicalForm["blue"] = "ocean";
			Dictionary<string, int> idCounts = new Dictionary<string, int>();
			LiftExporter.GetHumanReadableId(entry, idCounts);
			Assert.AreEqual(1, idCounts["grass"]);
		}
*/
		/*      this is not currently relevant, as we are now using form_guid as the id
	  [Test]
		public void GetHumanReadableId_EntryHasNoIdAndIsSameAsAlreadyEncountered_GivesIncrementedId()
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm["green"] = "grass";
			entry.LexicalForm["blue"] = "ocean";
			Dictionary<string, int> idCounts = new Dictionary<string, int>();
			LiftExporter.GetHumanReadableId(entry, idCounts);
			Assert.AreEqual("grass_2", LiftExporter.GetHumanReadableId(entry, idCounts));
		}
*/
		/*      this is not currently relevant, as we are now using form_guid as the id

		[Test]
		public void GetHumanReadableId_EntryHasNoIdAndIsSameAsAlreadyEncountered_IncrementsIdCount()
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm["green"] = "grass";
			entry.LexicalForm["blue"] = "ocean";
			Dictionary<string, int> idCounts = new Dictionary<string, int>();
			LiftExporter.GetHumanReadableId(entry, idCounts);
			LiftExporter.GetHumanReadableId(entry, idCounts);
			Assert.AreEqual(2, idCounts["grass"]);
		}
*/
		/*      this is not currently relevant, as we are now using form_guid as the id
	  [Test]
		public void GetHumanReadableId_IdsDifferByWhiteSpaceTypeOnly_WhitespaceTreatedAsSpaces()
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm["green"] = "string\t1\n2\r3 4";
			Assert.AreEqual("string 1 2 3 4", LiftExporter.GetHumanReadableId(entry, new Dictionary<string, int>()));
		}
*/
		[Test]
		public void GetHumanReadableId_IdIsSpace_NoForm()
		{
			LexEntry entry = new LexEntry(" ",Guid.NewGuid(), 0);
			Assert.IsTrue(LiftExporter.GetHumanReadableId(entry, new Dictionary<string, int>()).StartsWith("Id'dPrematurely_"));
		}

		[Test]
		public void GetHumanReadableId_IdIsSpace_TreatedAsThoughNonExistentId()
		{
			LexEntry entry = new LexEntry(" ", Guid.NewGuid(), 0);
			entry.LexicalForm["green"] = "string";
			Assert.IsTrue(LiftExporter.GetHumanReadableId(entry, new Dictionary<string, int>()).StartsWith("string"));
		}

		[Test]
		public void EntryWithSenses()
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm["blue"] = "ocean";
			LexSense sense1 = new LexSense();
			sense1.Gloss["a"] = "aaa";
			entry.Senses.Add(sense1);
			LexSense sense2 = new LexSense();
			sense2.Gloss["b"] = "bbb";
			entry.Senses.Add(sense2);
			_exporter.Add(entry);

			ShouldContain(string.Format(GetSenseElement(sense1)+"<gloss lang=\"a\"><text>aaa</text></gloss></sense>"+
				GetSenseElement(sense2)+"<gloss lang=\"b\"><text>bbb</text></gloss></sense></entry>"));
			AssertXPathNotNull("entry[count(sense)=2]");
		}

		[Test]
		public void SensesAreLastObjectsInEntry() // this helps conversions to sfm
		{
			LexEntry entry = new LexEntry();
			entry.LexicalForm["blue"] = "ocean";

			LexSense sense1 = new LexSense();
			sense1.Gloss["a"] = "aaa";
			entry.Senses.Add(sense1);
			LexSense sense2 = new LexSense();
			sense2.Gloss["b"] = "bbb";
			entry.Senses.Add(sense2);

			MultiText citation = entry.GetOrCreateProperty<MultiText>(LexEntry.WellKnownProperties.Citation);
			citation["zz"] = "orange";

			MultiText note = entry.GetOrCreateProperty<MultiText>(LexEntry.WellKnownProperties.Note);
			note["zz"] = "orange";

			MultiText field = entry.GetOrCreateProperty<MultiText>("custom");
			field["zz"] = "orange";

			_exporter.Add(entry);

			ShouldContain(string.Format(GetSenseElement(sense1) + "<gloss lang=\"a\"><text>aaa</text></gloss></sense>" +
				GetSenseElement(sense2) + "<gloss lang=\"b\"><text>bbb</text></gloss></sense></entry>"));
		}


		[Test]
		public void NoteOnEntry_OutputAsNote()
		{
			LexEntry entry = new LexEntry();
			MultiText m = entry.GetOrCreateProperty<MultiText>(LexEntry.WellKnownProperties.Note);
			m["zz"] = "orange";
			_exporter.Add(entry);
			_exporter.End();
			AssertXPathNotNull("entry/note/form[@lang='zz' and text='orange']");
			AssertXPathNotNull("entry[not(field)]");
		}

		[Test]
		public void EmptyNoteOnEntry_NoOutput()
		{
			LexEntry entry = new LexEntry();
			MultiText m = entry.GetOrCreateProperty<MultiText>(LexEntry.WellKnownProperties.Note);
			_exporter.Add(entry);
			_exporter.End();
			AssertXPathNotNull("entry[not(note)]");
			AssertXPathNotNull("entry[not(field)]");
		}


		[Test]
		public void NoteOnSense_OutputAsNote()
		{
			LexSense sense = new LexSense();
			MultiText m = sense.GetOrCreateProperty<MultiText>(LexSense.WellKnownProperties.Note);
			m["zz"] = "orange";
			_exporter.Add(sense);
			_exporter.End();
			AssertXPathNotNull("sense/note/form[@lang='zz' and text='orange']");
			AssertXPathNotNull("sense[not(field)]");
		}

		[Test]
		public void NoteOnExample_OutputAsNote()
		{
			LexExampleSentence example = new LexExampleSentence();
			MultiText m = example.GetOrCreateProperty<MultiText>(LexExampleSentence.WellKnownProperties.Note);
			m["zz"] = "orange";
			_exporter.Add(example);
			_exporter.End();
			AssertXPathNotNull("example/note/form[@lang='zz' and text='orange']");
			AssertXPathNotNull("example[not(field)]");
		}


		[Test]
		public void DefinitionOnSense_OutputAsDefinition()
		{
			LexSense sense = new LexSense();
			MultiText m = sense.GetOrCreateProperty<MultiText>(LexSense.WellKnownProperties.Definition);
			m["zz"] = "orange";
			_exporter.Add(sense);
			_exporter.End();
			AssertXPathNotNull("sense/definition/form[@lang='zz']/text[text()='orange']");
			AssertXPathNotNull("sense[not(field)]");
		}

		[Test]
		public void EmptyDefinitionOnSense_NotOutput()
		{
			LexSense sense = new LexSense();
			sense.GetOrCreateProperty<MultiText>(LexSense.WellKnownProperties.Definition);
			_exporter.Add(sense);
			_exporter.End();
			AssertXPathNotNull("sense[not(definition)]");
			AssertXPathNotNull("sense[not(field)]");
		}

		[Test]
		public void EmptyCustomMultiText()
		{
			LexSense sense = new LexSense();
			sense.GetOrCreateProperty<MultiText>("flubadub");
			_exporter.Add(sense);
			_exporter.End();
			AssertXPathNotNull("sense[not(field)]");
		}


		[Test]
		public void CustomMultiTextOnSense()
		{
			LexSense sense = new LexSense();
			MultiText m = sense.GetOrCreateProperty<MultiText>("flubadub");
			m["zz"] = "orange";
			_exporter.Add(sense);
			_exporter.End();
			AssertXPathNotNull("sense/field[@type='flubadub']/form[@lang='zz' and text='orange']");
		}

		[Test]
		public void CustomMultiTextOnEntry()
		{
			LexEntry entry = new LexEntry();
			MultiText m = entry.GetOrCreateProperty<MultiText>("flubadub");
			m["zz"] = "orange";
			_exporter.Add(entry);
			_exporter.End();
			AssertXPathNotNull("entry/field[@type='flubadub']/form[@lang='zz' and text='orange']");
		}

		[Test]
		public void CustomMultiTextOnExample()
		{
			LexExampleSentence example = new LexExampleSentence();
			MultiText m = example.GetOrCreateProperty<MultiText>("flubadub");
			m["zz"] = "orange";
			_exporter.Add(example);
			_exporter.End();
			AssertXPathNotNull("example/field[@type='flubadub']/form[@lang='zz' and text='orange']");
		}

		[Test]
		public void EmptyCustomOptionRef()
		{
			LexSense sense = new LexSense();
			sense.GetOrCreateProperty<OptionRef>("flubadub");
			_exporter.Add(sense);
			_exporter.End();
			AssertXPathNotNull("sense[not(trait)]");
		}

		[Test]
		public void CustomOptionRefOnEntry()
		{
			_fieldToOptionListName.Add("flub", "kindsOfFlubs");
			LexEntry entry = new LexEntry();
			OptionRef o = entry.GetOrCreateProperty<OptionRef>("flub");
			o.Value = "orange";
			_exporter.Add(entry);
			_exporter.End();
			AssertXPathNotNull("entry/trait[@name='flub' and @value='orange']");
		}
		[Test]
		public void CustomOptionRefOnSense()
		{
			_fieldToOptionListName.Add("flub", "kindsOfFlubs");
			LexSense sense = new LexSense();
			OptionRef o = sense.GetOrCreateProperty<OptionRef>("flub");
			o.Value = "orange";
			_exporter.Add(sense);
			_exporter.End();
			Assert.AreEqual(GetSenseElement(sense)+"<trait name=\"flub\" value=\"orange\" /></sense>", _stringBuilder.ToString());
		}

		[Test]
		public void CustomOptionRefOnSenseWithGrammi()
		{
			_fieldToOptionListName.Add("flub", "kindsOfFlubs");
			LexSense sense = new LexSense();
			OptionRef grammi = sense.GetOrCreateProperty<OptionRef>(LexSense.WellKnownProperties.PartOfSpeech);
			grammi.Value = "verb";

			OptionRef o = sense.GetOrCreateProperty<OptionRef>("flub");
			o.Value = "orange";
			_exporter.Add(sense);
			_exporter.End();
			AssertXPathNotNull("sense/trait[@name='flub' and @value='orange']");
			AssertXPathNotNull("sense[count(trait)=1]");
		}

		[Test]
		public void CustomOptionRefOnExample()
		{
			_fieldToOptionListName.Add("flub", "kindsOfFlubs");
			LexExampleSentence example = new LexExampleSentence();
			OptionRef o = example.GetOrCreateProperty<OptionRef>("flub");
			o.Value = "orange";
			_exporter.Add(example);
			_exporter.End();
			Assert.AreEqual("<example><trait name=\"flub\" value=\"orange\" /></example>", _stringBuilder.ToString());
		}

		[Test]
		public void EmptyCustomOptionRefCollection()
		{
			LexSense sense = new LexSense();
			sense.GetOrCreateProperty<OptionRefCollection>("flubadub");
			_exporter.Add(sense);
			_exporter.End();
			AssertXPathNotNull("sense[not(trait)]");
		}


		[Test]
		public void CustomOptionRefCollectionOnEntry()
		{
			_fieldToOptionListName.Add("flubs", "colors");
			LexEntry entry = new LexEntry();
			OptionRefCollection o = entry.GetOrCreateProperty<OptionRefCollection>("flubs");
			o.AddRange(new string[] { "orange", "blue" });
			_exporter.Add(entry);
			_exporter.End();
			AssertXPathNotNull("entry/trait[@name='flubs' and @value='orange']");
			AssertXPathNotNull("entry/trait[@name='flubs' and @value='blue']");
			AssertXPathNotNull("entry[count(trait) =2]");
		}

		[Test]
		public void CustomOptionRefCollectionOnSense()
		{
			_fieldToOptionListName.Add("flubs", "colors");
			LexSense sense = new LexSense();
			OptionRefCollection o = sense.GetOrCreateProperty<OptionRefCollection>("flubs");
			o.AddRange(new string[] { "orange", "blue" });
			_exporter.Add(sense);
			_exporter.End();
			Assert.AreEqual(GetSenseElement(sense)+"<trait name=\"flubs\" value=\"orange\" /><trait name=\"flubs\" value=\"blue\" /></sense>", _stringBuilder.ToString());
		}

		[Test]
		public void CustomOptionRefCollectionOnExample()
		{
			_fieldToOptionListName.Add("flubs", "colors");
			LexExampleSentence example = new LexExampleSentence();
			OptionRefCollection o = example.GetOrCreateProperty<OptionRefCollection>("flubs");
			o.AddRange(new string[] {"orange", "blue"});
			_exporter.Add(example);
			_exporter.End();
			Assert.AreEqual("<example><trait name=\"flubs\" value=\"orange\" /><trait name=\"flubs\" value=\"blue\" /></example>", _stringBuilder.ToString());
		}

		[Test]
		public void SenseWithExample()
		{
			LexSense sense = new LexSense();
			LexExampleSentence example = new LexExampleSentence();
			example.Sentence["red"] = "red sunset tonight";
			sense.ExampleSentences.Add(example);
			_exporter.Add(sense);
			CheckAnswer(GetSenseElement(sense)+"<example><form lang=\"red\"><text>red sunset tonight</text></form></example></sense>");
		}


		[Test]
		public void SenseWithSynonymRelations()
		{
			LexSense sense = new LexSense();

			LexRelationType synonymRelationType = new LexRelationType("synonym", LexRelationType.Multiplicities.Many, LexRelationType.TargetTypes.Sense);

			LexRelationType antonymRelationType = new LexRelationType("antonym", LexRelationType.Multiplicities.Many, LexRelationType.TargetTypes.Sense);

			LexRelationCollection relations = new LexRelationCollection();
			sense.Properties.Add(new KeyValuePair<string, object>("relations", relations));

			relations.Relations.Add(new LexRelation(synonymRelationType.ID, "one", sense));
			relations.Relations.Add(new LexRelation(synonymRelationType.ID, "two", sense));
			relations.Relations.Add(new LexRelation(antonymRelationType.ID, "bee", sense));

			_exporter.Add(sense);
			CheckAnswer(GetSenseElement(sense)+"<relation type=\"synonym\" ref=\"one\" /><relation type=\"synonym\" ref=\"two\" /><relation type=\"antonym\" ref=\"bee\" /></sense>");
		}

		private void CheckAnswer(string answer)
		{
			_exporter.End();
			Assert.AreEqual(answer, _stringBuilder.ToString());
		}


		[Test]
		public void FlagOnEntry_OutputAsTrait()
		{
			LexEntry entry = new LexEntry();
			entry.SetFlag("ATestFlag");
			_exporter.Add(entry);
			_exporter.End();
			AssertXPathNotNull("entry/trait[@name='ATestFlag' and @value]");
		}

		[Test]
		public void FlagCleared_NoOutput()
		{
			LexEntry entry = new LexEntry();
			entry.SetFlag("ATestFlag");
			entry.ClearFlag("ATestFlag");
			_exporter.Add(entry);
			_exporter.End();
			AssertXPathNotNull("entry[not(trait)]");
		}


		[Test]
		public void Picture_OutputAsPictureURLRef()
		{
			LexSense sense = new LexSense();
			PictureRef p = sense.GetOrCreateProperty<PictureRef>("Picture");
			p.Value = "bird.jpg";
			_exporter.Add(sense);
			_exporter.End();
			CheckAnswer(GetSenseElement(sense)+"<illustration href=\"bird.jpg\" /></sense>");
		}

		[Test]
		public void Picture_OutputAsPictureWithCaption()
		{
			LexSense sense = new LexSense();
			PictureRef p = sense.GetOrCreateProperty<PictureRef>("Picture");
			p.Value = "bird.jpg";
			p.Caption = new MultiText();
			p.Caption["aa"] = "aCaption";
			_exporter.Add(sense);
			_exporter.End();
			CheckAnswer(GetSenseElement(sense)+"<illustration href=\"bird.jpg\"><label><form lang=\"aa\"><text>aCaption</text></form></label></illustration></sense>");
		}

	}
}