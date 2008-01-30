// Derived from code by Peter Femiani available on CodeProject http://www.codeproject.com/csharp/AutoCompleteTextBox.asp

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WeSay.Foundation;

namespace WeSay.UI.AutoCompleteTextBox
{
	/// <summary>
	/// Summary description for AutoCompleteTextBox.
	/// </summary>
	[Serializable]
	public class WeSayAutoCompleteTextBox : WeSayTextBox
	{
		public delegate object FormToObectFinderDelegate(string form);
		private FormToObectFinderDelegate _formToObectFinderDelegate;

		public event EventHandler SelectedItemChanged;
		private bool _inMidstOfSettingSelectedItem = false;
		#region EntryMode

		public enum EntryMode
		{
			Text,
			List
		}

		#endregion

		#region Members

		private IDisplayStringAdaptor _itemDisplayAdaptor= new ToStringAutoCompleteAdaptor();
		private readonly ListBox _listBox;
		private Control _popupParent;

		#endregion

		#region Properties

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public FormToObectFinderDelegate FormToObectFinder
		{
			get
			{
				return _formToObectFinderDelegate;
			}
			set
			{
				_formToObectFinderDelegate = value;
			}
		}

		private EntryMode mode = EntryMode.Text;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public EntryMode Mode
		{
			get
			{
				return this.mode;
			}
			set
			{
				this.mode = value;
			}
		}

		private IEnumerable _items = new Collection<object>();
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public IEnumerable Items
		{
			get
			{
				return this._items;
			}
			set
			{
				this._items = value;
				if(Mode == EntryMode.List)
				{
					UpdateList();
				}
			}
		}

		private AutoCompleteTriggerCollection triggers = new AutoCompleteTriggerCollection();
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public AutoCompleteTriggerCollection Triggers
		{
			get
			{
				return this.triggers;
			}
			set
			{
				this.triggers = value;
			}
		}

		private bool _autoSizePopup = true;
		[Browsable(true)]
		[Description("The width of the popup (-1 will auto-size the popup to the larger of the width of the textbox or the content of the list).")]
		public int PopupWidth
		{
			get
			{
				return this._listBox.Width;
			}
			set
			{
				if (value == -1)
				{
					_autoSizePopup = true;
					this._listBox.Width = Width;
				}
				else
				{
					_autoSizePopup = false;
					this._listBox.Width = value;
				}
			}
		}

		public BorderStyle PopupBorderStyle
		{
			get
			{
				return this._listBox.BorderStyle;
			}
			set
			{
				this._listBox.BorderStyle = value;
			}
		}

		private Point popOffset = new Point(0, 0);
		[Description("The popup defaults to the lower left edge of the textbox.")]
		public Point PopupOffset
		{
			get
			{
				return this.popOffset;
			}
			set
			{
				this.popOffset = value;
			}
		}

		private Color popSelectBackColor = SystemColors.Highlight;
		public Color PopupSelectionBackColor
		{
			get
			{
				return this.popSelectBackColor;
			}
			set
			{
				this.popSelectBackColor = value;
			}
		}

		private Color popSelectForeColor = SystemColors.HighlightText;
		public Color PopupSelectionForeColor
		{
			get
			{
				return this.popSelectForeColor;
			}
			set
			{
				this.popSelectForeColor = value;
			}
		}

		private bool triggersEnabled = true;
		protected bool TriggersEnabled
		{
			get
			{
				return this.triggersEnabled;
			}
			set
			{
				this.triggersEnabled = value;
			}
		}

		//nb: if we used an interface (e.g. IFilter) rather than a delegate, then the adaptor part wouldn't have to be
		// part of the interface, where it doesn't really fit. It would instead be added to the constructor of the filterer,
		// if appropriate.
		public delegate IEnumerable ItemFilterDelegate(string text, IEnumerable items, IDisplayStringAdaptor adaptor);
		private ItemFilterDelegate _itemFilterDelegate;
		private object _selectedItem;
		private readonly ToolTip _toolTip;
		private object _previousToolTipTarget=null;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ItemFilterDelegate ItemFilterer
		{
			get
			{
				return this._itemFilterDelegate;
			}
			set
			{
				if(value == null)
				{
					throw new ArgumentNullException();
				}
				this._itemFilterDelegate = value;
			}
		}

