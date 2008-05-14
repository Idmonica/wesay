using System.IO;
using System.Windows.Forms;
using NUnit.Framework;
using WeSay.Project;
using WeSay.ConfigTool;

namespace WeSay.ConfigTool.Tests
{
	[TestFixture]
	public class CauseProblemWithAutoCompleteTestsWithCreationTests
	{
		private ConfigurationWindow _window;

		[SetUp]
		public void Setup()
		{
			Palaso.Reporting.ErrorReport.IsOkToInteractWithUser = false;
			_window = new ConfigurationWindow(new string[] { });
			_window.Show();
		}
		[TearDown]
		public void TearDown()
		{
			_window.Close();
			_window.Dispose();
			if (WeSayWordsProject.IsInitialized)
			{
				WeSayWordsProject.Project.Dispose();
			}
		}

		[Test, Ignore("Mysteriously Causes AutoCompleteWithCreationBoxTestsToFail")]
		public void WalkTabsAfterOpeningPretendProject()
		{
			_window.OpenProject(BasilProject.GetPretendProjectDirectory());
			File.Copy(Path.Combine(WeSayWordsProject.Project.ApplicationTestDirectory, "PRETEND.WeSayConfig"), WeSayWordsProject.Project.PathToConfigFile, true);

			ToolStrip toolstrip = (ToolStrip)_window.Controls.Find("_areasToolStrip", true)[0];
			foreach (ToolStripButton button in toolstrip.Items)
			{
				button.PerformClick();
			}
		}
	}
}
