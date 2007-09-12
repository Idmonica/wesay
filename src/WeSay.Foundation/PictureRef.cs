using System.ComponentModel;

namespace WeSay.Foundation
{
	public class PictureRef  : IParentable, IValueHolder<string>
	{
		private string _fileName;

		/// <summary>
		/// This "backreference" is used to notify the parent of changes.
		/// IParentable gives access to this during explicit construction.
		/// </summary>
		private WeSayDataObject _parent;

		/// <summary>
		/// For INotifyPropertyChanged
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#region IParentable Members

		public WeSayDataObject Parent
		{
			set
			{
				_parent = value;
			}
		}

		#endregion

		private void NotifyPropertyChanged()
		{
			//tell any data binding
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs("picture"));
			}

			//tell our parent

			if (_parent != null)
			{
				this._parent.NotifyPropertyChanged("picture");
			}
		}

		public string Value
		{
			get
			{
				return this._fileName;
			}
			set
			{
				this._fileName = value;
				NotifyPropertyChanged();
			}
		}
	}
}
