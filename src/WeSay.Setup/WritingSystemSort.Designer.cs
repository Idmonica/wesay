namespace WeSay.Setup
{
	partial class WritingSystemSort
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
			this.textBoxCustomRules = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.comboBoxCultures = new System.Windows.Forms.ComboBox();
			this.textBoxSortTest = new System.Windows.Forms.TextBox();
			this.buttonSortTest = new System.Windows.Forms.Button();
			this.SuspendLayout();
			//
			// textBoxCustomRules
			//
			this.textBoxCustomRules.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxCustomRules.Location = new System.Drawing.Point(20, 37);
			this.textBoxCustomRules.Multiline = true;
			this.textBoxCustomRules.Name = "textBoxCustomRules";
			this.textBoxCustomRules.Size = new System.Drawing.Size(299, 98);
			this.textBoxCustomRules.TabIndex = 0;
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(21, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(43, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Sort as:";
			//
			// comboBoxCultures
			//
			this.comboBoxCultures.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxCultures.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this.comboBoxCultures.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this.comboBoxCultures.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxCultures.FormattingEnabled = true;
			this.comboBoxCultures.Location = new System.Drawing.Point(70, 10);
			this.comboBoxCultures.Name = "comboBoxCultures";
			this.comboBoxCultures.Size = new System.Drawing.Size(249, 21);
			this.comboBoxCultures.TabIndex = 2;
			//
			// textBox1
			//
			this.textBoxSortTest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxSortTest.Location = new System.Drawing.Point(326, 37);
			this.textBoxSortTest.Multiline = true;
			this.textBoxSortTest.Name = "textBox1";
			this.textBoxSortTest.Size = new System.Drawing.Size(100, 98);
			this.textBoxSortTest.TabIndex = 3;
			//
			// button1
			//
			this.buttonSortTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSortTest.Location = new System.Drawing.Point(326, 8);
			this.buttonSortTest.Name = "button1";
			this.buttonSortTest.Size = new System.Drawing.Size(100, 23);
			this.buttonSortTest.TabIndex = 4;
			this.buttonSortTest.Text = "Sort the following:";
			this.buttonSortTest.UseVisualStyleBackColor = true;
			this.buttonSortTest.Click += new System.EventHandler(this.buttonSortTest_Click);
			//
			// WritingSystemSort
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.buttonSortTest);
			this.Controls.Add(this.textBoxSortTest);
			this.Controls.Add(this.comboBoxCultures);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textBoxCustomRules);
			this.Name = "WritingSystemSort";
			this.Size = new System.Drawing.Size(434, 150);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textBoxCustomRules;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox comboBoxCultures;
		private System.Windows.Forms.TextBox textBoxSortTest;
		private System.Windows.Forms.Button buttonSortTest;
	}
}
