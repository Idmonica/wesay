namespace WeSay.CommonTools
{
	partial class CurrentTaskIndicatorControl
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
			this.label1 = new System.Windows.Forms.Label();
			this._indicatorPanel = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(4, 4);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(114, 24);
			this.label1.TabIndex = 0;
			this.label1.Text = "Current task:";
			//
			// _indicatorPanel
			//
			this._indicatorPanel.Location = new System.Drawing.Point(80, 31);
			this._indicatorPanel.Name = "_indicatorPanel";
			this._indicatorPanel.Size = new System.Drawing.Size(471, 62);
			this._indicatorPanel.TabIndex = 1;
			//
			// CurrentTaskIndicatorControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.Controls.Add(this._indicatorPanel);
			this.Controls.Add(this.label1);
			this.Name = "CurrentTaskIndicatorControl";
			this.Size = new System.Drawing.Size(563, 100);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel _indicatorPanel;
	}
}