		[Browsable(true)]
		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				TriggersEnabled = false;
				base.Text = value;
				TriggersEnabled = true;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IDisplayStringAdaptor ItemDisplayStringAdaptor
		{
			get
			{
				return _itemDisplayAdaptor;
			}
			set
			{
				_itemDisplayAdaptor = value;
			}
		}

		public object SelectedItem
		{
			get
			{
				return _selectedItem;
			}


			set
			{
				if(_inMidstOfSettingSelectedItem)
					return;


				if(_selectedItem == value)
					return;
				_inMidstOfSettingSelectedItem = true;

				_selectedItem = value;
				if (value != null)
				{
					if (_itemDisplayAdaptor != null)
					{
						Text = _itemDisplayAdaptor.GetDisplayLabel(value);
						//keep cursor form jumping to start when user typed in
						//a matched value that varied by case
						SelectionStart = Text.Length;
					}
					else
					{
						Text = value.ToString();
					}
				}

				if (SelectedItemChanged != null)
				{
					SelectedItemChanged.Invoke(this, null);
				}
				//Debug.WriteLine("AutomCOmplete Set selectedItem(" + value + ") now " + _selectedItem);
				_inMidstOfSettingSelectedItem = false;
			}
		}

		internal ListBox FilteredChoicesListBox
		{
			get { return _listBox; }
		}

		#endregion

		public WeSayAutoCompleteTextBox()
		{
			if (DesignMode)
				return;

			_formToObectFinderDelegate = DefaultFormToObjectFinder;
			_itemFilterDelegate = FilterList;

			Leave += Popup_Deactivate;

			_toolTip = new ToolTip();
			MouseHover += OnMouseHover;

			// Create the list box that will hold matching items
			this._listBox = new ListBox();
			this._listBox.MaximumSize = new Size(400,100);
			this._listBox.Cursor = Cursors.Hand;
			this._listBox.BorderStyle = BorderStyle.FixedSingle;
			this._listBox.SelectedIndexChanged += List_SelectedIndexChanged;
			this._listBox.Click += List_Click;
			this._listBox.MouseMove += List_MouseMove;
			this._listBox.ItemHeight = _listBox.Font.Height;
			this._listBox.Visible = false;
			this._listBox.Sorted = false;

			// Add default triggers.
			this.triggers.Add(new TextLengthTrigger(2));
			this.triggers.Add(new ShortCutTrigger(Keys.Enter, TriggerState.Select));
			this.triggers.Add(new ShortCutTrigger(Keys.Tab, TriggerState.Select));
			this.triggers.Add(new ShortCutTrigger(Keys.Control | Keys.Space, TriggerState.ShowAndConsume));
			this.triggers.Add(new ShortCutTrigger(Keys.Escape, TriggerState.HideAndConsume));
		}

		void OnMouseHover(object sender, EventArgs e)
		{
			if (_itemDisplayAdaptor == null || SelectedItem == null)
				return;

			string tip = _itemDisplayAdaptor.GetToolTip(SelectedItem);
			_toolTip.SetToolTip(this, tip);
			_toolTip.ToolTipTitle =_itemDisplayAdaptor.GetToolTipTitle(SelectedItem);;
		}

		private void List_MouseMove ( object sender, MouseEventArgs e )
		{
			if (_itemDisplayAdaptor == null)
				return;

			string tip = "";
			string tipTitle = "";

			//Get the item
			int nIdx = _listBox.IndexFromPoint(e.Location);
			if ((nIdx >= 0) && (nIdx < _listBox.Items.Count))
			{
				ItemWrapper wrapper = (ItemWrapper)_listBox.Items[nIdx];
				if(wrapper.Item == _previousToolTipTarget)//prevent flicker and unnecessary work
				{
					return;
				}
				_previousToolTipTarget = wrapper.Item;
				tip = _itemDisplayAdaptor.GetToolTip(wrapper.Item);
				tipTitle = _itemDisplayAdaptor.GetToolTipTitle(wrapper.Item);
			}
			_toolTip.SetToolTip(_listBox, tip);
			_toolTip.ToolTipTitle = tipTitle;
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			if (_listBox != null)
			{
				//NB: this height can be multiple lines, so we don't just want the Height
				//this._listBox.ItemHeight = Height;
				this._listBox.ItemHeight = _listBox.Font.Height;
			}
			if(_listBox !=null && _autoSizePopup)
			{
				if (this._listBox.Width < Width)
				{
					this._listBox.Width = Width;
				}
			}
		}

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			if (this._popupParent != null)
			{
				this._popupParent.Move -= Parent_Move;
				this._popupParent.ParentChanged -= popupParent_ParentChanged;
			}
			this._popupParent = Parent;
			if (this._popupParent != null)
			{
				while (this._popupParent.Parent != null)
				{
					this._popupParent = this._popupParent.Parent;
				}
				this._popupParent.Move += Parent_Move;
				this._popupParent.ParentChanged += popupParent_ParentChanged;
			}
		}

