using System.IO;
using NUnit.Framework;
using Palaso.Reporting;
using Palaso.Test;

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

		[Test]
		public void EntryMakeItToPLift()
		{
			var xmlOfEntries = @" <entry id='foo1'>
										<lexical-unit><form lang='v'><text>hello</text></form></lexical-unit>
								 </entry>";
			using (var p = new WeSay.Project.Tests.ProjectDirectorySetupForTesting(xmlOfEntries))
			{
				PLiftMaker maker = new PLiftMaker();
				using (var project = p.CreateLoadedProject())
				{
					var repository = project.GetLexEntryRepository();
					string outputPath = Path.Combine(project.PathToExportDirectory, project.Name + ".xhtml");
					maker.MakePLiftTempFile(outputPath, repository, project.DefaultPrintingTemplate);
					AssertThatXmlIn.File(outputPath).
						HasAtLeastOneMatchForXpath("//field[@type='headword']/form[@lang='v']/text[text()='hello']");
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