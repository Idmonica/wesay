using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using WeSay.Foundation;
using WeSay.Language;

namespace WeSay.Foundation
{
	public interface IParentable
	{
		WeSayDataObject Parent
		{
			set;
		}
	}

	public interface IReferenceContainer
	{
		object Target
		{
			get;
			set;
		}

	}

	public abstract class WeSayDataObject : INotifyPropertyChanged
	{
		public class WellKnownProperties
		{
			static public string Note = "note";
		} ;

		/// <summary>
		/// For INotifyPropertyChanged
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged = delegate{};
		public event EventHandler EmptyObjectsRemoved = delegate{};

		// abandoned due to db4o difficulties
		//          private  Dictionary<string,object>  _properties;

		private List<KeyValuePair<string, object>> _properties;

		/// <summary>
		/// see comment on _parent field of MultiText for an explanation of this field
		/// </summary>
		private WeSayDataObject _parent;

		protected WeSayDataObject(WeSayDataObject parent)
		{
			_properties = new List<KeyValuePair<string, object>>();
			_parent = parent;
		}

		[NonSerialized]
		private ArrayList _listEventHelpers;
//
//        [CLSCompliant(false)]
//        public void objectOnActivate(Db4objects.Db4o.IObjectContainer container)
//        {
//            container.Activate(this, int.MaxValue);
//            EmptyObjectsRemoved = delegate{};
//            WireUpEvents();
//        }

		/// <summary>
		/// Do the non-db40-specific parts of becoming activated
		/// </summary>
		public void FinishActivation()
		{
			EmptyObjectsRemoved = delegate{};
			WireUpEvents();
		}

		public abstract bool IsEmpty{get;}

		/// <summary>
		/// see comment on _parent field of MultiText for an explanation of this field
		/// </summary>
		public WeSayDataObject Parent
		{
			get { return _parent; }
			set
			{
				Debug.Assert(value != null);
				_parent = value;
			}
	   }

		public List<KeyValuePair<string, object>> Properties
		{
			get {
				if (_properties == null)
				{
					_properties = new List<KeyValuePair<string, object>>();
					NotifyPropertyChanged("properties dictionary");
				}

				return _properties;
			}
		}

		protected void WireUpList(IBindingList list, string listName)
		{
			_listEventHelpers.Add(new ListEventHelper(this, list, listName));
		}

		protected virtual void WireUpEvents()
		{
			_listEventHelpers = new ArrayList();
			PropertyChanged += new PropertyChangedEventHandler(OnPropertyChanged);
		}

		void OnEmptyObjectsRemoved(object sender, EventArgs e)
		{
			// perculate up
			EmptyObjectsRemoved(sender, e);
		}

		protected void OnEmptyObjectsRemoved() {
			EmptyObjectsRemoved(this, new EventArgs());
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			SomethingWasModified(e.PropertyName);
		}

		public void WireUpChild(INotifyPropertyChanged child)
		{
			child.PropertyChanged += new PropertyChangedEventHandler(OnChildObjectPropertyChanged);
			if (child is WeSayDataObject)
			{
			   ((WeSayDataObject)child).EmptyObjectsRemoved += new EventHandler(OnEmptyObjectsRemoved);
			}
		}

		/// <summary>
		/// called by the binding list when senses are added, removed, reordered, etc.
		/// Also called when the user types in fields, etc.
		/// </summary>
		/// <remarks>The only side effect of this should be to update the dateModified fields</remarks>
		public virtual void SomethingWasModified(string propertyModified)
		{
		   //NO: can't do this until really putting the record to bed;
			//only the display code knows when to do that.      RemoveEmptyProperties();
		}

		public virtual void CleanUpAfterEditting()
		{
			RemoveEmptyProperties();
		}

		public virtual void CleanUpEmptyObjects() {}

		/// <summary>
		/// BE CAREFUL about when this is called. Empty properties *should exist*
		/// as long as the record is being editted
		/// </summary>
		public void RemoveEmptyProperties()
		{
			// remove any custom fields that are empty
			int count = Properties.Count;

			for (int i = count - 1; i >= 0; i--)
			{
				object property = Properties[i].Value;
				if (property is IEmptinessCleanup)
				{
					((IEmptinessCleanup) property).RemoveEmptyStuff();
				}
				if (IsPropertyEmpty(property))
				{
					Properties.RemoveAt(i);
				}
			}
		}

