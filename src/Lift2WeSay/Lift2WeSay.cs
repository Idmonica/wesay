using System;
using System.IO;
using System.Threading;
using WeSay;
using WeSay.Foundation;
using WeSay.Foundation.Progress;
using WeSay.Project;

namespace Lift2WeSay
{
	class Lift2WeSay
	{
		[STAThread]
		static void Main(string[] args)
		{
			if (args.Length != 2)
			{
				PrintUsage();
				return;
			}
			string sourcePath = args[0];
			if (!File.Exists(sourcePath))
			{
				Console.WriteLine(string.Format("Cannot find file {0})", sourcePath));
				return;
			}

			string destPath = args[1];
			if (!Directory.Exists(destPath) && Path.GetFileName(destPath) != string.Empty)
			{
				Console.WriteLine(string.Format("You can only specify a directory for the output, not the name of the output. (eg. {0}{1} instead of {2})", Path.GetDirectoryName(destPath), Path.DirectorySeparatorChar, destPath));
				return;
			}
			string projectPath = destPath;
			if (projectPath.Length == 0)
			{
				projectPath = Environment.CurrentDirectory;
			}
			projectPath = Path.Combine(projectPath, "..");
			projectPath = Path.GetFullPath(projectPath);

			Console.WriteLine("Lift2WeSay is converting");
			Console.WriteLine("Lift: " + sourcePath);
			Console.WriteLine("to WeSay: " + destPath);
			Console.WriteLine("in project: " + projectPath);

			if (!WeSayWordsProject.IsValidProjectDirectory(projectPath))
			{
				Console.Error.WriteLine("destination must be in 'wesay' subdirectory of a basil project");
				return;
			}

			WeSayWordsProject project = new WeSayWordsProject();
			try
			{
				project.LoadFromProjectDirectoryPath(projectPath);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("WeSay was not able to open that project. \r\n" + e.Message);
				return;
			}


			ConsoleProgress progress = new ConsoleProgress();
			progress.Log += new EventHandler<ProgressState.LogEvent>(progress_Log);

			ImportLIFTCommand command = new ImportLIFTCommand(sourcePath);
			command.BeginInvoke(progress);

			while (true)
			{
				switch (progress.State)
				{
					case ProgressState.StateValue.NotStarted:
						break;
					case ProgressState.StateValue.Busy:
						break;
					case ProgressState.StateValue.Finished:
						Console.WriteLine(string.Empty);
						Console.WriteLine("Done.");
						return;
					case ProgressState.StateValue.StoppedWithError:
						Console.Error.WriteLine(string.Empty);
						Console.Error.WriteLine("Error. Unable to complete import.");
						return;
					default:
						break;
				}
				Thread.Sleep(10);
			}
		}

		static void progress_Log(object sender, ProgressState.LogEvent e)
	  {
		Console.Error.WriteLine(e.message);
	  }

		private static void PrintUsage()
		{
			Console.WriteLine("Usage: (outputfile must be in 'wesay' subdirectory of a basil project)");
			Console.WriteLine("Lift2WeSay inputLiftFilePath targetWeSayDirectory");
		}
	}
}
