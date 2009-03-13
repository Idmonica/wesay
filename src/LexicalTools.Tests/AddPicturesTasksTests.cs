﻿using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Palaso.IO;
using WeSay.Foundation;
using WeSay.Foundation.Tests.TestHelpers;
using WeSay.LexicalModel;
using WeSay.LexicalTools.AddPictures;

namespace WeSay.LexicalTools.Tests
{
	[TestFixture]
	public class AddPicturesTasksAbnormalSetupTests
	{
		[Test, ExpectedException(typeof(Palaso.Reporting.ConfigurationException))]
		public void Activate_IndexNotFound_GivesUserMessage()
		{
				using (var repoFile = new WeSay.Foundation.Tests.TestHelpers.TempLiftFile(""))
				{
					using(var repo = new LexEntryRepository(repoFile.Path))
					{
						AddPicturesConfig config = MakeConfig("Bogus.txt");
						var task = new AddPicturesTask(config, repo,
							new TaskMemoryRepository(),
							new FileLocator(new string[0])  );
						task.Activate();
					}
				}
		}

		private AddPicturesConfig MakeConfig(string indexName)
		{
			return new AddPicturesConfig(string.Format("<task taskName='AddMissingInfo' visible='true'><indexFileName>{0}</indexFileName></task>", indexName));
		}
	}

	[TestFixture]
	public class AddPicturesTasksTests
	{
		/*
		 * The actual collection is part of Palaso, and the tests for it are over there.
		 *
		 *
		 *
		 *
		 *
		 */
		private TempLiftFile _repoFile;
		private LexEntryRepository _repo;
		private AddPicturesTask _task;

		[SetUp]
		public void Setup()
		{
			_repoFile = new WeSay.Foundation.Tests.TestHelpers.TempLiftFile("");
			_repo = new LexEntryRepository(_repoFile.Path);
			AddPicturesConfig config = new AddPicturesConfig(string.Format("<task taskName='AddMissingInfo' visible='true'><indexFileName>{0}</indexFileName></task>", "ArtOfReadingIndexV3_en.txt"));
			_task = new AddPicturesTask(config, _repo,
										new TaskMemoryRepository(),
										new FileLocator(new string[]{WeSay.Project.BasilProject.ApplicationCommonDirectory}));
			_task.Activate();
		}
		[TearDown]
		public void TearDown()
		{
			_task.Deactivate();
			_repo.Dispose();
			_repoFile.Dispose();
		}


	}
}