		void popupParent_ParentChanged(object sender, EventArgs e)
		{
			OnParentChanged(e);
		}

		void Parent_Move(object sender, EventArgs e)
		{
			HideList();
		}

		protected virtual bool DefaultCmdKey(ref Message msg, Keys keyData)
		{
			bool val = base.ProcessCmdKey (ref msg, keyData);

			if (TriggersEnabled)
			{
				switch (Triggers.OnCommandKey(keyData))
				{
					case TriggerState.ShowAndConsume:
					{
						val = true;
						ShowList();
					} break;
					case TriggerState.Show:
					{
						ShowList();
					} break;
					case TriggerState.HideAndConsume:
					{
						val = true;
						HideList();
					} break;
					case TriggerState.Hide:
					{
						HideList();
					} break;
					case TriggerState.SelectAndConsume:
					{
						if (this._listBox.Visible)
						{
							val = true;
							SelectCurrentItemAndHideList();
						}
					} break;
					case TriggerState.Select:
					{
						if (this._listBox.Visible)
						{
							SelectCurrentItemAndHideList();
						}
					} break;
					default:
						break;
				}
			}

			return val;
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			switch (keyData)
			{
				case Keys.Up:
				{
					Mode = EntryMode.List;
					if (this._listBox.Visible == false)
					{
						ShowList();
					}
					if (this._listBox.SelectedIndex > 0)
					{
						this._listBox.SelectedIndex--;
					}
					return true;
				}
				case Keys.Down:
				{
					Mode = EntryMode.List;
					if (this._listBox.Visible == false)
					{
						ShowList();
					}
					if (this._listBox.SelectedIndex < this._listBox.Items.Count - 1)
					{
						this._listBox.SelectedIndex++;
					}
					return true;
				}
				default:
				{
					return DefaultCmdKey(ref msg, keyData);
				}
			}
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged (e);

			if (TriggersEnabled)
			{
				switch (Triggers.OnTextChanged(Text))
				{
					case TriggerState.Show:
						{
							ShowList();
						}
						break;
					case TriggerState.Hide:
						{
							HideList();
						}
						break;
					default:
						{
							UpdateList();
						}
						break;
				}
			}

			SelectedItem = _formToObectFinderDelegate(Text);
		}

		/// <summary>
		/// can be replaced by something smarter, using the FormToObectFinderDelegate
		/// </summary>
		/// <param name="form"></param>
		private object DefaultFormToObjectFinder(string form)
		{
			foreach (object item in _items)
			{
				if (_itemDisplayAdaptor.GetDisplayLabel(item) == form)
				{
					return item;
				}
			}
			return null;
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus (e);

			if (!(Focused || this._listBox.Focused))
			{
				HideList();
			}
		}

		protected virtual void SelectCurrentItem()
		{
			if (this._listBox.SelectedIndex == -1)
			{
				return;
			}

			Text = this._listBox.SelectedItem.ToString();
			SelectedItem = ((ItemWrapper) _listBox.SelectedItem).Item;
			if (Text.Length > 0)
			{
				SelectionStart = Text.Length;
			}
		}

		protected void SelectCurrentItemAndHideList()
		{
			if (this._listBox.SelectedIndex != -1)
			{
				SelectCurrentItem();
			}
			HideList();
		}

