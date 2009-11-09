using System;
using System.ComponentModel;
using Palaso.Annotations;

namespace Palaso.LexicalModel.Options
{
	/// <summary>
	/// Used to refer to this option from a field.
	/// This class just wraps the key, which is a string, with various methods to make it fit in
	/// with the system.
	/// </summary>
	public class OptionRef: Annotatable,
							IParentable,
							IValueHolder<string>,
							IReportEmptiness,
							IReferenceContainer,
							IComparable

	{
		private string _humanReadableKey;

		/// <summary>
		/// This "backreference" is used to notify the parent of changes.
		/// IParentable gives access to this during explicit construction.
		/// </summary>
		private IReceivePropertyChangeNotifications _parent;

		private bool _suspendNotification;

		public OptionRef(): this(string.Empty) {}

		public OptionRef(string key) //WeSay.Foundation.PalasoDataObject parent)
		{
			_humanReadableKey = key;
		}

		public bool IsEmpty
		{
			get { return string.IsNullOrEmpty(Value); }
		}

		#region IParentable Members

		public PalasoDataObject Parent
		{
			set { _parent = value; }
		}

		#endregion

		#region IValueHolder<string> Members

		/// <summary>
		/// For INotifyPropertyChanged
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		public string Key
		{
			get { return Value; }
			set { Value = value; }
		}

		public string Value
		{
			get { return _humanReadableKey; }
			set
			{
				if (value != null)
				{
					_humanReadableKey = value.Trim();
				}
				else
				{
					_humanReadableKey = null;
				}
				// this.Guid = value.Guid;
				NotifyPropertyChanged();
			}
		}

		// IReferenceContainer
		public string TargetId
		{
			get { return _humanReadableKey; }
			set
			{
				if (value == _humanReadableKey ||
					(value == null && _humanReadableKey == string.Empty))
				{
					return;
				}

				if (value == null)
				{
					_humanReadableKey = string.Empty;
				}
				else
				{
					_humanReadableKey = value;
				}
				NotifyPropertyChanged();
			}
		}

		public void SetTarget(Option o)
		{
			if (o == null)
			{
				TargetId = string.Empty;
			}
			else
			{
				TargetId = o.Key;
			}
		}

		#endregion

		private void NotifyPropertyChanged()
		{
			if (_suspendNotification)
			{
				return;
			}
			//tell any data binding
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs("option"));
				//todo
			}

			//tell our parent

			if (_parent != null)
			{
				_parent.NotifyPropertyChanged("option");
			}
		}

		#region IReportEmptiness Members

		public bool ShouldHoldUpDeletionOfParentObject
		{
			get { return false; }
		}

		public bool ShouldCountAsFilledForPurposesOfConditionalDisplay
		{
			get { return !IsEmpty; }
		}

		public bool ShouldBeRemovedFromParentDueToEmptiness
		{
			get { return IsEmpty; }
		}

		public void RemoveEmptyStuff()
		{
			if (Value == string.Empty)
			{
				_suspendNotification = true;
				Value = null; // better for matching 'missing' for purposes of missing info task
				_suspendNotification = false;
			}
		}

		#endregion

		public int CompareTo(object obj)
		{
			if(obj == null)
			{
				return 1;
			}
			if(!(obj is OptionRef))
			{
				throw new ArgumentException("Can not compare to anythiong but OptionRefs.");
			}
			OptionRef other = (OptionRef) obj;
			int order = Key.CompareTo(other.Key);
			return order;
		}
	}
}