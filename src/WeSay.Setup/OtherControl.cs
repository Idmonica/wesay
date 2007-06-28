using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using WeSay.Language;
using WeSay.Project;

namespace WeSay.Setup
{
	public partial class OtherControl : UserControl
	{
		public OtherControl()
		{
			InitializeComponent();
		}

		private void OnLoad(object sender, EventArgs e)
		{
			LoadPoFilesIntoCombo(Project.WeSayWordsProject.Project.PathToWeSaySpecificFilesDirectoryInProject);
			LoadPoFilesIntoCombo(Project.WeSayWordsProject.Project.ApplicationCommonDirectory);

			WeSayWordsProject.Project.HackedEditorsSaveNow += Project_HackedEditorsSaveNow;
			UpdateFontDisplay();
	   }

		private void LoadPoFilesIntoCombo(string directory)
		{
			foreach (string file in Directory.GetFiles(directory,"*.po"))
			{
				string selector = Path.GetFileNameWithoutExtension(file);
				_languageCombo.Items.Add(selector);
				if(Project.WeSayWordsProject.Project.StringCatalogSelector == selector)
				{
					_languageCombo.SelectedItem = selector;
				}
			}
		}

		void Project_HackedEditorsSaveNow(object owriter, EventArgs e)
		{
			XmlWriter writer = (XmlWriter) owriter;

			writer.WriteStartElement("uiOptions");
			writer.WriteAttributeString("uiLanguage", _languageCombo.SelectedItem.ToString());
			writer.WriteAttributeString("uiFont", StringCatalog.LabelFont.Name);
			writer.WriteEndElement();
		}

		private void OnChooseFont(object sender, EventArgs e)
		{
			System.Windows.Forms.FontDialog dialog = new FontDialog();
			dialog.Font = StringCatalog.LabelFont;
			dialog.ShowColor = false;
			dialog.ShowEffects = false;

			dialog.MaxSize = 12;//size is not relevant
			dialog.MinSize = 12;
			if (DialogResult.OK != dialog.ShowDialog())
			{
				return;
			}
			StringCatalog.LabelFont = dialog.Font;
			UpdateFontDisplay();
		}
		private void UpdateFontDisplay()
		{
			_fontInfoDisplay.Text = string.Format(StringCatalog.LabelFont.Name);
			//this.Invalidate();
		}

	}
}
