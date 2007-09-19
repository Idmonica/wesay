using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using WeSay.Foundation;
using WeSay.Project;
using WeSay.UI;

namespace WeSay.Setup
{
	public partial class OptionListControl : UserControl
	{
		private OptionsList _currentList;
		private Option _currentOption;
		private Field _currentField;
		private List<Option> _newlyCreatedOptions = new List<Option>();
		private bool _currentListWasModified;

		public OptionListControl()
		{
			InitializeComponent();
			this.VisibleChanged += new EventHandler(OptionListControl_VisibleChanged);

			WeSayWordsProject.Project.EditorsSaveNow += OnEditorSaveNow;
			_currentListWasModified = false;
		}

		private void OnEditorSaveNow(object sender, EventArgs e)
		{
			SaveCurrentList();
		}

		private void SaveCurrentList()
		{
			if (_currentListWasModified)
			{
				SaveEditsToCurrentItem();
				//notice that we always save to the project directory, even if we started with the
				//one in the program files directory.
				string path  = Path.Combine(WeSay.Project.WeSayWordsProject.Project.PathToWeSaySpecificFilesDirectoryInProject, _currentField.OptionsListFile);

				try
				{
					_currentList.SaveToFile(path);
				}
				catch (Exception error)
				{
					Palaso.Reporting.ErrorReport.ReportNonFatalMessage(
						"WeSay Config could not save the options list {0}.  Please make sure it is not marked as 'read-only'.  The error was: {1}",
						path, error.Message);
				}
			}
		}

		void OptionListControl_VisibleChanged(object sender, EventArgs e)
		{
			if (this.Visible && _listBox.Items.Count > 0)
			{
				//things, like active fields, may have been changed in a different tab

				if (_listBox.SelectedItem != null)
				{
					if (_listBox.SelectedIndex == 0)
					{
						_listBox.SelectedIndex = _listBox.Items.Count-1;
					}
				}
				_listBox.SelectedIndex = 0;
				AdjustLocations();

			}
		}


		private void OnLoad(object sender, EventArgs e)
		{
		   _currentField = GetFirstFieldWithOptionsList();
			if(null == _currentField)
				return;

		   _currentList =  WeSay.Project.WeSayWordsProject.Project.GetOptionsList(_currentField);
			_listDescriptionLabel.Text = String.Format(StringCatalog.Get("~This is the contents of the {0} options list, used in the {1} field."), _currentField.OptionsListFile, _currentField.FieldName);

			LoadList();
		}

		private void LoadList()
		{
			SaveCurrentList();
			_listBox.Items.Clear();
			_currentListWasModified = false;
			foreach (Option option in _currentList.Options)
			{
				_listBox.Items.Add(option.GetDisplayProxy(PreferredWritingSystem));
			}
			if (_listBox.Items.Count > 0)
			{
				_listBox.SelectedIndex = 0;
			}
			UpdateDisplay();
		}

		private string PreferredWritingSystem
		{
			get
			{
				string preferredWritingSystem = "en";
				if (_currentField.WritingSystemIds.Count > 0)
				{
					preferredWritingSystem = _currentField.WritingSystemIds[0];
				}
				return preferredWritingSystem;
			}
		}

		private Field GetFirstFieldWithOptionsList()
		{
			foreach (Field field in WeSayWordsProject.Project.DefaultViewTemplate)
			{
				if (!String.IsNullOrEmpty(field.OptionsListFile))
				{
					return field;
				}
			}
			return null;
		}

		private void OnSelectedIndexChanged(object sender, EventArgs e)
		{
			if (_listBox.SelectedIndex > -1 && ((Option.OptionDisplayProxy) _listBox.SelectedItem).UnderlyingOption != _currentOption)
			{
				SaveEditsToCurrentItem();
				Option.OptionDisplayProxy proxy = (Option.OptionDisplayProxy) _listBox.SelectedItem;
				this.splitContainer1.Panel2.Controls.Remove(this._nameMultiTextControl);

				_currentOption = proxy.UnderlyingOption;
				MultiTextControl m = new MultiTextControl(_currentField.WritingSystems,
								 _currentOption.Name,
								 _currentField.FieldName,
								 false,
								 BasilProject.Project.WritingSystems,
								 CommonEnumerations.VisibilitySetting.Visible);
				m.SizeChanged += new EventHandler(OnNameControlSizeChanged);
				m.Bounds = _nameMultiTextControl.Bounds;
				m.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

				_nameMultiTextControl = m;
				this.splitContainer1.Panel2.Controls.Add(m);

				_keyText.TextChanged -= this.OnKeyTextChanged;
				_keyText.Text = proxy.UnderlyingOption.Key;
				_keyText.TextChanged += this.OnKeyTextChanged;

				foreach (WeSayTextBox box in m.TextBoxes)
				{
					TextBinding binding = new TextBinding(_currentOption.Name, box.WritingSystem.Id, box);
					//hooking on to this is more reliable, sequence-wise, than directly wiring to m.TextChanged
					binding.DataTarget.PropertyChanged += new PropertyChangedEventHandler(DataTarget_PropertyChanged);
				}
				_keyText.Left = _nameMultiTextControl.Left;
				UpdateDisplay();
			}
		}

		void OnNameControlSizeChanged(object sender, EventArgs e)
		{
			 AdjustLocations();
		}

		private void AdjustLocations()
		{
			_keyText.Left = _nameMultiTextControl.Left;
			_keyText.Top = _nameMultiTextControl.Bottom + 20;
			_keyText.Width = _nameMultiTextControl.Width;
			_keyLabel.Top = _keyText.Top;
		}

		private void SaveEditsToCurrentItem()
		{
			if (_currentOption == null)
			{
				return;
			}
			_currentOption.Key = _keyText.Text;
		}

		void DataTarget_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			_keyText.Text = _currentOption.Key; //will automatically change as we type, if the key isn't set yet

			//this weirdness is because I couldn't get the list item to update
			// with any of the normal, documented means (e.g. _listBox.Text)
			_listBox.Items[_listBox.SelectedIndex] = _listBox.SelectedItem;
			UserModifiedList();
		}

		private void _btnAdd_Click(object sender, EventArgs e)
		{
			Option newOption = new Option();
			_newlyCreatedOptions.Add(newOption);
			_currentList.Options.Add(newOption);

			int index =_listBox.Items.Add(newOption.GetDisplayProxy(PreferredWritingSystem));
			_listBox.SelectedIndex = index;
			UpdateDisplay();
			_nameMultiTextControl.TextBoxes[0].Focus();
			UserModifiedList();
		}

		private void _btnDelete_Click(object sender, EventArgs e)
		{
			_currentList.Options.Remove(_currentOption);
			_listBox.Items.Remove(_listBox.SelectedItem);

			if(_listBox.Items.Count>0)
			{
				_listBox.SelectedIndex = 0;
			}
			UpdateDisplay();
			UserModifiedList();
		}

		private void UserModifiedList()
		{
			_currentListWasModified = true;
		}

		private void _btnEditList_Click(object sender, EventArgs e)
		{

		}

		private void UpdateDisplay()
		{
			_btnDelete.Enabled = _listBox.SelectedItem != null;
			_keyText.Enabled = _newlyCreatedOptions.Contains(_currentOption);
			_nameMultiTextControl.Visible = _listBox.SelectedItem != null;
			_keyText.Visible = _nameMultiTextControl.Visible;

		}

		private void OnKeyTextChanged(object sender, EventArgs e)
		{
			UserModifiedList();
		}

	}
}
