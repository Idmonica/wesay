namespace WeSay.CommonTools
{
	partial class TaskIndicator
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
			this._count = new System.Windows.Forms.Label();
			this._btnName = new System.Windows.Forms.Button();
			this._textShortDescription = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			//
			// _count
			//
			this._count.AutoSize = true;
			this._count.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._count.Location = new System.Drawing.Point(-5, 10);
			this._count.Name = "_count";
			this._count.Size = new System.Drawing.Size(70, 23);
			this._count.TabIndex = 0;
			this._count.Text = "12345";
			//
			// _btnName
			//
			this._btnName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._btnName.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this._btnName.BackColor = System.Drawing.Color.AliceBlue;
			this._btnName.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._btnName.Location = new System.Drawing.Point(79, 9);
			this._btnName.Name = "_btnName";
			this._btnName.Size = new System.Drawing.Size(275, 33);
			this._btnName.TabIndex = 1;
			this._btnName.Text = "Gather from Foo words";
			this._btnName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._btnName.UseVisualStyleBackColor = false;
			this._btnName.Click += new System.EventHandler(this.OnBtnNameClick);
			//
			// _textShortDescription
			//
			this._textShortDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._textShortDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._textShortDescription.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._textShortDescription.Location = new System.Drawing.Point(79, 48);
			this._textShortDescription.Multiline = true;
			this._textShortDescription.Name = "_textShortDescription";
			this._textShortDescription.Size = new System.Drawing.Size(275, 32);
			this._textShortDescription.TabIndex = 2;
			this._textShortDescription.Text = "See words in Foo, write the same words in Boo";
			this._textShortDescription.TabStop = false;
			//
			// TaskIndicator
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this._textShortDescription);
			this.Controls.Add(this._btnName);
			this.Controls.Add(this._count);
			this.Name = "TaskIndicator";
			this.Size = new System.Drawing.Size(357, 83);
			this.BackColorChanged += new System.EventHandler(this.TaskIndicator_BackColorChanged);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label _count;
		private System.Windows.Forms.Button _btnName;
		private System.Windows.Forms.TextBox _textShortDescription;
	}
}
