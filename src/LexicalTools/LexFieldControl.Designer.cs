namespace WeSay.LexicalTools
{
	partial class LexFieldControl
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
			this._lexicalEntryView = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			//
			// richTextBox1
			//
			this._lexicalEntryView.BackColor = System.Drawing.SystemColors.Control;
			this._lexicalEntryView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._lexicalEntryView.Location = new System.Drawing.Point(3, 3);
			this._lexicalEntryView.Name = "richTextBox1";
			this._lexicalEntryView.ReadOnly = true;
			this._lexicalEntryView.Size = new System.Drawing.Size(464, 115);
			this._lexicalEntryView.TabIndex = 0;
			this._lexicalEntryView.Text = "";
			//
			// LexFieldControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._lexicalEntryView);
			this.Name = "LexFieldControl";
			this.Size = new System.Drawing.Size(474, 370);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RichTextBox _lexicalEntryView;
	}
}