		static private bool IsPropertyEmpty(object property)
		{
			if (property is MultiText)
			{
				return MultiText.IsEmpty((MultiText) property);
			}
			else if (property is OptionRef)
			{
				return ((OptionRef)property).IsEmpty;
			}
			else if (property is OptionRefCollection)
			{
				return ((OptionRefCollection)property).IsEmpty;
			}
//  todo we can make an IAmEmpty later for this
			else if (property is  IEmptinessCleanup)
			{
				return ((IEmptinessCleanup) property).IsEmpty;
			}
//            Debug.Fail("Unknown property type");
			return false;//don't throw it away if you don't know what it is
		}

		public bool HasProperties
		{
			get
			{
				foreach (KeyValuePair<string, object> pair in _properties)
				{
					if(!IsPropertyEmpty(pair.Value))
					{
						return true;
					}
				}
				return false;
			}
		}

		public void NotifyPropertyChanged(string propertyName)
		{
			PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		protected virtual void OnChildObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			NotifyPropertyChanged(e.PropertyName);
		}

		public TContents GetOrCreateProperty<TContents>(string fieldName) where TContents : class, IParentable, new()
		{
			TContents value = GetProperty<TContents>(fieldName);
			if (value != null)
			{
				return value;
			}

			TContents newGuy = new TContents();
			//Properties.Add(fieldName, newGuy);
			Properties.Add(new KeyValuePair<string, object>(fieldName, newGuy));
			newGuy.Parent = this;

			//temp hack until mt's use parents for notification
			if (newGuy is MultiText)
			{
				WireUpChild((INotifyPropertyChanged) newGuy);
			}

			return newGuy;
		}

		/// <summary>
		/// Will return null if not found
		/// </summary>
		/// <typeparam name="TContents"></typeparam>
		/// <returns>null if not found</returns>
		public TContents GetProperty<TContents>(string fieldName) where TContents : class//, IParentable
		{
			KeyValuePair<string, object> found = Properties.Find(delegate(KeyValuePair<string, object> p) { return p.Key == fieldName; });
			if (found.Key == fieldName)
			{
				//temp hack until mt's use parents for notification
				if (found.Value is MultiText)
				{
					WireUpChild((INotifyPropertyChanged)found.Value);
				}
				return found.Value as TContents;
			}
			return null;
		}

		public bool GetHasFlag(string propertyName)
		{
			string value =GetProperty<string>(propertyName);
			if (value == null)
				return false;
			return value == "set";
		}

		/// <summary>
		///
		/// </summary>
		///<remarks>Seting a flag is represented by creating a property and giving it a "set"
		/// value, though that is not really meaningful (there are no other possible values).</remarks>
		/// <param name="propertyName"></param>
		public void SetFlag(string propertyName)
		{
			KeyValuePair<string, object> found = Properties.Find(delegate(KeyValuePair<string, object> p) { return p.Key == propertyName; });
			if (found.Key == propertyName)
			{
				_properties.Remove(found);
			}

			Properties.Add(new KeyValuePair<string, object>(propertyName, "set"));
		}

		/// <summary>
		///
		/// </summary>
		/// <remarks>Clearing a flag is represented by just removing the property, if it exists</remarks>
		/// <param name="propertyName"></param>
		public void ClearFlag(string propertyName)
		{
			KeyValuePair<string, object> found = Properties.Find(delegate(KeyValuePair<string, object> p) { return p.Key == propertyName; });
			if (found.Key == propertyName)
			{
				_properties.Remove(found);
			}
		}
	}

	public interface IEmptinessCleanup
	{
		bool IsEmpty
		{
			get;
		}

		void RemoveEmptyStuff();
	}

	/// <summary>
	/// This class enables creating the necessary event subscriptions. It was added
	/// before we were forced to add "parent" fields to everything.  I could probably
	/// be removed now, since that field could be used by children to cause the wiring,
	/// but we are hoping that the parent field might go away with future version of db4o.
	/// </summary>
	public class ListEventHelper
	{
		private WeSayDataObject _listOwner;
		private string _listName;

		public ListEventHelper(WeSayDataObject listOwner, IBindingList list, string listName)
		{
			_listOwner = listOwner;
			_listName = listName;
			list.ListChanged += new ListChangedEventHandler(OnListChanged);
			foreach (INotifyPropertyChanged x in list)
			{
				_listOwner.WireUpChild(x);
			}
		}

		void OnListChanged(object sender, ListChangedEventArgs e)
		{
			if (e.ListChangedType == ListChangedType.ItemAdded)
			{
				IBindingList list = (IBindingList) sender;
				INotifyPropertyChanged newGuy = (INotifyPropertyChanged)list[e.NewIndex];
				_listOwner.WireUpChild(newGuy);
				if (newGuy is WeSayDataObject)
				{
					((WeSayDataObject) newGuy).Parent =  this._listOwner;
				}
			}
			_listOwner.NotifyPropertyChanged(_listName);
		}

	}
}
