using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WeSay.Foundation.Options
{
	/// <summary>
	/// Used to refer to this option from a field
	/// </summary>
	public class OptionRefCollection: IParentable,
									  INotifyPropertyChanged,
									  // ICollection<string>,
									  IReportEmptiness
	{
		// private readonly List<string> _keys;
		private BindingList<OptionRef> _members;

		/// <summary>
		/// This "backreference" is used to notify the parent of changes.
		/// IParentable gives access to this during explicit construction.
		/// </summary>
		private IReceivePropertyChangeNotifications _whomToNotify;

		private BindingList<OptionRef> _optionRefProxyList = new BindingList<OptionRef>();

		public OptionRefCollection(): this(null)
		{
		}
		public OptionRefCollection(IReceivePropertyChangeNotifications whomToNotify)
		{
			_whomToNotify = whomToNotify;
			//_keys = new List<string>();
			_members = new BindingList<OptionRef>();
			_members.ListChanged += new ListChangedEventHandler(_members_ListChanged);
		}

		void _members_ListChanged(object sender, ListChangedEventArgs e)
		{
			NotifyPropertyChanged();
		}

		public bool IsEmpty
		{
			get { return _members.Count == 0; }
		}

		#region ICollection<string> Members

//        void ICollection<string>.Add(string key)
//        {
//            if (Keys.Contains(key))
//            {
//                throw new ArgumentOutOfRangeException("key", key,
//                        "OptionRefCollection already contains that key");
//            }
//
//            Add(key);
//        }

		private OptionRef FindByKey(string key)
		{
			foreach (OptionRef _member in _members)
			{
				if(_member.Key == key)
				{
					return _member;
				}

			}
			return null;
		}

		/// <summary>
		/// Removes a key from the OptionRefCollection
		/// </summary>
		/// <param name="key">The OptionRef key to be removed</param>
		/// <returns>true when removed, false when doesn't already exists in collection</returns>
		public bool Remove(string key)
		{
			OptionRef or = FindByKey(key);
			if (or!=null)
			{
				this._members.Remove(or);
				NotifyPropertyChanged();
				return true;
			}
			return false;
		}

		public bool Contains(string key)
		{
			foreach (OptionRef _member in _members)
			{
				if(_member.Key == key)
					return true;
			}
			return false;
			//return Keys.Contains(key);
		}

		public int Count
		{
			get { return _members.Count; }
		}

		public void Clear()
		{
			_members.Clear();
			NotifyPropertyChanged();
		}

//        public void CopyTo(string[] array, int arrayIndex)
//        {
//            Keys.CopyTo(array, arrayIndex);
//        }
//
//        public bool IsReadOnly
//        {
//            get { return false; }
//        }
//
//        public IEnumerator<string> GetEnumerator()
//        {
//            return Keys.GetEnumerator();
//        }
//
//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return Keys.GetEnumerator();
//        }

		#endregion

		#region INotifyPropertyChanged Members

		/// <summary>
		/// For INotifyPropertyChanged
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region IParentable Members

		public WeSayDataObject Parent
		{
			set { _whomToNotify = value; }
		}

		#endregion

		protected void NotifyPropertyChanged()
		{
			//tell any data binding
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs("option"));
				//todo
			}

			//tell our parent
			_whomToNotify.NotifyPropertyChanged("option");
		}

		/// <summary>
		/// Adds a key to the OptionRefCollection
		/// </summary>
		/// <param name="key">The OptionRef key to be added</param>
		/// <returns>true when added, false when already exists in collection</returns>
		public bool Add(string key)
		{
			if (Contains(key))
			{
				return false;
			}

			_members.Add(new OptionRef(key));
			NotifyPropertyChanged();
			return true;
		}

		/// <summary>
		/// Adds a set of keys to the OptionRefCollection
		/// </summary>
		/// <param name="keys">A set of keys to be added</param>
		public void AddRange(IEnumerable<string> keys)
		{
			bool changed = false;
			foreach (string key in keys)
			{
				if ( this.Contains(key))
				{
					continue;
				}

				Add(key);
				changed = true;
			}

			if (changed)
			{
				NotifyPropertyChanged();
			}
		}

		#region IReportEmptiness Members

		public bool ShouldHoldUpDeletionOfParentObject
		{
			get
			{
				//this one is a conundrum.  Semantic domain gathering involves making senses
				//and adding to their semantic domain collection, without (necessarily) adding
				//a definition.  We don't want this info lost just because some eager-beaver decides
				//to clean up.
				// OTOH, we would like to have this *not* prevent deletion, if it looks like
				//the user is trying to delete the sense.
				//It will take more code to have both of these desiderata at the same time. For
				//now, we'll choose the first one, in interest of not loosing data.  It will just
				//be impossible to delete such a sense until we have SD editing.
				return ShouldBeRemovedFromParentDueToEmptiness;
			}
		}

		public bool ShouldCountAsFilledForPurposesOfConditionalDisplay
		{
			get { return !(IsEmpty); }
		}

		public bool ShouldBeRemovedFromParentDueToEmptiness
		{
			get
			{
				foreach (string s in Keys)
				{
					if (!string.IsNullOrEmpty(s))
					{
						return false;   // one non-empty is enough to keep us around
					}
				}
				return true;
			}
		}

		public IEnumerable<string>Keys
		{
			get
			{
				foreach (OptionRef _member in _members)
				{
					yield return _member.Key;
				}
			}
		}

		public IBindingList Members
		{
			get { return _members; }
		}

		public string KeyAtIndex(int index)
		{
			if (index < 0)
				throw new ArgumentException("index");
			if (index >= _members.Count)
				throw new ArgumentException("index");
			return _members[index].Key;
		}

//        public IEnumerable<OptionRef> AsEnumeratorOfOptionRefs
//        {
//            get
//            {
//                foreach (string key in _keys)
//                {
//                    OptionRef or = new OptionRef();
//                    or.Value = key;
//                   yield return or;
//                }
//            }
//        }

//        public IBindingList GetConnectedBindingListOfOptionRefs()
//        {
//            foreach (string key in _keys)
//                {
//                    OptionRef or = new OptionRef();
//                    or.Key = key;
//                    or.Parent = (WeSayDataObject) _whomToNotify ;
//
//                    _optionRefProxyList.Add(or);
//                }
//
//        }

		public void RemoveEmptyStuff()
		{
			List<OptionRef> condemened = new List<OptionRef>();
			foreach (OptionRef _member in _members)
			{
				if (_member.IsEmpty)
				{
					condemened.Add(_member);
				}
			}
			foreach (OptionRef or in condemened)
			{
				this.Members.Remove(or);
			}
		}

		#endregion
	}
}