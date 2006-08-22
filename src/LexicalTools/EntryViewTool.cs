using System;
using System.Collections.Generic;
using Gtk;
using System.ComponentModel;
using WeSay.LexicalModel;
using WeSay.UI;
using WeSay.TreeViewIList;

namespace WeSay.LexicalTools
{
	public class EntryViewTool : WeSay.UI.ITask
	{
		private VBox _container;
		private IBindingList _records;
		private System.Windows.Forms.BindingSource _bindingSource; //could get by with CurrencyManager

		public EntryViewTool(IBindingList records)
		{
			_records = records;
			_bindingSource = new System.Windows.Forms.BindingSource(_records, null);
			_bindingSource.PositionChanged += new EventHandler(OnPositionChanged);
		}

		public void Activate()
		{
			AddToolbar();
			AddToolview();
			_container.ShowAll();
		}

		private void AddToolbar()
		{
			Gtk.Toolbar bar = new Toolbar();
			Gtk.ToolButton previous = new ToolButton(Gtk.Stock.GoBack);
			bar.Add(previous);
			previous.Clicked += new EventHandler(OnPreviousClicked);
			Gtk.ToolButton next = new ToolButton(Gtk.Stock.GoForward);
			bar.Add(next);
			next.Clicked += new EventHandler(OnNextClicked);
			_container.PackStart(bar, false, false, 0);
		}

		private void AddToolview()
		{
			HBox hbox = new HBox();
			_container.PackStart(hbox);
			AddListArea(hbox);
			AddDetailArea(hbox);
		}

		private void AddListArea(Box parent)
		{
			System.Collections.IList list = this._records;
			Gtk.ScrolledWindow scrolled = new ScrolledWindow();
			scrolled.HscrollbarPolicy = PolicyType.Never;
			scrolled.VscrollbarPolicy = PolicyType.Always;
			parent.PackStart(scrolled);

			TreeViewAdaptorIList treeview = new TreeViewAdaptorIList(list);
			treeview.GetValueStrategy = delegate(object o, int column)
				{
					LexEntry lexEntry = (LexEntry)o;
					return lexEntry.LexicalForm["th"];
				};
			treeview.Column_Types.Add(GLib.GType.String);

			Gtk.CellRendererText renderer = new Gtk.CellRendererText();
			renderer.SizePoints = 15;
			Gtk.TreeViewColumn treeViewColumn = new Gtk.TreeViewColumn("Entries", renderer, "text", 0);
			treeViewColumn.Sizing = TreeViewColumnSizing.Fixed;
			treeViewColumn.Visible = true;
			treeViewColumn.Resizable = true;
			treeViewColumn.MinWidth = 0;
			treeview.AppendColumn(treeViewColumn);
			TreeSelection selection = treeview.Selection;
			_records.IndexOf(_bindingSource.Current);
			selection.SelectPath(new TreePath("0"));
			selection.Changed += new EventHandler(OnSelectionChanged);


			treeview.FixedHeightMode = true;

			scrolled.Child = treeview;
			scrolled.ShowAll();
		}

		void OnSelectionChanged(object sender, EventArgs e)
		{
			//throw new Exception("The method or operation is not implemented.");
		}

		private void AddDetailArea(Box parent)
		{
			LexEntry record = _bindingSource.Current as LexEntry;
			if (record == null)
			{
				parent.PackStart(new Label("No Records Yet"));
				return; //what to do?
			}

			TableBuilder builder = new TableBuilder();
			LexEntryLayouter layout = new LexEntryLayouter(builder);
			layout.AddWidgets(record);

			parent.PackStart(builder.BuildTable());
		}

		private void RefreshDetailArea(Box parent)
		{
			while (parent.Children.Length != 1)
			{
				parent.Children[1].Destroy();
			}
			//Widget detailArea = parent.Children[1];
			//detailArea.Destroy();
			AddDetailArea(parent);
		}


		public void OnPreviousClicked(object sender, EventArgs e)
		{
			_bindingSource.MovePrevious();

		}
		public void OnNextClicked(object sender, EventArgs e)
		{
			_bindingSource.MoveNext();

		}
		void OnPositionChanged(object sender, EventArgs e)
		{
			RefreshDetailArea((HBox)_container.Children[1]);
			_container.ShowAll();
		}


		public void Deactivate()
		{
			while (_container.Children.Length != 0)
			{
				_container.Children[0].Destroy();
			}
		}

		public string Label
		{
			get
			{
				return "Dictionary";
			}
		}

		public VBox Container
		{
			get
			{
				return _container;
			}
			set
			{
				_container = value;
			}
		}
	}
}
