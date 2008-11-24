using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using LiftIO;
using NUnit.Framework;
using Palaso.Reporting;
using WeSay.Foundation;
using WeSay.Foundation.Options;
using WeSay.Foundation.Tests.TestHelpers;
using WeSay.LexicalModel;

namespace WeSay.Project.Tests
{
	[TestFixture]
	public class WeSayWordsProjectTests
	{

		[SetUp]
		public void Setup()
		{
			ErrorReport.IsOkToInteractWithUser = false;

		}

		[TearDown]
		public void TearDown()
		{
		}


		[Test]
		public void UpdateFileStructure_LiftByItself_DoesNothing()
		{
			using (TemporaryFolder f = new TemporaryFolder("OpeningLiftFile_MissingConfigFile_GivesMessage"))
			{
				using(TempLiftFile lift = new TempLiftFile(f, "", "0.12"))
				{
					using(WeSayWordsProject p = new WeSayWordsProject())
					{
						Assert.AreEqual(lift.Path,p.UpdateFileStructure(lift.Path));
					}
				}
			}
		}

		/// <summary>
		/// check  (WS-1004) Exception: Access to the path is denied
		/// </summary>
		[Test]
		public void MakeWritingSystemIdChange_FileLocked_NotifiesUser()
		{
			using (ProjectDirectorySetupForTesting p = new ProjectDirectorySetupForTesting("<entry id='foo1'><lexical-unit><form lang='v'><text>fooOne</text></form></lexical-unit></entry>"))
			{
				WeSayWordsProject project = p.CreateLoadedProject();
				using (File.OpenWrite(p.PathToLiftFile))
				{
					WritingSystem ws = project.WritingSystems["v"];
					ws.Id = "newIdForV";
					using (new Palaso.Reporting.ErrorReport.NonFatalErrorReportExpected())
					{
						Assert.IsFalse(project.MakeWritingSystemIdChange(ws, "v"));
					}
				}
			}
		}

		[Test]
		public void MakeWritingSystemIdChange_WritingSystemFoundInLift_Changed()
		{
			using (ProjectDirectorySetupForTesting p = new ProjectDirectorySetupForTesting("<entry id='foo1'><lexical-unit><form lang='v'><text>fooOne</text></form></lexical-unit></entry>"))
			{
				WeSayWordsProject project = p.CreateLoadedProject();
				XmlDocument doc = new XmlDocument();
				doc.Load(p.PathToLiftFile);
				Assert.IsNotNull(doc.SelectNodes("//form[lang='v']"));
				WritingSystem ws = project.WritingSystems["v"];
				ws.Id = "newIdForV";
				Assert.IsTrue(project.MakeWritingSystemIdChange(ws, "v"));
				doc.Load(p.PathToLiftFile);
				Assert.IsNotNull(doc.SelectNodes("//form[lang='newIdForV']"));
				Assert.AreEqual("newIdForV", ws.Id);

			}
		}
		/// <summary>
		/// related to ws-944: Crash opening lift file from FLEx which was sitting in My Documents without a configuration file
		/// </summary>
		[Test, Ignore("Cannot easily run on vista or linux")]
		public void UpdateFileStructure_LiftByItselfAtRoot_DoesNothing()
		{
			string path = @"C:\unittest.lift"; //this is at the root ON PURPOSE
			File.CreateText(path).Close();
			using (TempFile.TrackExisting(path))
			{
					using (WeSayWordsProject p = new WeSayWordsProject())
					{
						Assert.AreEqual(path, p.UpdateFileStructure(path));
					}
				}
		}

		[Test]
		public void DefaultConfigFile_DoesntNeedMigrating()
		{
			WeSayWordsProject p = new WeSayWordsProject();
			XPathDocument defaultConfig = new XPathDocument(p.PathToDefaultConfig);
			using (TempFile f = new TempFile())
			{
				bool migrated = WeSayWordsProject.MigrateConfigurationXmlIfNeeded(defaultConfig, f.Path);
				Assert.IsFalse(migrated, "The default config file should never need migrating");
			}
		}

		[Test]
		[ExpectedException(typeof (ErrorReport.NonFatalMessageSentToUserException))]
		public void WeSayDirNotInValidBasilDir()
		{
			string experimentDir = MakeDir(Path.GetTempPath(), Path.GetRandomFileName());
			string weSayDir = experimentDir; // MakeDir(experimentDir, "WeSay");
			string wordsPath = Path.Combine(weSayDir, "AAA.words");
			File.Create(wordsPath).Close();
			TryLoading(wordsPath, experimentDir);
		}

		[Test]
		public void LoadPartsOfSpeechList()
		{
			using (ProjectDirectorySetupForTesting p = new ProjectDirectorySetupForTesting(""))
			{
			   // WeSayWordsProject p = CreateAndLoad();
				Field f = new Field();
				f.OptionsListFile = "PartsOfSpeech.xml";
				OptionsList list = p.CreateLoadedProject().GetOptionsList(f, false);
				Assert.IsTrue(list.Options.Count > 2);
			}
		}