		protected virtual void ShowList()
		{
			if (this._listBox.Visible == false)
			{
				UpdateList();
				Form form = FindForm();
				Point parentPointOnScreen = Parent.PointToClient(form.Location);
				Point formPointOnScreen = form.PointToClient(form.Location);
				Point offset = new Point(formPointOnScreen.X - parentPointOnScreen.X,
										 formPointOnScreen.Y - parentPointOnScreen.Y);
				Point p = Location;
				p.X += offset.X;
				p.Y += offset.Y;
				p.X += PopupOffset.X;
				p.Y += Height + PopupOffset.Y;
				this._listBox.Location = p;
				if (this._listBox.Items.Count > 0)
				{
					if (!form.Controls.Contains(this._listBox))
					{
						form.Controls.Add(this._listBox);
					}

					this._listBox.BringToFront();
					this._listBox.Visible = true;
					this._listBox.Location = p;
				}
			}
			else
			{
				UpdateList();
			}
		}

		protected virtual void HideList()
		{
			Mode = EntryMode.Text;
			this._listBox.Visible = false;
		}

		class ItemWrapper
		{
			private object _item;
			private readonly string _label;

			public ItemWrapper(object item, string label)
			{
				Item = item;
				_label = label;
			}

			public object Item
			{
				get
				{
					return _item;
				}
				set
				{
					_item = value;
				}
			}

			public override string ToString()
			{
				return _label;
			}
		}

		protected virtual void UpdateList()
		{
			this._listBox.BeginUpdate();
			this._listBox.Items.Clear();

			//hatton experimental:
			if (string.IsNullOrEmpty(Text))
				return;
			//end hatton experimental

			this._listBox.Font = Font;
			this._listBox.ItemHeight = _listBox.Font.Height;

			int maxWidth = Width;
			using (Graphics g = (_autoSizePopup)?CreateGraphics():null)
			{
				foreach (object item in ItemFilterer.Invoke(Text, Items, ItemDisplayStringAdaptor))
				{
					string label = ItemDisplayStringAdaptor.GetDisplayLabel(item);
					this._listBox.Items.Add(new ItemWrapper(item, label));
					if (_autoSizePopup)
					{
					  maxWidth = Math.Max(maxWidth, MeasureItem(g, label).Width);
					}
				}
			}
			this._listBox.EndUpdate();

			if (this._listBox.Items.Count == 0)
			{
				HideList();
			}
			else
			{
				int visItems = this._listBox.Items.Count;
				if (visItems > 8)
					visItems = 8;

				this._listBox.Height = (visItems * this._listBox.ItemHeight) + 4;

				switch (BorderStyle)
				{
					case BorderStyle.FixedSingle:
					{
						this._listBox.Height += 2;
						break;
					}
					case BorderStyle.Fixed3D:
					{
						this._listBox.Height += 4;
						break;
					}
				}

				if (_autoSizePopup)
				{
					bool hasScrollBar = visItems < this._listBox.Items.Count;
					_listBox.Width = maxWidth + (hasScrollBar?10:0); // in theory this shouldn't be needed
				}
				this._listBox.RightToLeft = RightToLeft;
			}
		}

		private Size MeasureItem(IDeviceContext dc, string s) {
				TextFormatFlags flags = TextFormatFlags.Default |
										TextFormatFlags.NoClipping;
				if (WritingSystem != null && WritingSystem.RightToLeft)
				{
					flags |= TextFormatFlags.RightToLeft;
				}
				return TextRenderer.MeasureText(dc, s, _listBox.Font,
											   new Size(int.MaxValue, _listBox.ItemHeight),
											   flags);
		}

		private static IEnumerable FilterList(string text, IEnumerable items, IDisplayStringAdaptor adaptor)
		{
			ICollection<object> newList = new Collection<object>();

			foreach (object item in items)
			{
				string label = adaptor.GetDisplayLabel(item);
				if (label.ToLower().StartsWith(text.ToLower()))
				{
					newList.Add(item);
					break;
				}
			}
			return newList;
		}

		private void List_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (Mode != EntryMode.List)
			{
				SelectCurrentItemAndHideList();
			}
			else
			{
				SelectCurrentItem();
			}
		}

		public event EventHandler AutoCompleteChoiceSelected = delegate {};

		private void List_Click(object sender, EventArgs e)
		{
			SelectCurrentItemAndHideList();
			AutoCompleteChoiceSelected.Invoke(this, new EventArgs());
		}

		private void Popup_Deactivate(object sender, EventArgs e)
		{
			if (!(Focused || this._listBox.Focused))
			{
				HideList();
			}
		}
	}
}
