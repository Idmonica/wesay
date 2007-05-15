using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;
using WeSay.Project;

namespace WeSay.Foundation.Tests
{
	[TestFixture]
	public class BackupMakerTests
	{
		public BackupMaker _backupMaker;
		private string _destinationZip;
		private string _sourceProjectPath;
		private string[] _filesToBackup;

		[SetUp]
		public void Setup()
		{
			WeSayWordsProject.InitializeForTests();
			_destinationZip = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".zip");
			_sourceProjectPath = Project.WeSayWordsProject.GetPretendProjectDirectory();
			_backupMaker = new BackupMaker();
			_filesToBackup = WeSay.Project.WeSayWordsProject.GetFilesBelongingToProject(_sourceProjectPath);

		}

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(_destinationZip))
			{
				File.Delete(_destinationZip);
			}
		}

		[Test, ExpectedException(typeof(ApplicationException))]
		public void ThrowIfCannotCreateDestination()
		{
			BackupMaker.BackupToExternal(Project.WeSayWordsProject.GetPretendProjectDirectory(), "Q:\\" + Path.GetRandomFileName(), _filesToBackup);
		}

		[Test, ExpectedException(typeof(ApplicationException))]
		public void ThrowIfSourceDoesntExist()
		{
			BackupMaker.BackupToExternal(Path.GetRandomFileName(), _destinationZip,_filesToBackup);
		}

		[Test, Ignore("Not implemented yet")]
		public void WhatIfNotEnoughRoom()
		{
		}

		[Test]
		public void OverwritesExistingZip()
		{
			string dummyPath = MakeDummyExistingZip();
			try
			{
				BackupMaker.BackupToExternal(_sourceProjectPath, _destinationZip, _filesToBackup);
				AssertHasReasonableContents();
				AssertDoesNotContainFile(dummyPath);
			}
			finally
			{
				File.Delete(dummyPath);
			}
		}

		private void AssertDoesNotContainFile(string partialPath)
		{
			ICSharpCode.SharpZipLib.Zip.ZipFile f = null;
			try
			{
				f = new ICSharpCode.SharpZipLib.Zip.ZipFile(_destinationZip);
				Assert.AreEqual(-1, f.FindEntry(GetZipFileInternalPath(partialPath), true));
			}
			finally
			{
				if (f != null)
				{
					f.Close();
				}
			}
		}

		/// <summary>
		/// strip off c:\ and make all slashes forward slashes
		/// </summary>
		private static string GetZipFileInternalPath(string partialPath)
		{
			return partialPath.Substring(3).Replace("\\","/");
		}

		private string MakeDummyExistingZip()
		{
			ICSharpCode.SharpZipLib.Zip.ZipFile z = ICSharpCode.SharpZipLib.Zip.ZipFile.Create(_destinationZip);
			z.BeginUpdate();
			string tempFile = Path.GetTempFileName();
			z.Add(tempFile);
			z.CommitUpdate();
			z.Close();
			File.Delete(tempFile);
			return tempFile;
		}

		[Test]
		public void SelectsCorrectFiles()
		{
			BackupMaker.BackupToExternal(_sourceProjectPath, _destinationZip, _filesToBackup);
			AssertHasReasonableContents();
		}


		[Test]
		public void OkIfNoFilesChosen()
		{
			BackupMaker.BackupToExternal(_sourceProjectPath, _destinationZip, new string[] { });
		}

		private void AssertHasReasonableContents()
		{
			Assert.IsTrue(File.Exists(_destinationZip));
			ICSharpCode.SharpZipLib.Zip.ZipFile f=null;
			try
			{
				f= new ICSharpCode.SharpZipLib.Zip.ZipFile(_destinationZip);
				Assert.AreNotEqual(-1, f.FindEntry("pretend/wesay/pretend.lift", true));
			}
			finally
			{
				if (f != null)
				{
					f.Close();
				}
			}
		}
	}

}