		[Test]
		public void CorrectFieldToOptionListNameDictionary()
		{
			using (ProjectDirectorySetupForTesting p = new ProjectDirectorySetupForTesting(""))
			{
				Field f = new Field();
				f.OptionsListFile = "PartsOfSpeech.xml";
				WeSayWordsProject project = p.CreateLoadedProject();
				Dictionary<string, string> dict = project.GetFieldToOptionListNameDictionary();
				Assert.AreEqual("PartsOfSpeech", dict[LexSense.WellKnownProperties.PartOfSpeech]);
			}
		}
//
//        private static WeSayWordsProject CreateAndLoad(TemporaryFolder projectFolder)
//        {
//            WeSayWordsProject p = new WeSayWordsProject();
//            p.LoadFromProjectDirectoryPath(projectDir);
//            return p;
//        }

		private static string MakeDir(string existingParent, string newChild)
		{
			string dir = Path.Combine(existingParent, newChild);
			Directory.CreateDirectory(dir);
			return dir;
		}

		private static void TryLoading(string lexiconPath, string experimentDir)
		{
			try
			{
				WeSayWordsProject p = new WeSayWordsProject();
				lexiconPath = p.UpdateFileStructure(lexiconPath);

				p.LoadFromLiftLexiconPath(lexiconPath);
			}
			finally
			{
				Directory.Delete(experimentDir, true);
			}
		}

		[Test]
		public void GetOptionsListFromFieldName()
		{
			WeSayWordsProject p = new WeSayWordsProject();

			OptionsList list = p.GetOptionsList("POS");
			Assert.IsNotNull(list);
			Assert.IsNotNull(list.Options);
			Assert.Greater(list.Options.Count, 2);
		}

		[Test]
		public void MakeFieldNameChange_CannotBeBrokenWithWeirdNames_IfSafe()
		{
			TryFieldNameChangeAfterMakingSafe("color)", "color(");
			TryFieldNameChangeAfterMakingSafe("(color", ")color");
			TryFieldNameChangeAfterMakingSafe("*color", "color*");
			TryFieldNameChangeAfterMakingSafe("[color", "]color");
			TryFieldNameChangeAfterMakingSafe("color[", "color]");
			TryFieldNameChangeAfterMakingSafe("{color", "}color");
			TryFieldNameChangeAfterMakingSafe("color{", "color}");
			TryFieldNameChangeAfterMakingSafe("?color{", "color");
		}

		[Test]
		public void MigrateAndSaveProduceSameVersion()
		{

			using (ProjectDirectorySetupForTesting projectDir = new ProjectDirectorySetupForTesting(""))
			{
				string configPath = Path.Combine(projectDir.PathToDirectory, "TestProj.WeSayConfig");
				File.WriteAllText(configPath,
								  "<?xml version='1.0' encoding='utf-8'?><tasks><components><viewTemplate></viewTemplate></components><task id='Dashboard' class='WeSay.CommonTools.DashboardControl' assembly='CommonTools' default='true'></task></tasks>");
				XPathDocument doc = new XPathDocument(configPath);
				string outputPath = Path.Combine(projectDir.PathToDirectory, Path.GetTempFileName());
				WeSayWordsProject.MigrateConfigurationXmlIfNeeded(doc, outputPath);
				XmlDocument docFile = new XmlDocument();
				docFile.Load(outputPath);
				XmlNode node = docFile.SelectSingleNode("configuration");
				string migrateVersion = node.Attributes["version"].Value;

				WeSayWordsProject p = projectDir.CreateLoadedProject();
				p.Save();
				docFile.Load(p.PathToConfigFile);
				node = docFile.SelectSingleNode("configuration");
				string saveVersion = node.Attributes["version"].Value;

				Assert.AreEqual(saveVersion, migrateVersion);
			}
		}

		private static void TryFieldNameChangeAfterMakingSafe(string oldName, string newName)
		{
			using (
					ProjectDirectorySetupForTesting dir =
							new ProjectDirectorySetupForTesting(string.Empty))
			{
				WeSayWordsProject p = dir.CreateLoadedProject();
				p.ViewTemplates.Add(new ViewTemplate());
				oldName = Field.MakeFieldNameSafe(oldName);
				newName = Field.MakeFieldNameSafe(newName);
				Field f = new Field(oldName, "LexEntry", new string[] {"en"});
				p.ViewTemplates[0].Add(f);
				p.Save();
				f.FieldName = newName;
				p.MakeFieldNameChange(f, oldName);
			}
		}

