//originally from Matthew Adams

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using WeSay;
using WeSay.Foundation.Progress;

namespace WeSay.UI
{
	/// <summary>
	/// Provides a progress dialog similar to the one shown by Windows
	/// </summary>
	public class ProgressDialog : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label _statusLabel;
		private System.Windows.Forms.ProgressBar _progressBar;
		private System.Windows.Forms.Label _progressLabel;
		private System.Windows.Forms.Button _cancelButton;
		private System.Windows.Forms.Timer _showWindowIfTakingLongTimeTimer;
		private bool _showOnce;
		private System.Windows.Forms.Timer _progressTimer;
		private bool _isClosing;
		private Label _overviewLabel;
		private DateTime _startTime = DateTime.Now;
		private System.ComponentModel.IContainer components;
		private BackgroundWorker _backgroundWorker;
		private ProgressState _lastHeardFromProgressState;

		/// <summary>
		/// Standard constructor
		/// </summary>
		public ProgressDialog()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			_statusLabel.BackColor = System.Drawing.SystemColors.Control;
			_progressLabel.BackColor = System.Drawing.SystemColors.Control;
			_overviewLabel.BackColor = System.Drawing.SystemColors.Control;

		}

		/// <summary>
		/// Get / set the time in ms to delay
		/// before showing the dialog
		/// </summary>
		public int DelayShowInterval
		{
			get
			{
				return _showWindowIfTakingLongTimeTimer.Interval;
			}
			set
			{
				_showWindowIfTakingLongTimeTimer.Interval = value;
			}
		}

		/// <summary>
		/// Get / set the text to display in the first status panel
		/// </summary>
		public string StatusText
		{
			get
			{
				return _statusLabel.Text;
			}
			set
			{
				_statusLabel.Text = value;
			}
		}

		/// <summary>
		/// Description of why this dialog is even showing
		/// </summary>
		public string Overview
		{
			get
			{
				return _overviewLabel.Text;
			}
			set
			{
				_overviewLabel.Text = value;
			}
		}

		/// <summary>
		/// Get / set the minimum range of the progress bar
		/// </summary>
		public int ProgressRangeMinimum
		{
			get
			{
				return _progressBar.Minimum;
			}
			set
			{
				_progressBar.Minimum = value;
			}
		}

		/// <summary>
		/// Get / set the maximum range of the progress bar
		/// </summary>
		public int ProgressRangeMaximum
		{
			get
			{
				return _progressBar.Maximum;
			}
			set
			{
				_progressBar.Maximum = value;
			}
		}

		/// <summary>
		/// Get / set the current value of the progress bar
		/// </summary>
		public int Progress
		{
			get
			{
				return _progressBar.Value;
			}
			set
			{
				_progressBar.Value = value;
			}
		}

		/// <summary>
		/// Get/set a boolean which determines whether the form
		/// will show a cancel option (true) or not (false)
		/// </summary>
		public bool CanCancel
		{
			get
			{
				return _cancelButton.Enabled;
			}
			set
			{
				_cancelButton.Enabled = value;
			}
		}

		/// <summary>
		/// If this is set before showing, the dialog will run the worker and respond
		/// to its events
		/// </summary>
		public BackgroundWorker BackgroundWorker
		{
			get
			{
				return _backgroundWorker;
			}
			set
			{
				_backgroundWorker = value;
			}
		}

		public ProgressState ProgressStateResult
		{
			get
			{
				return _lastHeardFromProgressState;
			}
		}

		void OnBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			//BackgroundWorkerState progressState = e.Result as ProgressState;

			if(e.Cancelled )
			{
				this.DialogResult = DialogResult.Cancel;
			}
			//NB: I don't know how to actually let the BW know there was an error
			//else if (e.Error != null ||
			else if (ProgressStateResult != null && ProgressStateResult.ExceptionThatWasEncountered != null)
			{
				Reporting.ErrorReporter.ReportException(ProgressStateResult.ExceptionThatWasEncountered, this, false);
				this.DialogResult = DialogResult.Abort;//not really matching semantics
			}
			else
			{
				this.DialogResult = DialogResult.OK;
			}
			_isClosing = true;
			Close();
		}

		void OnBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			ProgressState state = e.UserState as ProgressState;
			if (state != null)
			{
				_lastHeardFromProgressState = state;
				ProgressRangeMaximum = state.TotalNumberOfSteps;
				Progress = state.NumberOfStepsCompleted;
				StatusText = state.StatusLabel;

			}
			else
			{
				Progress = e.ProgressPercentage;
			}
		}

		/// <summary>
		/// Show the control, but honor the
		/// <see cref="DelayShowInterval"/>.
		/// </summary>
		public void DelayShow()
		{
			// This creates the control, but doesn't
			// show it; you can't use CreateControl()
			// here, because it will return because
			// the control is not visible
			CreateHandle();
		}

		/// <summary>
		/// Close the dialog, ignoring cancel status
		/// </summary>
		public void ForceClose()
		{
			_isClosing = true;
			Close();
		}

		/// <summary>
		/// Raised when the cancel button is clicked
		/// </summary>
		public event EventHandler CancelRequested;

		/// <summary>
		/// Raises the cancelled event
		/// </summary>
		/// <param name="e">Event data</param>
		protected virtual void OnCancelled( EventArgs e )
		{
			EventHandler cancelled = CancelRequested;
			if( cancelled != null )
			{
				cancelled( this, e );
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// Custom handle creation code
		/// </summary>
		/// <param name="e">Event data</param>
		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated (e);
			if( !_showOnce )
			{
				// First, we don't want this to happen again
				_showOnce = true;
				// Then, start the timer which will determine whether
				// we are going to show this again
				_showWindowIfTakingLongTimeTimer.Start();
			}
		}

		/// <summary>
		/// Custom close handler
		/// </summary>
		/// <param name="e">Event data</param>
		protected override void OnClosing(CancelEventArgs e)
		{
			if( !_isClosing )
			{
				e.Cancel = true;
				_cancelButton.PerformClick();
			}
			base.OnClosing( e );
		}


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProgressDialog));
			this._statusLabel = new System.Windows.Forms.Label();
			this._progressBar = new System.Windows.Forms.ProgressBar();
			this._cancelButton = new System.Windows.Forms.Button();
			this._progressLabel = new System.Windows.Forms.Label();
			this._showWindowIfTakingLongTimeTimer = new System.Windows.Forms.Timer(this.components);
			this._progressTimer = new System.Windows.Forms.Timer(this.components);
			this._overviewLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			// _statusLabel
			//
			this._statusLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			resources.ApplyResources(this._statusLabel, "_statusLabel");
			this._statusLabel.Name = "_statusLabel";
			//
			// _progressBar
			//
			resources.ApplyResources(this._progressBar, "_progressBar");
			this._progressBar.Name = "_progressBar";
			//
			// _cancelButton
			//
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			resources.ApplyResources(this._cancelButton, "_cancelButton");
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.Click += new System.EventHandler(this.OnCancelButton_Click);
			//
			// _progressLabel
			//
			this._progressLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			resources.ApplyResources(this._progressLabel, "_progressLabel");
			this._progressLabel.Name = "_progressLabel";
			//
			// _showWindowIfTakingLongTimeTimer
			//
			this._showWindowIfTakingLongTimeTimer.Interval = 2000;
			this._showWindowIfTakingLongTimeTimer.Tick += new System.EventHandler(this.OnTakingLongTimeTimerClick);
			//
			// _progressTimer
			//
			this._progressTimer.Enabled = true;
			this._progressTimer.Interval = 1000;
			this._progressTimer.Tick += new System.EventHandler(this.progressTimer_Tick);
			//
			// _overviewLabel
			//
			this._overviewLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
			resources.ApplyResources(this._overviewLabel, "_overviewLabel");
			this._overviewLabel.Name = "_overviewLabel";
			//
			// ProgressDialog
			//
			resources.ApplyResources(this, "$this");
			this.ControlBox = false;
			this.Controls.Add(this._overviewLabel);
			this.Controls.Add(this._progressLabel);
			this.Controls.Add(this._cancelButton);
			this.Controls.Add(this._progressBar);
			this.Controls.Add(this._statusLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ProgressDialog";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Shown += new System.EventHandler(this.OnShow);
			this.ResumeLayout(false);

		}
		#endregion


		private void OnTakingLongTimeTimerClick(object sender, System.EventArgs e)
		{
			// Show the window now the timer has elapsed, and stop the timer
			_showWindowIfTakingLongTimeTimer.Stop();
			this.Show();
		}

		private void OnCancelButton_Click(object sender, System.EventArgs e)
		{
			// Prevent further cancellation
			_cancelButton.Enabled = false;
			_progressTimer.Stop();
			_progressLabel.Text =  "Canceling...";
			// Tell people we're canceling
			OnCancelled( e );
			if (_backgroundWorker != null)
			{
				_backgroundWorker.CancelAsync();
			}
		}

		private void progressTimer_Tick(object sender, System.EventArgs e)
		{
			int range = _progressBar.Maximum - _progressBar.Minimum;
			if( range <= 0 )
			{
				return;
			}
			if( _progressBar.Value <= 0 )
			{
				return;
			}
			TimeSpan elapsed = DateTime.Now - _startTime;
			double estimatedSeconds = (elapsed.TotalSeconds * (double) range) / (double)_progressBar.Value;
			TimeSpan estimatedToGo = new TimeSpan(0,0,0,(int)(estimatedSeconds - elapsed.TotalSeconds),0);
//			_progressLabel.Text = String.Format(
//				System.Globalization.CultureInfo.CurrentUICulture,
//                "Elapsed: {0} Remaining: {1}",
//				GetStringFor(elapsed),
//				GetStringFor(estimatedToGo) );
			_progressLabel.Text = String.Format(
				System.Globalization.CultureInfo.CurrentUICulture,
				"{0}",
				//GetStringFor(elapsed),
				GetStringFor(estimatedToGo));
		}

		private static string GetStringFor( TimeSpan span )
		{
			if( span.TotalDays > 1 )
			{
				return string.Format(System.Globalization.CultureInfo.CurrentUICulture, "{0} day {1} hour", span.Days, span.Hours);
			}
			else if( span.TotalHours > 1 )
			{
				return string.Format(System.Globalization.CultureInfo.CurrentUICulture, "{0} hour {1} min", span.Hours, span.Minutes);
			}
			else if( span.TotalMinutes > 1 )
			{
				return string.Format(System.Globalization.CultureInfo.CurrentUICulture, "{0} min {1}s", span.Minutes, span.Seconds);
			}
			return string.Format( System.Globalization.CultureInfo.CurrentUICulture, "{0}s", span.Seconds );
		}

		private static string GetResourceString( string name )
		{
			System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager( "MultiThreadProgress.Strings", typeof( ProgressDialog ).Assembly );
			return resourceManager.GetString( name, System.Globalization.CultureInfo.CurrentUICulture );
		}

		public void OnNumberOfStepsCompletedChanged(object sender, EventArgs e)
		{
			this.Progress = ((ProgressState) sender).NumberOfStepsCompleted;
			//in case there is no event pump showing us (mono-threaded)
			progressTimer_Tick(this, null);
			this.Refresh();
		}

		public void OnTotalNumberOfStepsChanged(object sender, EventArgs e)
		{
			this.ProgressRangeMaximum = ((ProgressState)sender).TotalNumberOfSteps;
			this.Refresh();
		}

		public void OnStatusLabelChanged(object sender, EventArgs e)
		{
			this.StatusText = ((ProgressState)sender).StatusLabel;
			this.Refresh();
		}

		private void OnShow(object sender, EventArgs e)
		{
			if (_backgroundWorker != null)
			{
				ProgressState progressState = new BackgroundWorkerState(_backgroundWorker);

				//BW uses percentages (unless it's using our custom ProgressState in the UserState member)
				ProgressRangeMinimum = 0;
				ProgressRangeMaximum = 100;

				//if the actual task can't take cancelling, the caller of this should set CanCancel to false;
				_backgroundWorker.WorkerSupportsCancellation = this.CanCancel;

				_backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(OnBackgroundWorker_ProgressChanged);
				_backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnBackgroundWorker_RunWorkerCompleted);
				_backgroundWorker.RunWorkerAsync(progressState);
			}
		}
	}
}
