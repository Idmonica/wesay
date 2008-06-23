using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.i8n;
using WeSay.UI.Buttons;

namespace WeSay.UI
{
	[Designer(typeof (LocalizationHelperDesigner))]
	[ToolboxItem(true)]
	[ProvideProperty("ParentFo", typeof (Form))]
	public partial class LocalizationHelper : Component, ISupportInitialize, IExtenderProvider
	{
		private Dictionary<Control, TextFontPair> _originalControlProperties = new Dictionary<Control, TextFontPair>();
		private bool _alreadyChanging;
		private bool _wiredToParent;
		private Control _parent;

		public LocalizationHelper()
		{
			InitializeComponent();
		}

		public LocalizationHelper(IContainer container)
		{
			if (container != null)
			{
				container.Add(this);
			}

			InitializeComponent();
		}

		public Control Parent
		{
			get { return _parent; }
			set
			{
				if (_parent == value)
				{
					return;
				}
				if (_wiredToParent && _parent != null)
				{
					UnwireFromChildren(_parent);
				}
				_parent = value;
				if (_wiredToParent && _parent != null)
				{
					WireToChildren(_parent);
				}
			}
		}

		private void OnFontChanged(object sender, EventArgs e)
		{
			if (_alreadyChanging)
			{
				return;
			}
			Control control = (Control) sender;
			_alreadyChanging = true;
			_originalControlProperties[control].Font = control.Font;
			if (!(control is RegionButton))
				//making a big font on these things that don't have text was causing them to grow
			{
				control.Font = StringCatalog.ModifyFontForLocalization(control.Font);
			}
			_alreadyChanging = false;
		}

		private void OnTextChanged(object sender, EventArgs e)
		{
			if (_alreadyChanging)
			{
				return;
			}
			Control control = (Control) sender;

			if (control.Text.Contains("{0}"))
			{
				return; //they're going to have to format it anyways, so we can't fix it automatically
			}

			_alreadyChanging = true;
			_originalControlProperties[control].Text = control.Text;
			if (!String.IsNullOrEmpty(control.Text)) //don't try to translation, for example, buttons with no label
			{
				control.Text = StringCatalog.Get(control.Text);
			}
			_alreadyChanging = false;
		}

		private void OnControlAdded(object sender, ControlEventArgs e)
		{
			WireToControl(e.Control);
			WireToChildren(e.Control);
		}

		private void OnControlRemoved(object sender, ControlEventArgs e)
		{
			UnwireFromControl(e.Control);
			UnwireFromChildren(e.Control);
		}

		private void OnControlDisposed(object sender, EventArgs e)
		{
			Control control = (Control) sender;
			if (control != Parent)
			{
				UnwireFromControl(control);
			}
			UnwireFromChildren(control);
		}

		private void WireToChildren(Control control)
		{
			Debug.Assert(control != null);
			//Debug.WriteLine("Wiring to children of " + control.Name);
			control.ControlAdded += OnControlAdded;
			control.ControlRemoved += OnControlRemoved;
			control.Disposed += OnControlDisposed;
			foreach (Control child in control.Controls)
			{
				WireToControl(child);
				WireToChildren(child);
			}
		}

		private void WireToControl(Control control)
		{
			Debug.Assert(control != null);
			if (IsAllowedControl(control))
			{
				// Debug.WriteLine("Wiring to " + control.Name);
				control.TextChanged += OnTextChanged;
				control.FontChanged += OnFontChanged;
				_originalControlProperties.Add(control, new TextFontPair(control.Text, control.Font));
				OnTextChanged(control, null);
				OnFontChanged(control, null);
			}
		}

		private void UnwireFromChildren(Control control)
		{
			Debug.Assert(control != null);
			control.ControlAdded -= OnControlAdded;
			control.ControlRemoved -= OnControlRemoved;
			control.Disposed -= OnControlDisposed;
			//Debug.WriteLine("Unwiring from children of " + control.Name);
			foreach (Control child in control.Controls)
			{
				UnwireFromControl(child);
				UnwireFromChildren(child);
			}
		}

		private void UnwireFromControl(Control control)
		{
			Debug.Assert(control != null);
			if (IsAllowedControl(control))
			{
				control.TextChanged -= OnTextChanged;
				control.FontChanged -= OnFontChanged;
				control.Text = _originalControlProperties[control].Text;
				control.Font = _originalControlProperties[control].Font;
				_originalControlProperties.Remove(control);
			}
		}

		private static bool IsAllowedControl(Control control)
		{
			return control is Label || control is IButtonControl || control is TabControl;
		}

		#region ISupportInitialize Members

		///<summary>
		///Signals the object that initialization is starting.
		///</summary>
		///
		public void BeginInit()
		{
		}

		///<summary>
		///Signals the object that initialization is complete.
		///</summary>
		///
		public void EndInit()
		{
			if (!_wiredToParent && Parent != null)
			{
				_wiredToParent = true;
				WireToChildren(Parent);
			}
		}

		#endregion

		#region IExtenderProvider Members

		///<summary>
		///Specifies whether this object can provide its extender properties to the specified object.
		///</summary>
		///
		///<returns>
		///true if this object can provide extender properties to the specified object; otherwise, false.
		///</returns>
		///
		///<param name="extendee">The <see cref="T:System.Object"></see> to receive the extender properties. </param>
		public bool CanExtend(object extendee)
		{
			return (extendee is UserControl);
		}

		#endregion

		private class TextFontPair
		{
			private string _text;
			private Font _font;

			public TextFontPair(string text, Font font)
			{
				_text = text;
				_font = font;
			}

			public string Text
			{
				get { return _text; }
				set { _text = value; }
			}

			public Font Font
			{
				get { return _font; }
				set { _font = value; }
			}
		}
	}

	/// <summary>
	///   Designer object used to set the Parent property.
	/// </summary>
	internal class LocalizationHelperDesigner : ComponentDesigner
	{
		///   <summary>
		///   Sets the Parent property to "this" -
		///   the Form/UserControl where the component is being dropped.
		///   </summary>
		[Obsolete]
		public override void OnSetComponentDefaults()
		{
			LocalizationHelper rp = (LocalizationHelper) Component;
			rp.Parent = (Control) Component.Site.Container.Components[0];
		}
	}
}