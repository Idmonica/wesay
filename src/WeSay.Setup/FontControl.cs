using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using WeSay.Language;

namespace WeSay.Setup
{
	public partial class FontControl : UserControl
	{
		private WritingSystem _writingSystem;

		public FontControl()
		{
			InitializeComponent();
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public WritingSystem WritingSystem
		{
			get { return this._writingSystem; }
			set
			{
				this._writingSystem = value;

				UpdateFontDisplay();
			}
		}

		private void _btnFont_Click(object sender, EventArgs e)
		{
			_fontDialog.Font = _writingSystem.Font;
			_fontDialog.ShowColor = false;
			_fontDialog.ShowEffects = false;
			if (DialogResult.OK != _fontDialog.ShowDialog())
			{
				return;
			}
			_writingSystem.Font = _fontDialog.Font;
			UpdateFontDisplay();
		  }

		private void UpdateFontDisplay()
		{
			_fontInfoDisplay.Text =
				string.Format("{0}, {1}", this.WritingSystem.Font.Name, Math.Round(this.WritingSystem.Font.Size));
			_sampleTextBox.WritingSystem =WritingSystem;
			this.Invalidate();
		}
	}
}
