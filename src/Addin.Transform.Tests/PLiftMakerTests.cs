using System;
using System.IO;
using NUnit.Framework;
using Palaso.Reporting;
using WeSay.LexicalModel;
using WeSay.Project;

namespace Addin.Transform.Tests
{
	[TestFixture]
	public class PLiftMakerTests
	{
		private string _outputPath;

		[SetUp]
		public void Setup()
		{
			_outputPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
			ErrorReport.IsOkToInteractWithUser = false;
		}

		//        [Test]
		//        public void EntryMakeItToXHtml()
		//        {
		//            string xmlForEntries = @"<entry id='foo1'><lexical-unit><form lang='v'><text>fooOne</text></form></lexical-unit></entry>";
		//
		//            using (Db4oProjectSetupForTesting projectSetup = new Db4oProjectSetupForTesting(xmlForEntries))
		//            {
		//                PLiftMaker maker = new PLiftMaker();
		//                string outputPath = Path.Combine(projectSetup._project.PathToExportDirectory, projectSetup._project.Name + ".xhtml");
		//                maker.MakeXHtmlFile(outputPath, projectSetup._lexEntryRepository, projectSetup._project);
		//                Assert.IsTrue(File.ReadAllText(outputPath).Contains("<span class=\"v\">fooOne"));
		//            }
		//        }

		[Test]
		[Ignore("not a real test")]
		public void MakePLiftForBiatah2()
		{
			using (WeSayWordsProject p = new WeSayWordsProject())
			{
				p.LoadFromProjectDirectoryPath(@"E:\Users\John\Documents\WeSay\biatah");

				using (
						LexEntryRepository lexEntryRepository =
								new LexEntryRepository(p.PathToRepository))
				{
					using (var f = new WeSay.Foundation.Tests.TestHelpers.TempFile())
					{
						PLiftMaker maker = new PLiftMaker();
						maker.MakePLiftTempFile(f.Path, lexEntryRepository, p.DefaultPrintingTemplate);
						Console.WriteLine(f.Path);
					}
				}
			}
		}

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(_outputPath))
			{
				File.Delete(_outputPath);
			}
		}
	}
}