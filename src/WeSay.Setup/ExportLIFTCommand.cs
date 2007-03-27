using System;
using MultithreadProgress;
using WeSay.Data;
using WeSay.Foundation;
using WeSay.Foundation.Progress;
using WeSay.LexicalModel;
using WeSay.LexicalModel.Db4o_Specific;
using WeSay.Project;

namespace WeSay
{
	 public class ExportLIFTCommand : BasicCommand
	{
		protected string _destinationLIFTPath;
		protected string _sourceWordsPath;
		 protected WeSay.Foundation.Progress.ProgressState _progress;

		public ExportLIFTCommand(
			string destinationLIFTPath, string sourceWordsPath        )
		{
			_destinationLIFTPath = destinationLIFTPath;
			_sourceWordsPath = sourceWordsPath;
		}
		 protected override void DoWork2(ProgressState progress)
		 {
			 _progress = progress;
			 _progress.StatusLabel="Exporting...";
			 _progress.Status = ProgressState.StatusValue.Busy;
			 WeSay.LexicalModel.LiftExporter exporter = null;
			 try
			 {
				 exporter = new LiftExporter(WeSayWordsProject.Project.GetFieldToOptionListNameDictionary(), _destinationLIFTPath);

				 using (Db4oDataSource ds = new Db4oDataSource(_sourceWordsPath))
				 {
				   Db4oLexModelHelper.Initialize(ds.Data);

					 using (Db4oRecordList<LexEntry> entries = new Db4oRecordList<LexEntry>(ds))
					 {
						 _progress.NumberOfSteps = entries.Count;
						 for (int i = 0; i < entries.Count; )
						 {
							 int howManyAtATime = 100;
							 howManyAtATime = Math.Min(100, entries.Count - i);
							 exporter.Add(entries, i, howManyAtATime);
							 i += howManyAtATime;
							 _progress.NumberOfStepsCompleted = i;
							 if (_progress.Cancel)
							 {
								 break; ;
							 }
						 }
					 }
				 }
				 _progress.Status = ProgressState.StatusValue.Finished;

			 }
			 catch (Exception e)
			 {
				 _progress.Status = ProgressState.StatusValue.StoppedWithError;
			 }
			 finally
			 {
				 exporter.End();
			 }
		 }

		protected override void DoWork(
			InitializeProgressCallback initializeCallback,
			ProgressCallback progressCallback,
			StatusCallback primaryStatusTextCallback,
			StatusCallback secondaryStatusTextCallback
			)
		{
			throw new NotImplementedException();

//            primaryStatusTextCallback.Invoke("Exporting...");
//           WeSay.LexicalModel.LiftExporter exporter=null;
//            try
//            {
//                exporter = new LiftExporter(_destinationLIFTPath);
//
//                using (Db4oDataSource ds = new Db4oDataSource(_sourceWordsPath))
//                {
//                    using (Db4oRecordList<LexEntry> entries = new Db4oRecordList<LexEntry>(ds))
//                    {
//                        initializeCallback(0, entries.Count);
//                        for (int i = 0; i < entries.Count; )
//                        {
//                            const int howManyAtATime = 100;
//                            exporter.Add(entries, i, howManyAtATime);
//                            i += howManyAtATime;
//                            progressCallback(i);
//                            if( Canceling )
//                            {
//                                return;
//                            }
//                        }
//                    }
//                }
//            }
//            finally
//            {
//                exporter.End();
//            }
		}

	}
}