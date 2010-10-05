﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Palaso.TestUtilities;
using WeSay.Project.ConfigMigration.UserConfig;

using NUnit.Framework;

namespace WeSay.Project.Tests.ConfigMigration
{
	[TestFixture]
	public class WeSayUserConfigMigrationTests
	{

		[Test]
		public void Migrate_1To2_MigratesToVersion2WithChangedLanguage()
		{
			string v1 = @"<?xml version='1.0' encoding='utf-8'?>
<configuration version='1'>
  <backupPlan />
  <uiOptions>
	<language>wesay-th</language>
	<labelFontName>Angsana New</labelFontName>
	<labelFontSizeInPoints>18</labelFontSizeInPoints>
  </uiOptions>
</configuration>
".Replace("'", "\"");

			using (var folder = new TemporaryFolder("WeSayUserConfigMigrationTests"))
			{
				using (var file = folder.GetNewTempFile(false))
				{
					File.WriteAllText(file.Path, v1);
					var migrator = new WeSayUserConfigMigrator(file.Path);
					migrator.MigrateIfNeeded();
					string result = File.ReadAllText(file.Path);
					AssertThatXmlIn.File(file.Path).HasAtLeastOneMatchForXpath("configuration[@version='2']");
					AssertThatXmlIn.File(file.Path).HasAtLeastOneMatchForXpath("configuration/uiOptions[language='th']");
				}

			}


		}
	}
}
