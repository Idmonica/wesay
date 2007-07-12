using System;
using System.Drawing;
using System.Windows.Forms;

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
			this.components = new System.ComponentModel.Container();
			this.label1 = new System.Windows.Forms.Label();
			this._indicatorPanel = new System.Windows.Forms.Panel();
			this.localizationHelper1 = new WeSay.UI.LocalizationHelper(this.components);
			((System.ComponentModel.ISupportInitialize)(this.localizationHelper1)).BeginInit();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(136, 23);
			this.label1.TabIndex = 0;
			this.label1.Text = "~Current task:";
			//
			// _indicatorPanel
			//
			this._indicatorPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._indicatorPanel.BackColor = System.Drawing.Color.Transparent;
			this._indicatorPanel.Location = new System.Drawing.Point(70, 35);
			this._indicatorPanel.Name = "_indicatorPanel";
			this._indicatorPanel.Size = new System.Drawing.Size(485, 100);
			this._indicatorPanel.TabIndex = 1;

			this._s = new ShapeControl.ShapeControl();
			this._s.TabStop = false;
			this._s.Shape = ShapeControl.ShapeType.RoundedRectangle;
			this._s.BorderStyle = System.Drawing.Drawing2D.DashStyle.Dot;
			this._s.BorderWidth = 1;
			this._s.BorderColor = System.Drawing.Color.Black;
			this._s.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(253)))), ((int)(((byte)(219)))));
			this._s.Dock = DockStyle.Fill;

			//
			// localizationHelper1
			//
			this.localizationHelper1.Parent = this;
			//
			// CurrentTaskIndicatorControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this._indicatorPanel);
			this.Controls.Add(this.label1);
			this.Controls.Add(_s);
			this.Name = "CurrentTaskIndicatorControl";
			this.Size = new System.Drawing.Size(563, 138);
//            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

//            this.SizeChanged += new System.EventHandler(this.CurrentTaskIndicatorControl_SizeChanged);
			((System.ComponentModel.ISupportInitialize)(this.localizationHelper1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel _indicatorPanel;
		private WeSay.UI.LocalizationHelper localizationHelper1;
		private ShapeControl.ShapeControl _s;
	}
}
