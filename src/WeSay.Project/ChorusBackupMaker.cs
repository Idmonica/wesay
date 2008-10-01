﻿using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Chorus.sync;
using Chorus.Utilities;

namespace WeSay.Project
{
	/// <summary>
	/// Uses chorus to regularly backup to a second drive (SD, SSD, etc.)
	/// </summary>
	[XmlRoot("backupPlan")]
	public class ChorusBackupMaker
	{
	  public const string ElementName = "backupPlan";

	  [XmlElement("pathToParentOfRepositories")]
		public string PathToParentOfRepositories;

		public static ChorusBackupMaker LoadFromReader(XmlReader reader)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(ChorusBackupMaker));
			return (ChorusBackupMaker)serializer.Deserialize(reader);
		}

		public void Save(XmlWriter writer)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(ChorusBackupMaker));
			serializer.Serialize(writer, this);
		}

		public void BackupNow(string pathToProjectDirectory)
		{
			if (string.IsNullOrEmpty(PathToParentOfRepositories))
			{
				Palaso.Reporting.Logger.WriteMinorEvent("Backup location not specified, skipping backup.");
				return;
			}
			if (!Directory.Exists(PathToParentOfRepositories))
			{
				Palaso.Reporting.Logger.WriteEvent("Backup location not found, skipping backup.");
				return;
			}

			try
			{
				ProjectFolderConfiguration projectFolder = new ProjectFolderConfiguration(pathToProjectDirectory);
				projectFolder.ExcludePatterns.Add("**/cache");
				projectFolder.ExcludePatterns.Add("*.old");
				projectFolder.ExcludePatterns.Add("*.tmp");
				projectFolder.IncludePatterns.Add("*.*");
			   // projectFolder.IncludePatterns.Add(project.ProjectDirectoryPath);

				Chorus.sync.SyncOptions options = new SyncOptions();
				options.DoMergeWithOthers = false;
				options.DoPullFromOthers = false;
				options.DoPushToLocalSources = true;
				options.RepositorySourcesToTry.Clear();
				RepositorySource backupSource = RepositorySource.Create(PathToParentOfRepositories, "backup", false);
				options.RepositorySourcesToTry.Add(backupSource);

				RepositoryManager manager = RepositoryManager.FromRootOrChildFolder(projectFolder);

				if (!RepositoryManager.CheckEnvironmentAndShowMessageIfAppropriate("en"))//todo localization
				{
					Palaso.Reporting.Logger.WriteEvent("Backup not possible: {0}", RepositoryManager.GetEnvironmentReadinessMessage("en"));
					return;
				}


				//TODO: figure out how/what/when to show progress. THis is basically just throwing it away
				IProgress progress = new Chorus.Utilities.StringBuilderProgress();
				manager.SyncNow(options, progress);
			}
			catch (Exception error)
			{
				Palaso.Reporting.Logger.WriteEvent("Error during Backup: {0}", error.Message);
				//TODO we need some passive way indicating the health of the backup system
			}
		}
	}
}
