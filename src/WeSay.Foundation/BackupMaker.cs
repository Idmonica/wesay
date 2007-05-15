using System;
using System.Collections.Generic;
using System.IO;


namespace WeSay.Foundation
{
	public class BackupMaker
	{
		/// <summary>
		/// Caller must catch exceptions this throws for ordinary conditions (like if a destination is full)
		/// </summary>
		/// <param name="sourceProjectPath"></param>
		/// <param name="destinationZipPath"></param>
		public static void BackupToExternal(string sourceProjectPath, string destinationZipPath,  string[] paths)
		{
			if (!Directory.Exists(Path.GetPathRoot(sourceProjectPath)))
				throw new ApplicationException("Directory of the project doesn't exist.");

			if (!Directory.Exists(Path.GetPathRoot(destinationZipPath)))
				throw new ApplicationException("Directory of the destination doesn't exist.");

			Reporting.Logger.WriteEvent("Start Backup to {0}", destinationZipPath);

			//the tricky part here is to get all the paths to be relative, starting with the
			//project root, rather than specifying them all the way up to the hard drive

			System.IO.Directory.SetCurrentDirectory(Directory.GetParent(sourceProjectPath).FullName);
			ICSharpCode.SharpZipLib.Zip.ZipFile zipFile = ICSharpCode.SharpZipLib.Zip.ZipFile.Create(destinationZipPath);
			zipFile.BeginUpdate();

			string pathPriorToRootOfProject = Directory.GetParent(sourceProjectPath).FullName;
			foreach (string s in paths)
			{
				zipFile.Add(s.Replace(pathPriorToRootOfProject+System.IO.Path.DirectorySeparatorChar,""));
			}
			zipFile.CommitUpdate();
			zipFile.Close();

			Reporting.Logger.WriteEvent("Backup Done");
		}
	}
}
