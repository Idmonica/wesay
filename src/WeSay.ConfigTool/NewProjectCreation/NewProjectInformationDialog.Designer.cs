﻿namespace WeSay.ConfigTool.NewProjectCreation
{
	partial class NewProjectInformationDialog
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewProjectInformationDialog));
			this.btnOK = new System.Windows.Forms.Button();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this._whereLabel = new System.Windows.Forms.TextBox();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			//
			// btnOK
			//
			this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnOK.Location = new System.Drawing.Point(299, 241);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(91, 29);
			this.btnOK.TabIndex = 3;
			this.btnOK.Text = "&OK";
			this.btnOK.UseVisualStyleBackColor = true;
			//
			// linkLabel1
			//
			this.linkLabel1.AutoSize = true;
			this.linkLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.linkLabel1.LinkColor = System.Drawing.Color.Gray;
			this.linkLabel1.Location = new System.Drawing.Point(3, 59);
			this.linkLabel1.MinimumSize = new System.Drawing.Size(360, 0);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(360, 15);
			this.linkLabel1.TabIndex = 6;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "(click here if you want it somewhere else)";
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
			//
			// _whereLabel
			//
			this._whereLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._whereLabel.BackColor = System.Drawing.SystemColors.Control;
			this._whereLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._whereLabel.Location = new System.Drawing.Point(3, 3);
			this._whereLabel.Multiline = true;
			this._whereLabel.Name = "_whereLabel";
			this._whereLabel.ReadOnly = true;
			this._whereLabel.Size = new System.Drawing.Size(372, 53);
			this._whereLabel.TabIndex = 7;
			this._whereLabel.TabStop = false;
			this._whereLabel.Text = "Your project has been created here:\r\n{0}";
			//
			// textBox2
			//
			this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBox2.BackColor = System.Drawing.SystemColors.Control;
			this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBox2.Location = new System.Drawing.Point(3, 77);
			this.textBox2.Multiline = true;
			this.textBox2.Name = "textBox2";
			this.textBox2.ReadOnly = true;
			this.textBox2.Size = new System.Drawing.Size(372, 70);
			this.textBox2.TabIndex = 8;
			this.textBox2.TabStop = false;
			this.textBox2.Text = "The project is ready to use: just click on \"Open in WeSay\" button in the upper-ri" +
				"ght.  Later, you can come back here and make adjustments.";
			//
			// tableLayoutPanel1
			//
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this._whereLabel, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.linkLabel1, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.textBox2, 0, 2);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 5;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(378, 223);
			this.tableLayoutPanel1.TabIndex = 10;
			//
			// NewProjectInformationDialog
			//
			this.AcceptButton = this.btnOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this.btnOK;
			this.ClientSize = new System.Drawing.Size(402, 282);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.btnOK);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "NewProjectInformationDialog";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "New Project Information";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		protected System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.TextBox _whereLabel;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	}
}