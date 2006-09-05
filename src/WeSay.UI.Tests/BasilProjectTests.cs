using System.IO;
using NUnit.Framework;
using WeSay.Language;

namespace WeSay.UI.Tests
{
	[TestFixture]
	public class BasilProjectTests
	{
		private string _projectDirectory;

		[SetUp]
		public void Setup()
		{
			DirectoryInfo dirProject = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
			//DirectoryInfo dirCommon = Directory.CreateDirectory(Path.Combine(dirProject.FullName, "common"));
			_projectDirectory = dirProject.FullName;
			BasilProject p = new BasilProject(_projectDirectory);

			Directory.CreateDirectory(Directory.GetParent(p.PathToWritingSystemPrefs).FullName);

			StreamWriter writer = File.CreateText(p.PathToWritingSystemPrefs);
			writer.Write(TestResources.WritingSystemPrefs);
			writer.Close();

			p.InitWritingSystems();
		}
		[TearDown]
		public void TearDown()
		{
			Directory.Delete(_projectDirectory, true);
		}

		[Test]
		public void Test()
		{
			WritingSystem ws = BasilProject.Project.AnalysisWritingSystemDefault;
			Assert.AreEqual("ANA", ws.Id);
			Assert.AreEqual("Wingdings", ws.Font.Name);
			Assert.AreEqual(20, ws.Font.Size);

		}

		[Test]
		public void NoSetupDefaultWritingSystems()
		{
			WritingSystem ws = BasilProject.Project.AnalysisWritingSystemDefault;
			Assert.IsNotNull(ws);
			ws = BasilProject.Project.VernacularWritingSystemDefault;
			Assert.IsNotNull(ws);
		}
	}
}
