namespace WeSay.LexicalTools
{
	partial class EntryDetailControl
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
			this._recordsListBox = new System.Windows.Forms.ListBox();
			this._detailPanel = new WeSay.UI.DetailList();
			this.panel1 = new System.Windows.Forms.Panel();
			this.btnAdd = new System.Windows.Forms.Button();
			this.btnDelete = new System.Windows.Forms.Button();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			//
			// _recordsListBox
			//
			this._recordsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this._recordsListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._recordsListBox.FormattingEnabled = true;
			this._recordsListBox.ItemHeight = 20;
			this._recordsListBox.Location = new System.Drawing.Point(0, 5);
			this._recordsListBox.Name = "_recordsListBox";
			this._recordsListBox.Size = new System.Drawing.Size(120, 124);
			this._recordsListBox.TabIndex = 2;
			//
			// _detailPanel
			//
			this._detailPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._detailPanel.AutoScroll = true;
			this._detailPanel.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this._detailPanel.Location = new System.Drawing.Point(126, 5);
			this._detailPanel.Margin = new System.Windows.Forms.Padding(5);
			this._detailPanel.Name = "_detailPanel";
			this._detailPanel.Size = new System.Drawing.Size(367, 122);
			this._detailPanel.TabIndex = 4;
			//
			// panel1
			//
			this.panel1.Controls.Add(this.btnDelete);
			this.panel1.Controls.Add(this.btnAdd);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 135);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(493, 34);
			this.panel1.TabIndex = 7;
			//
			// btnAdd
			//
			this.btnAdd.Location = new System.Drawing.Point(18, 8);
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.Size = new System.Drawing.Size(75, 23);
			this.btnAdd.TabIndex = 6;
			this.btnAdd.Text = "New Word";
			this.btnAdd.UseVisualStyleBackColor = true;
			//
			// btnDelete
			//
			this.btnDelete.Location = new System.Drawing.Point(147, 8);
			this.btnDelete.Name = "btnDelete";
			this.btnDelete.Size = new System.Drawing.Size(75, 23);
			this.btnDelete.TabIndex = 7;
			this.btnDelete.Text = "Delete Word";
			this.btnDelete.UseVisualStyleBackColor = true;
			//
			// EntryDetailControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.panel1);
			this.Controls.Add(this._detailPanel);
			this.Controls.Add(this._recordsListBox);
			this.Name = "EntryDetailControl";
			this.Size = new System.Drawing.Size(493, 169);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListBox _recordsListBox;
		private WeSay.UI.DetailList _detailPanel;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button btnDelete;
		private System.Windows.Forms.Button btnAdd;
	}
}
