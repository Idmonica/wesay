using System;
using System.IO;
using System.Xml;
using WeSay.Data;
using WeSay.Foundation;
using WeSay.Foundation.Progress;
using WeSay.LexicalModel;
using WeSay.LexicalModel.Db4o_Specific;

namespace WeSay
{
	public class ImportLIFTCommand : BasicCommand
	{
		protected string _destinationDatabasePath;
		protected string _sourceLIFTPath;
		private LiftImporter _importer;
		protected WeSay.Foundation.Progress.ProgressState _progress;

		public ImportLIFTCommand(string destinationDatabasePath, string sourceLIFTPath)
		{
			_destinationDatabasePath = destinationDatabasePath;
			_sourceLIFTPath =sourceLIFTPath;
		}

		protected override void DoWork2(ProgressState progress)
		{


			_progress = progress;
			if (File.Exists(_destinationDatabasePath)) // make backup of the file we're about to over-write
			{
				progress.Status = "Backing up existing file...";
				string backupPath = _destinationDatabasePath + ".bak";
				File.Delete(backupPath);
				File.Move(_destinationDatabasePath, backupPath);
			}

			progress.Status = "Importing...";
			using (Db4oDataSource ds = new WeSay.Data.Db4oDataSource(_destinationDatabasePath))
			{
				using (Db4oRecordList<LexEntry> entries = new Db4oRecordList<LexEntry>(ds))
				{
					entries.WriteCacheSize = 0; // don't commit all the time.
					if (Db4oLexModelHelper.Singleton == null)
					{
						Db4oLexModelHelper.Initialize(ds.Data);
					}

					XmlDocument doc = new XmlDocument();
					doc.Load(_sourceLIFTPath);
					_importer = LiftImporter.CreateCorrectImporter(doc);

					foreach (string name in WeSay.Project.WeSayWordsProject.Project.OptionFieldNames)
					{
						_importer.ExpectedOptionTraits.Add(name);
					}
					foreach (string name in WeSay.Project.WeSayWordsProject.Project.OptionCollectionFieldNames)
					{
						_importer.ExpectedOptionCollectionTraits.Add(name);
					}
					_importer.Progress = progress;
					_importer.ReadFile(doc, entries);
				}
			}
		}

		protected override void DoWork(InitializeProgressCallback initializeCallback, ProgressCallback progressCallback,
									   StatusCallback primaryStatusTextCallback,
									   StatusCallback secondaryStatusTextCallback)
		{
			throw new NotImplementedException();
		}
	}

}