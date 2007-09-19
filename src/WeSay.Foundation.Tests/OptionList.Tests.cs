using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using Exortech.NetReflector;
using NUnit.Framework;
using WeSay.Foundation;

namespace WeSay.Language.Tests
{
    [TestFixture]
    public class OptionListTests
    {
        [SetUp]
        public void Setup()
        {

        }

        [TearDown]
        public void TearDown()
        {

        }
        [Test]
        public void LoadFromFile()
        {
            string path = Path.GetTempFileName();
            File.WriteAllText(path,
                @"<?xml version='1.0' encoding='utf-8'?><optionsList>
		                <option>
                            <key>verb</key>
			                <name>
				                <form ws='en'>verb</form>
				                <form ws='xy'>xyverb</form>
			                </name>
		                </option>
		                <option>
			                <key>noun</key>
			                <name>
				                <form ws='en'>noun</form>
				                <form ws='fr'>nom</form>
				                <form ws='es'>nombre</form>
				                <form ws='th'>นาม</form>
			                </name>
		                </option>
                 </optionsList>");

            OptionsList list = OptionsList.LoadFromFile(path);
            File.Delete(path);
            Assert.AreEqual("verb", list.Options[0].Key);
            Assert.AreEqual("verb", list.Options[0].Name.GetBestAlternative("en"));
            Assert.AreEqual("xyverb", list.Options[0].Name.GetBestAlternative("xy"));
            Assert.AreEqual("noun", list.Options[1].Name.GetBestAlternative("en"));
            
        }

        [Test]
        public void DeserializeWithEmptyOption()
        {
            string path = Path.GetTempFileName();
            File.WriteAllText(path,
                @"<?xml version='1.0' encoding='utf-8'?><optionsList>
		                 <option>
                                <key>abc</key>
                                <name />
                                <abbreviation />
                                <description />
                              </option>
                 </optionsList>");

            OptionsList list = OptionsList.LoadFromFile(path);
            File.Delete(path);
            Assert.IsNotNull(list.Options[0].Name.Forms);
            Assert.AreEqual(string.Empty, list.Options[0].Name.GetBestAlternative("z"));
        }
        
        [Test]
        public void LoadFromOldFormatFile()
        {
            string path = Path.GetTempFileName();
            File.WriteAllText(path,
                @"<?xml version='1.0' encoding='utf-8'?><optionsList>
		                <options>
<option>
                            <key>verb</key>
			                <name>
				                <form ws='en'>verb</form>
				                <form ws='xy'>xyverb</form>
			                </name>
		                </option>
		                <option>
			                <key>noun</key>
			                <name>
				                <form ws='en'>noun</form>
				                <form ws='fr'>nom</form>
				                <form ws='es'>nombre</form>
				                <form ws='th'>นาม</form>
			                </name>
		                </option>
</options>
                 </optionsList>");

            OptionsList list = OptionsList.LoadFromFile(path);
            File.Delete(path);
            Assert.AreEqual("verb", list.Options[0].Key);
            Assert.AreEqual("verb", list.Options[0].Name.GetBestAlternative("en"));
            Assert.AreEqual("xyverb", list.Options[0].Name.GetBestAlternative("xy"));
            Assert.AreEqual("noun", list.Options[1].Name.GetBestAlternative("en"));

        }

//        [Test]
//        public void DeSerialize()
//        {
//            NetReflectorTypeTable t = new NetReflectorTypeTable();
//            t.Add(typeof(OptionsList));
//            t.Add(typeof(Option));
//            t.Add(typeof(MultiText));
//            t.Add(typeof(LanguageForm));
//
//            NetReflectorReader r = new NetReflectorReader(t);
//
//            OptionsList list = (OptionsList) r.Read(
//                @"<optionsList>
//	                <options>
//		                <option>
//                            <key>verb</key>
//			                <name>
//				                <form ws='en'><text>verb</text></form>
//			                </name>
//		                </option>
//	                </options>
//                </optionsList>");
//
//            Assert.AreEqual("verb", list.Options[0].Name.GetBestAlternative("en"));
//            
//        }

//        [Test]
//        public void SaveToFile()
//        {
//            StringWriter writer = new System.IO.StringWriter();
//            XmlAttributeOverrides overrides = new XmlAttributeOverrides();
//            XmlAttributes ignoreAttr = new XmlAttributes();
//            ignoreAttr.XmlIgnore = true;
//            overrides.Add(typeof(Annotatable), "IsStarred", ignoreAttr);
//
//
//            System.Xml.Serialization.XmlSerializer serializer = new XmlSerializer(typeof (OptionsList), overrides);
//            
//            Option x = new Option();
//            x.Abbreviation.SetAlternative("a", "aabrev");
//            x.Abbreviation.SetAlternative("b", "babrev");
//            x.Key = "akey";
//            x.Name.SetAlternative("a", "aname");
//            x.Name.SetAlternative("b", "bname");
//
//            Option y = new Option();
//            y.Abbreviation.SetAlternative("a", "aabrev");
//            y.Abbreviation.SetAlternative("b", "babrev");
//            y.Key = "akey";
//            y.Name.SetAlternative("a", "aname");
//            y.Name.SetAlternative("b", "bname");
//
//            OptionsList list = new OptionsList();
//            list.Options.Add(x);
//            list.Options.Add(y);
//
//
//            serializer.Serialize(writer, list);
//            string xml = writer.GetStringBuilder().ToString();
//            Debug.WriteLine(xml);
//            Assert.AreEqual("", xml);
//        }

        [Test]
        public void SaveToFile()
        {
            Option x = new Option();
            x.Abbreviation.SetAlternative("a", "aabrev");
            x.Abbreviation.SetAlternative("b", "babrev");
            x.Key = "akey";
            x.Name.SetAlternative("a", "aname");
            x.Name.SetAlternative("b", "bname");

            Option y = new Option();
            y.Abbreviation.SetAlternative("a", "aabrev");
            y.Abbreviation.SetAlternative("b", "babrev");
            y.Key = "akey";
            y.Name.SetAlternative("a", "aname");
            y.Name.SetAlternative("b", "bname");

            OptionsList list = new OptionsList();
            list.Options.Add(x);
            list.Options.Add(y);

            string path = Path.GetTempFileName();
            list.SaveToFile(path);

           // Debug.WriteLine(xml);
            OptionsList resultList = OptionsList.LoadFromFile(path);

            Assert.AreEqual("aname", list.Options[1].Name.GetBestAlternative("a"));
        }
    }

}