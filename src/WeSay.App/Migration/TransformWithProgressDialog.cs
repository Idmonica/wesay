using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Palaso.Progress;
using Palaso.UI.WindowsForms.Progress;

namespace WeSay.App
{
	/// <summary>
	/// Does an xslt transform with a cancellable progress dialog.
	/// The XSLT should have a <xsl:message/> to trigger each step
	/// </summary>
	public class TransformWithProgressDialog
	{
		private readonly string _pathToLift;
		private readonly Stream _xsltStream;
		private readonly string _xpathToCountSteps;
		private XsltArgumentList _xsltArguments;

		private string _outputPath;
		private static ProgressState _staticProgressStateForWorker;
		private string _taskMessage;

		public delegate void FileManipulationMethod(object sender, DoWorkEventArgs e);

		public TransformWithProgressDialog(string pathToLift, string outputPath, Stream xsltStream, string xpathToCountSteps)
		{
			_pathToLift = pathToLift;
			_outputPath = outputPath;
			_xsltStream = xsltStream;
			_xpathToCountSteps = xpathToCountSteps;
		}

		public XsltArgumentList XsltArguments
		{
			get { return _xsltArguments; }
			set { _xsltArguments = value; }
		}

		public string TaskMessage
		{
			get { return _taskMessage; }
			set { _taskMessage = value; }
		}

		public void Transform()
		{
			TransformWorkerArguments targs = new TransformWorkerArguments();
			targs.outputFilePath = _outputPath;
			targs.XpathForStepCount = _xpathToCountSteps;
			using (targs.outputStream = File.Create(_outputPath))
			{
				targs.inputDocument = new XmlDocument();
				targs.inputDocument.PreserveWhitespace = true;
				targs.inputDocument.Load(_pathToLift);

				targs.xsltStream = _xsltStream;
				if (XsltArguments == null)
				{
					_xsltArguments = new XsltArgumentList();
				}
				targs.xsltArguments = XsltArguments;
				if(!DoTransformWithProgressDialog(targs))
				{
					return ;
				}
			}
		}


		/// <summary>
		///
		/// </summary>
		/// <param name="arguments"></param>
		/// <returns>false if not successful or cancelled</returns>
		private bool DoTransformWithProgressDialog(TransformWorkerArguments arguments)
		{
			using (ProgressDialog dlg = new ProgressDialog())
			{
				dlg.Overview =string.Format("{0}", _taskMessage);
				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += new DoWorkEventHandler(OnDoTransformWork);
				dlg.BackgroundWorker = worker;
				dlg.CanCancel = true;
				//dlg.CancelRequested += new EventHandler(OnCancelRequested);
				dlg.ProgressState.Arguments = arguments;
				dlg.ShowDialog();
				if (dlg.ProgressStateResult!=null && dlg.ProgressStateResult.ExceptionThatWasEncountered != null)
				{
					Palaso.Reporting.ErrorNotificationDialog.ReportException(dlg.ProgressStateResult.ExceptionThatWasEncountered,null,false);
					return false;
				}
				return !dlg.ProgressState.Cancel;
			}
		}

		internal class TransformWorkerArguments
		{
			public XmlDocument inputDocument;
			public XsltArgumentList xsltArguments;
			public Stream outputStream;
			public Stream xsltStream;
			public FileManipulationMethod postTransformMethod;
			public string outputFilePath;
			public object postTransformArgument;
			public int postTransformSteps;
			private string _xpathForStepCount;

			public string XpathForStepCount
			{
				get { return _xpathForStepCount; }
				set { _xpathForStepCount = value; }
			}
		}

		/// <summary>
		/// this runs in a worker thread
		/// </summary>
		private static void OnDoTransformWork(object sender, DoWorkEventArgs args)
		{
			ProgressState progressState = (ProgressState) args.Argument;
			XslCompiledTransform transform = null;
			try
			{
				TransformWorkerArguments workerArguments = (TransformWorkerArguments) progressState.Arguments;

				transform = new XslCompiledTransform();

				//all this just to allow a DTD statement in the source xslt
				XmlReaderSettings readerSettings = new XmlReaderSettings();
				readerSettings.ProhibitDtd = false;

				progressState.StatusLabel = "Preparing...";
				using (Stream stream = workerArguments.xsltStream)
				{
					using (XmlReader xsltReader = XmlReader.Create(stream, readerSettings))
					{
						XsltSettings settings = new XsltSettings(true, true);
						transform.Load(xsltReader, settings, new XmlUrlResolver());
						xsltReader.Close();
					}
					stream.Close();
				}

				progressState.StatusLabel = "Processing...";
				int entriesCount = workerArguments.inputDocument.SelectNodes(workerArguments.XpathForStepCount).Count;
				progressState.TotalNumberOfSteps = entriesCount + workerArguments.postTransformSteps;
				_staticProgressStateForWorker = progressState;
				workerArguments.xsltArguments.XsltMessageEncountered += new XsltMessageEncounteredEventHandler(OnXsltMessageEncountered);
				transform.Transform(workerArguments.inputDocument, workerArguments.xsltArguments,
									workerArguments.outputStream);

				workerArguments.outputStream.Close();//let the next guy get at the file
				System.Diagnostics.Debug.Assert(progressState.NumberOfStepsCompleted <= entriesCount, "Should use up more than we reserved for ourselves");
				progressState.NumberOfStepsCompleted = entriesCount;
				if(workerArguments.postTransformMethod != null)
				{
					workerArguments.postTransformMethod.Invoke(sender, args);
				}
				progressState.State = ProgressState.StateValue.Finished;
			}
			catch(CancelingException notAnErr)
			{
				progressState.State = ProgressState.StateValue.Finished;
			}
			catch (Exception err)
			{
				//currently, error reporter can choke because this is
				//being called from a non sta thread.
				//so let's leave it to the progress dialog to report the error
				//                Reporting.ErrorReporter.ReportException(args,null, false);
				progressState.ExceptionThatWasEncountered = err;
				progressState.WriteToLog(err.Message);
				progressState.State = ProgressState.StateValue.StoppedWithError;
			}
			finally
			{
				if (transform != null)
				{
					progressState.StatusLabel = "Cleaning up...";
					TempFileCollection tempfiles = transform.TemporaryFiles;
					if (tempfiles != null)  // tempfiles will be null when debugging is not enabled
					{
						tempfiles.Delete();
					}
				}
			}
		}

		static void OnXsltMessageEncountered(object sender, XsltMessageEncounteredEventArgs e)
		{
			_staticProgressStateForWorker.NumberOfStepsCompleted++;
			if(_staticProgressStateForWorker.Cancel)
			{
				throw new CancelingException();
			}
		}

		/// <summary>
		/// used to break us out of the xslt transformer if the user cancels
		/// </summary>
		private class CancelingException : ApplicationException
		{
		}
	}
}