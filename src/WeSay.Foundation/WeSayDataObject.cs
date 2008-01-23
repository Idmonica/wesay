using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using WeSay.Foundation.Options;

namespace WeSay.Foundation
{
	public interface IParentable
	{
		WeSayDataObject Parent { set; }
	}

	public interface IReceivePropertyChangeNotifications
	{
		void NotifyPropertyChanged(string property);
	}

	public interface IReferenceContainer
	{
		object Target { get; set; }
		string Key { get; set; }
	}

	public abstract class WeSayDataObject : INotifyPropertyChanged, IReceivePropertyChangeNotifications
	{
		[NonSerialized]
		private ArrayList _listEventHelpers;

		/// <summary>
		/// see comment on _parent field of MultiText for an explanation of this field
		/// </summary>
		private WeSayDataObject _parent;

		private List<KeyValuePair<string, object>> _properties;

		protected WeSayDataObject(WeSayDataObject parent)
		{
			_properties = new List<KeyValuePair<string, object>>();
			_parent = parent;
		}

		public abstract bool IsEmpty { get; }

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
			get
			{
				if (_properties == null)
				{
					_properties = new List<KeyValuePair<string, object>>();
					NotifyPropertyChanged("properties dictionary");
				}

				return _properties;
			}
		}

		public bool HasProperties
		{
			get
			{
				foreach (KeyValuePair<string, object> pair in _properties)
				{
					if (!IsPropertyEmpty(pair.Value))
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool HasPropertiesForPurposesOfDeletion
		{
			get
			{
				foreach (KeyValuePair<string, object> pair in _properties)
				{
					if (!IsPropertyEmptyForPurposesOfDeletion(pair.Value))
					{
						return true;
					}
				}
				return false;
			}
		}

		#region INotifyPropertyChanged Members

		/// <summary>
		/// For INotifyPropertyChanged
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		#endregion

		public event EventHandler EmptyObjectsRemoved = delegate { };

		/// <summary>
		/// Do the non-db40-specific parts of becoming activated
		/// </summary>
		public void FinishActivation()
		{
			EmptyObjectsRemoved = delegate { };
			WireUpEvents();
		}

		protected void WireUpList(IBindingList list, string listName)
		{
			_listEventHelpers.Add(new ListEventHelper(this, list, listName));
		}

		protected virtual void WireUpEvents()
		{
			_listEventHelpers = new ArrayList();
			PropertyChanged += OnPropertyChanged;
		}

		private void OnEmptyObjectsRemoved(object sender, EventArgs e)
		{
			// perculate up
			EmptyObjectsRemoved(sender, e);
		}

		protected void OnEmptyObjectsRemoved()
		{
			EmptyObjectsRemoved(this, new EventArgs());
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			SomethingWasModified(e.PropertyName);
		}

		public void WireUpChild(INotifyPropertyChanged child)
		{
			child.PropertyChanged += OnChildObjectPropertyChanged;
			if (child is WeSayDataObject)
			{
				((WeSayDataObject) child).EmptyObjectsRemoved += OnEmptyObjectsRemoved;
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
			int originalCount = Properties.Count;

			for (int i = originalCount - 1;i >= 0;i--)// NB: counting backwards
			{
				//trying to reproduce ws-564
				Debug.Assert(Properties.Count > i, "Likely hit the ws-564 bug.");

				if (Properties.Count <= i)
				{
					Palaso.Reporting.ErrorNotificationDialog.ReportException(new Exception(string.Format("Error (not shown to user): Number of properties was orginally {0}, is now {1}, but the index is {2}. PLEASE help us reproduce this bug.", originalCount,
													   Properties.Count, i)), null, false);
					break; // things are screwy, give up before someone gets hurt.
				}

				object property = Properties[i].Value;
				if (property is IReportEmptiness)
				{
					((IReportEmptiness) property).RemoveEmptyStuff();
				}

				if (IsPropertyEmpty(property))
				{
					Palaso.Reporting.Logger.WriteMinorEvent("Removing {0} due to emptiness.", property.ToString());
					Properties.RemoveAt(i);
				   // don't: this just makes for false modified events: NotifyPropertyChanged(property.ToString());
				}
			}
		}

		private static bool IsPropertyEmpty(object property)
		{
			if (property is MultiText)
			{
				return MultiText.IsEmpty((MultiText) property);
			}
			else if (property is OptionRef)
			{
				return ((OptionRef) property).IsEmpty;
			}
			else if (property is OptionRefCollection)
			{
				return ((OptionRefCollection) property).IsEmpty;
			}
			else if (property is IReportEmptiness)
			{
				return
						((IReportEmptiness) property).
								ShouldBeRemovedFromParentDueToEmptiness;
			}
			//            Debug.Fail("Unknown property type");
			return false; //don't throw it away if you don't know what it is
		}

		private static bool IsPropertyEmptyForPurposesOfDeletion(object property)
		{
			if (property is MultiText)
			{
				return IsPropertyEmpty(property);
			}
			else if (property is OptionRef)
			{
				return true;
			}
			else if (property is OptionRefCollection)
			{
				return ((OptionRefCollection) property).ShouldHoldUpDeletionOfParentObject;
			}
			else if (property is IReportEmptiness)
			{
				return IsPropertyEmpty(property);
			}
			return false; //don't throw it away if you don't know what it is
		}

		public void NotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		protected virtual void OnChildObjectPropertyChanged(object sender,
															PropertyChangedEventArgs
																	e)
		{
			NotifyPropertyChanged(e.PropertyName);
		}

		public TContents GetOrCreateProperty<TContents>(string fieldName)
				where TContents : class, IParentable, new()
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
		public TContents GetProperty<TContents>(string fieldName)
				where TContents : class //, IParentable
		{
			KeyValuePair<string, object> found =
					Properties.Find(
							delegate(KeyValuePair<string, object> p) { return p.Key == fieldName; });
			if (found.Key == fieldName)
			{
				//temp hack until mt's use parents for notification
				if (found.Value is MultiText)
				{
					WireUpChild((INotifyPropertyChanged) found.Value);
				}
				return found.Value as TContents;
			}
			return null;
		}

		public bool GetHasFlag(string propertyName)
		{
			FlagState flag = GetProperty<FlagState>(propertyName);
			if (flag == null)
			{
				return false;
			}
			return flag.Value;
		}

		/// <summary>
		///
		/// </summary>
		///<remarks>Seting a flag is represented by creating a property and giving it a "set"
		/// value, though that is not really meaningful (there are no other possible values).</remarks>
		/// <param name="propertyName"></param>
		public void SetFlag(string propertyName)
		{
			FlagState f = GetOrCreateProperty<FlagState>(propertyName);
			f.Value = true;
			//            KeyValuePair<FlagState, object> found = Properties.Find(delegate(KeyValuePair<FlagState, object> p) { return p.Key == propertyName; });
			//            if (found.Key == propertyName)
			//            {
			//                _properties.Remove(found);
			//            }
			//
			//            Properties.Add(new KeyValuePair<string, object>(propertyName, "set"));
		}

		/// <summary>
		///
		/// </summary>
		/// <remarks>Clearing a flag is represented by just removing the property, if it exists</remarks>
		/// <param name="propertyName"></param>
		public void ClearFlag(string propertyName)
		{
			KeyValuePair<string, object> found =
					Properties.Find(
							delegate(KeyValuePair<string, object> p) { return p.Key == propertyName; });
			if (found.Key == propertyName)
			{
				_properties.Remove(found);
			}
		}

		#region Nested type: WellKnownProperties

		public class WellKnownProperties
		{
			public static string Note = "note";
			static public bool Contains(string fieldName)
			{
				List<string> list = new List<string>(new string[] { Note });
				return list.Contains(fieldName);
			}
		} ;

		#endregion


	}

	public interface IReportEmptiness
	{
		bool ShouldHoldUpDeletionOfParentObject { get; }
		bool ShouldCountAsFilledForPurposesOfConditionalDisplay { get; }

		bool ShouldBeRemovedFromParentDueToEmptiness { get; }

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
		private readonly string _listName;
		private readonly WeSayDataObject _listOwner;

		public ListEventHelper(WeSayDataObject listOwner,
							   IBindingList list,
							   string listName)
		{
			_listOwner = listOwner;
			_listName = listName;
			list.ListChanged += OnListChanged;
			foreach (INotifyPropertyChanged x in list)
			{
				_listOwner.WireUpChild(x);
			}
		}

		private void OnListChanged(object sender, ListChangedEventArgs e)
		{
			if (e.ListChangedType == ListChangedType.ItemAdded)
			{
				IBindingList list = (IBindingList) sender;
				INotifyPropertyChanged newGuy =
						(INotifyPropertyChanged) list[e.NewIndex];
				_listOwner.WireUpChild(newGuy);
				if (newGuy is WeSayDataObject)
				{
					((WeSayDataObject) newGuy).Parent = _listOwner;
				}
			}
			_listOwner.NotifyPropertyChanged(_listName);
		}
	}
}