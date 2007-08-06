using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.XPath;
using NUnit.Framework;
using NUnit.Extensions.Forms;
using Reporting;
using WeSay.Project;
using WeSay.Setup;

namespace WeSay.Admin.Tests
{
	[TestFixture]
	public class AdminWindowTests : NUnitFormTest
	{
		private AdminWindow _window;
		private string _projectFolder;
		private FormTester _mainWindowTester;

		public override void Setup()
		{
			Reporting.ErrorReport.OkToInteractWithUser = false;
			base.Setup();
			_window = new AdminWindow(new string[] { });
			_window.Show();
			string name = new Finder().Name(_window);
			_mainWindowTester = new FormTester(name);

			this._projectFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

		}

		public override void TearDown()
		{
			base.TearDown();
			if (WeSayWordsProject.IsInitialized)
			{
				WeSayWordsProject.Project.Dispose();
			}

			WeSay.Foundation.Tests.TestUtilities.DeleteFolderThatMayBeInUse(_projectFolder);
		}



		[Test]
		public void ProjectFilesTouched()
		{
			_window.OpenProject( BasilProject.GetPretendProjectDirectory());
			string p = BasilProject.Project.PathToWritingSystemPrefs;
			DateTime before = File.GetLastWriteTime(p);
			_mainWindowTester.Close();
			DateTime after = File.GetLastWriteTime(p);
			Assert.AreNotEqual(before, after);
		}

		//stupid nunitforms will freak 'cause window was closed
		[Test]//, ExpectedException(typeof(FormsTestAssertionException))]
		public void AfterCreateProjectAndQuitFilesExist()
		{
			List<string> paths = new List<string>();
			_window.CreateAndOpenProject(this._projectFolder);
			paths.Add(BasilProject.Project.PathToWritingSystemPrefs);
			paths.Add(WeSayWordsProject.Project.PathToConfigFile);
			//paths.Add(WeSayWordsProject.Project.PathToDb4oLexicalModelDB);
			_mainWindowTester.Close();
			foreach (string p in paths)
			{
				if (!File.Exists(p))
				{
					Assert.Fail("Did not create "+p);
				}
			}
		}

		[Test]
		public void WalkTabsAfterOpeningPretendProject()
		{
			  _window.OpenProject(BasilProject.GetPretendProjectDirectory());
		  //create or overwrite the tasks with our stored resource
//            File.Delete(WeSayWordsProject.Project.PathToProjectTaskInventory);
//            StreamWriter writer = File.CreateText(WeSayWordsProject.Project.PathToProjectTaskInventory);
//            writer.Write(TestResources.tasks);
//            writer.Close();
			  File.Copy(Path.Combine(WeSayWordsProject.Project.ApplicationTestDirectory, "pretend.WeSayConfig"), WeSayWordsProject.Project.PathToConfigFile, true);
			WalkTopLevelTabs();
		}

		[Test, ExpectedException(typeof(ErrorReport.NonFatalMessageSentToUserException))]
		public void TryingToOpenNonExistantProjectDoesntCrash()
		{
			_window.OnOpenProject(@"C:\notreallythere.WeSayConfig", null);
		}

		[Test]
		public void WalkTabsAfterCreateNewProject()
		{
			_window.CreateAndOpenProject(_projectFolder);
			WalkTopLevelTabs();
		}
		[Test]
		public void CreateNewProjectThenOpen()
		{
			_window.CreateAndOpenProject(_projectFolder);
		}

		[Test]
		public void NewProjectShowsTasks()
		{
			CreateProjectAndGoToTaskControl();
			CheckedListBoxTester c = new CheckedListBoxTester("_taskList");
			Assert.Greater(c.Properties.Items.Count, 0);
		}

		[Test]
		public void NewProjectShowSomeDefaultTasks()
		{
			CreateProjectAndGoToTaskControl();
			CheckedListBoxTester c = new CheckedListBoxTester("_taskList");
			Assert.Greater(c.Properties.CheckedItems.Count, 0);
		}

		[Test]
		public void NewProjectHasValidStructure()
		{
			_window.CreateAndOpenProject(_projectFolder);
			string path = WeSay.Project.WeSayWordsProject.Project.PathToConfigFile;
			XPathDocument doc = new XPathDocument(path);
			Assert.IsNotNull(doc.CreateNavigator().SelectSingleNode("configuration[@version='1']"));
			Assert.IsNotNull(doc.CreateNavigator().SelectSingleNode("configuration/tasks"));
			Assert.IsNotNull(doc.CreateNavigator().SelectSingleNode("configuration/components"));
		}

		private void CreateProjectAndGoToTaskControl()
		{
			_window.CreateAndOpenProject(_projectFolder);

			GotoProjectTab("_tasksPage");
		}

		[Test]
		public void NewProjectShowsTwoWritingSystems()
		{
			_window.CreateAndOpenProject(_projectFolder);

			GotoProjectTab("_writingSystemPage");
			ListBoxTester c = new ListBoxTester("_wsListBox");
			Assert.AreEqual(2, c.Properties.Items.Count);
		}

		private static void GotoProjectTab(string projectTabName)
		{
			TabControlTester t = new TabControlTester("_projectTabControl");

			foreach (TabPage page in t.Properties.TabPages)
			{
				if (page.Name == projectTabName)
				{
					t.Properties.SelectedTab = page;
					break;
				}
			}
//            t.Properties.SelectedTab = t.Properties.TabPages[projectTabName];

			Assert.IsNotNull(t.Properties.SelectedTab, "Couldn't find "+projectTabName);
		}

		static private void WalkTopLevelTabs()
		{
//            for(int i = 0; i<10000;i++)
//            {
//                Application.DoEvents();
//            }
			TabControlTester t = new TabControlTester("_projectTabControl");
			foreach(TabPage p in t.Properties.TabPages)
			{
				t.Properties.SelectedTab = p;
			}
		}

		[Test]
		public void RunAndExitWithoutOpening()
		{
			_mainWindowTester.Close();
			WeSayWordsProject.InitializeForTests(); // for Teardown
		}


	}
}
