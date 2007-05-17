using System.Diagnostics;
using System.Windows.Forms;
using WeSay.UI;
using WeSay.UI.Buttons;

namespace WeSay.LexicalTools
{
	partial class MissingInfoControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && !IsDisposed)
			{
				_records.ListChanged -= OnRecordsListChanged;

				_recordsListBox.SelectedIndexChanged -= OnRecordSelectionChanged;
				_recordsListBox.Enter += _recordsListBox_Enter;
				_recordsListBox.Leave += _recordsListBox_Leave;
				_recordsListBox.DataSource = null; // without this, the currency manager keeps trying to work

				_completedRecordsListBox.SelectedIndexChanged -= OnCompletedRecordSelectionChanged;
				_completedRecordsListBox.Enter += _completedRecordsListBox_Enter;
				_completedRecordsListBox.Leave += _completedRecordsListBox_Leave;

			   // Debug.Assert(_recordsListBox.BindingContext.Contains(_records) == false);
				((CurrencyManager) _recordsListBox.BindingContext[_records]).SuspendBinding();
				_recordsListBox.BindingContext = new BindingContext();


				if (_currentRecord != null)
				{
					_currentRecord.PropertyChanged -= OnCurrentRecordPropertyChanged;
				}
			}
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MissingInfoControl));
			this._recordsListBox = new WeSayListBox();
			this._completedRecordsListBox = new WeSayListBox();
			this._completedRecordsLabel = new System.Windows.Forms.Label();
			this._entryViewControl = new WeSay.LexicalTools.EntryViewControl();
			this._btnPreviousWord = new WeSay.UI.Buttons.PreviousButton();
			this._btnNextWord = new WeSay.UI.Buttons.NextButton();
			this.labelNextHotKey = new System.Windows.Forms.Label();
			this._congratulationsControl = new WeSay.LexicalTools.CongratulationsControl();
			this.SuspendLayout();
			//
			// _recordsListBox
			//
			this._recordsListBox.Location = new System.Drawing.Point(4, 5);
			this._recordsListBox.MinimumSize = new System.Drawing.Size(0, 50);
			this._recordsListBox.Name = "_recordsListBox";
			this._recordsListBox.Size = new System.Drawing.Size(116, 170);
			this._recordsListBox.TabIndex = 1;
			//
			// _completedRecordsListBox
			//
			this._completedRecordsListBox.Location = new System.Drawing.Point(4, 220);
			this._completedRecordsListBox.MinimumSize = new System.Drawing.Size(0, 50);
			this._completedRecordsListBox.Name = "_completedRecordsListBox";
			this._completedRecordsListBox.Size = new System.Drawing.Size(116, 170);
			this._completedRecordsListBox.TabIndex = 2;
			//
			// _completedRecordsLabel
			//
			this._completedRecordsLabel.AutoSize = true;
			this._completedRecordsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._completedRecordsLabel.Location = new System.Drawing.Point(4, 200);
			this._completedRecordsLabel.Name = "_completedRecordsLabel";
			this._completedRecordsLabel.Size = new System.Drawing.Size(70, 15);
			this._completedRecordsLabel.TabIndex = 3;
			this._completedRecordsLabel.Text = "Completed:";
			//
			// _entryViewControl
			//
			this._entryViewControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._entryViewControl.AutoScroll = true;
			this._entryViewControl.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this._entryViewControl.DataSource = null;
			this._entryViewControl.Location = new System.Drawing.Point(126, 5);
			this._entryViewControl.Name = "_entryViewControl";
			this._entryViewControl.Size = new System.Drawing.Size(367, 334);
			this._entryViewControl.TabIndex = 0;
			//
			// _btnPreviousWord
			//
			this._btnPreviousWord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._btnPreviousWord.Location = new System.Drawing.Point(303, 354);
			this._btnPreviousWord.Name = "_btnPreviousWord";
			this._btnPreviousWord.Size = new System.Drawing.Size(30, 30);
			this._btnPreviousWord.TabIndex = 4;
			this._btnPreviousWord.TabStop = false;
			this._btnPreviousWord.Click += new System.EventHandler(this.OnBtnPreviousWordClick);
			//
			// _btnNextWord
			//
			this._btnNextWord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._btnNextWord.Location = new System.Drawing.Point(336, 344);
			this._btnNextWord.Name = "_btnNextWord";
			this._btnNextWord.Size = new System.Drawing.Size(50, 50);
			this._btnNextWord.TabIndex = 5;
			this._btnNextWord.TabStop = false;
			this._btnNextWord.Click += new System.EventHandler(this.OnBtnNextWordClick);
			//
			// labelNextHotKey
			//
			this.labelNextHotKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.labelNextHotKey.AutoSize = true;
			this.labelNextHotKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelNextHotKey.ForeColor = System.Drawing.Color.DarkGray;
			this.labelNextHotKey.Location = new System.Drawing.Point(387, 363);
			this.labelNextHotKey.Name = "labelNextHotKey";
			this.labelNextHotKey.Size = new System.Drawing.Size(102, 15);
			this.labelNextHotKey.TabIndex = 6;
			this.labelNextHotKey.Text = "(Page Down Key)";
			//
			// _congratulationsControl
			//
			this._congratulationsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._congratulationsControl.Location = new System.Drawing.Point(126, 9);
			this._congratulationsControl.Name = "_congratulationsControl";
			this._congratulationsControl.Size = new System.Drawing.Size(367, 370);
			this._congratulationsControl.TabIndex = 8;
			//
			// MissingInfoControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._recordsListBox);
			this.Controls.Add(this._completedRecordsListBox);
			this.Controls.Add(this._entryViewControl);
			this.Controls.Add(this._completedRecordsLabel);
			this.Controls.Add(this._btnPreviousWord);
			this.Controls.Add(this._btnNextWord);
			this.Controls.Add(this.labelNextHotKey);
			this.Controls.Add(this._congratulationsControl);
			this.Name = "MissingInfoControl";
			this.Size = new System.Drawing.Size(493, 395);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private EntryViewControl _entryViewControl;
		private WeSayListBox _recordsListBox;
		private WeSayListBox _completedRecordsListBox;
		private Label _completedRecordsLabel;
		private NextButton _btnNextWord;
		private PreviousButton _btnPreviousWord;
		private Label labelNextHotKey;
		private CongratulationsControl _congratulationsControl;

	}
}