		[Test]
		[ExpectedException(typeof(ApplicationException))]
		public void WeSayConfigFileIsToNew_Throws()
		{

			using (ProjectDirectorySetupForTesting projectDir = new ProjectDirectorySetupForTesting(""))
			{
				string configPath = Path.Combine(projectDir.PathToDirectory, "TestProj.WeSayConfig");
				const int version = WeSayWordsProject.CurrentWeSayConfigFileVersion + 1;
				File.WriteAllText(configPath,
								  String.Format("<?xml version='1.0' encoding='utf-8'?><configuration version=\"{0}\"><tasks><components><viewTemplate></viewTemplate></components><task id='Dashboard' class='WeSay.CommonTools.DashboardControl' assembly='CommonTools' default='true'></task></tasks></configuration>", version));
				XPathDocument doc = new XPathDocument(configPath);
				WeSayWordsProject.CheckIfConfigFileVersionIsToNew(doc);
			}
		}

		[Test]
		public void WeSayConfigFileIsToCurrent_DoesNotThrow()
		{

			using (ProjectDirectorySetupForTesting projectDir = new ProjectDirectorySetupForTesting(""))
			{
				string configPath = Path.Combine(projectDir.PathToDirectory, "TestProj.WeSayConfig");
				const int version = WeSayWordsProject.CurrentWeSayConfigFileVersion;
				File.WriteAllText(configPath,
								  String.Format("<?xml version='1.0' encoding='utf-8'?><configuration version=\"{0}\"><tasks><components><viewTemplate></viewTemplate></components><task id='Dashboard' class='WeSay.CommonTools.DashboardControl' assembly='CommonTools' default='true'></task></tasks></configuration>", version));
				XPathDocument doc = new XPathDocument(configPath);
				WeSayWordsProject.CheckIfConfigFileVersionIsToNew(doc);
			}
		}


		/// <summary>
		/// check  (WS-1030) When WeSay is open and you try to change a field, get green box, should get friendly message.
		/// </summary>
		[Test]
		public void MakeFieldNameChange_FileLocked_NotifiesUser()
		{
			using (ProjectDirectorySetupForTesting p = new ProjectDirectorySetupForTesting("<entry id='foo1'><lexical-unit><form lang='v'><text>fooOne</text></form></lexical-unit></entry>"))
			{
				WeSayWordsProject project = p.CreateLoadedProject();
				using (File.OpenWrite(p.PathToLiftFile))
				{
					using (new Palaso.Reporting.ErrorReport.NonFatalErrorReportExpected())
					{
						Field f = new Field("old", "LexEntry", new string[] {"en"});
						project.ViewTemplates[0].Add(f);
						project.Save();
						f.FieldName = "new";
						Assert.IsFalse(project.MakeFieldNameChange(f, "old"));
					}
				}
			}
		}

		[Ignore("just for manual use"), Test]
		public void MakeTestLiftFile()
		{
			string pathToFolder=@"C:\wesay\lifttest";
			string projectName= "lifttest";

			if (!Directory.Exists(pathToFolder))
				Directory.CreateDirectory(pathToFolder);

			StringBuilder builder = new StringBuilder();
			int numberOfTestLexEntries =50;
			for (int i = 0; i < numberOfTestLexEntries; i++)
			{
				builder.AppendFormat(@"
				<entry id='{0}'>
					<lexical-unit>
					  <form lang='v'>
						<text>{0}</text>
					  </form>
					</lexical-unit>
					<sense>
						<grammatical-info value='n'/>
						<definition><form lang='en'><text>blah blah {0} blah blah</text></form></definition>
						<example lang='v'><text>and example of lah blah {0} blah blah</text></example>
					</sense>
				</entry>", i);
			}

			string liftContents =
					  string.Format(
							  "<?xml version='1.0' encoding='utf-8'?><lift version='{0}'>{1}</lift>",
							  .12,
							  builder.ToString());

			File.WriteAllText(Path.Combine(pathToFolder, projectName + ".lift"), liftContents);


		}

		/// <summary>
		/// check issue related to (WS-1035)
		/// </summary>
		[Test]
		public void PathProvidedAsSimpleFileName_GetsConverted()
		{
			using (ProjectDirectorySetupForTesting dir = new ProjectDirectorySetupForTesting("<entry id='foo1'><lexical-unit><form lang='v'><text>fooOne</text></form></lexical-unit></entry>"))
			{
				string oldWorkingDir= System.Environment.CurrentDirectory;
				try
				{
					using (WeSayWordsProject project = new WeSayWordsProject())
					{
						System.Environment.CurrentDirectory = dir.PathToDirectory;
						project.LoadFromLiftLexiconPath(Path.GetFileName(dir.PathToLiftFile));

						Assert.AreEqual(dir.PathToLiftFile, project.PathToLiftFile);
					}
				}
				finally
				{
					System.Environment.CurrentDirectory = oldWorkingDir;
				}

			}
		}
	}
}