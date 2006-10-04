using System;
using System.Drawing;
using System.Windows.Forms;
using WeSay.UI;

namespace WeSay.CommonTools
{
	public partial class PictureControl : UserControl, ITask
	{
		private string _label;
		private string _description;
		private bool _isActive;

		public PictureControl(string label, string description, string pictureFilePath)
		{
			_label = label;
			_description = description;
			InitializeComponent(new Bitmap(pictureFilePath));
		}

		#region ITask Members

		public void Activate()
		{
			if (IsActive)
			{
				throw new InvalidOperationException("Activate should not be called when object is active.");
			}

			_isActive = true;
		}

		public void Deactivate()
		{
			if (!IsActive)
			{
				throw new InvalidOperationException("Deactivate should only be called once after Activate.");
			}
			_isActive = false;

		}

		public bool IsActive
		{
			get
			{
				return this._isActive;
			}
		}

		public string Label
		{
			get
			{
				return StringCatalog.Get(_label);
			}
		}

		public Control Control
		{
			get
			{
				return this;
			}
		}

		public string Description
		{
			get
			{
				return StringCatalog.Get(_description);
			}
		}

		public bool IsPinned
		{
			get
			{
				return false;
			}
		}

		public string Status
		{
			get
			{
				return string.Empty;
			}
		}
		#endregion
	}
}
