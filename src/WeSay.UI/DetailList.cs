using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using WeSay.Foundation;

namespace WeSay.UI
{
	/// <summary>
	/// This control displays a simple list of {label, control} pairs, one per row.
	/// It supports dynamically removing and inserting new rows, to support
	/// "ghost" fields
	/// </summary>
	public partial class DetailList : TableLayoutPanel
	{
		/// <summary>
		/// Can be used to track which data item the user is currently editting, to,
		/// for example, hilight that piece in a preview control
		/// </summary>
		public event EventHandler<CurrentItemEventArgs> ChangeOfWhichItemIsInFocus = delegate{};

		private int _indexOfLabel = 0;
		private int _indexOfWidget = 1;

		public DetailList()
		{
			InitializeComponent();
			SetStyle(ControlStyles.OptimizedDoubleBuffer |
					 ControlStyles.AllPaintingInWmPaint |
					 ControlStyles.UserPaint,
					 true);

			Name = "DetailList";//for  debugging
			_fadeInTimer.Enabled = false;
			_fadeInTimer.Interval = 500;


		   if (_indexOfLabel == 0)
		   {
			 ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
			 ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		   }
		   else
		   {
			 ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			 ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
		   }
		}

		protected override void OnPaddingChanged(EventArgs e)
		{
			base.OnPaddingChanged(e);
			Padding = new Padding(Math.Max(Padding.Left, 20), Padding.Top, Padding.Right, Padding.Bottom);
		}

		public Control AddWidgetRow(string label, bool isHeader, Control control)
		{
			return AddWidgetRow(label, isHeader, control, RowCount, false);
		}

		public int Count
		{
			get
			{
				return RowCount;
			}
		}

		public Control FocussedImmediateChild
		{
			get
			{
				foreach (Control child in Controls)
				{
					if (child.ContainsFocus)
					{
						return child;
					}
				}
				return null;
			}
			//set
			//{
			//    //Keep track of the active control ourselves by storing it in a private member, note that
			//    //that we only allow the active control to be set if it is actually a child of ours.
			//    if (Contains(value))
			//        _focussedImmediateChild = value;
			//}
		}

		public void Clear()
		{
			//Debug.WriteLine("VBox " + Name + "   Clearing");

			while (Controls.Count > 0)
			{
				//  Debug.WriteLine("  VBoxClear() calling dispose on " + base.Controls[0].Name);
				Controls[0].Dispose();
			}
			Controls.Clear();
			// Debug.WriteLine("VBox " + Name + "   Clearing DONE");
			RowCount = 0;
			RowStyles.Clear();
		}

		public Control AddWidgetRow(string fieldLabel, bool isHeader, Control editWidget, int insertAtRow, bool isGhostField)
		{
			//Debug.WriteLine(String.Format("AddWidgetRow({0}, header={1}, , row={2}", fieldLabel, isHeader, insertAtRow));
			RowStyles.Add(new RowStyle(SizeType.AutoSize));

			if (insertAtRow >= RowCount)
			{
			   RowCount = insertAtRow+1;
			}
			else
			{
				if(insertAtRow == -1)
				{
					insertAtRow = RowCount;
				}
				else
				{
					// move down to make space for new row
					for (int row = RowCount; row > insertAtRow; row--)
					{
						for (int col = 0; col < ColumnCount; col++)
						{
							Control c = GetControlFromPosition(col, row - 1);
							SetCellPosition(c, new TableLayoutPanelCellPosition(col, row));
							c.TabIndex = row;
						}
					}
				}
				RowCount++;
			}


			int top = 0;// AddHorizontalRule(panel, isHeader, _rowCount == 0);
			if (isHeader)
			{
				int beforeHeadingPadding = 8;
				top = beforeHeadingPadding;
			}
			Label label = new Label();
			if (isHeader)
			{
				label.Font = new Font(StringCatalog.LabelFont /* label.Font*/, FontStyle.Bold);
				label.Font = new Font(StringCatalog.LabelFont /* label.Font*/, FontStyle.Bold);
			}
			label.Font =StringCatalog.ModifyFontForLocalization(label.Font);
			label.Text = fieldLabel;
			label.Size = new Size(75, 50);
			label.AutoSize = true;
			int verticalPadding = 0;
			label.Top = verticalPadding+3+top;
			if (isGhostField)
			{
				label.ForeColor = System.Drawing.Color.Gray;
			}

			Controls.Add(label, _indexOfLabel, insertAtRow);
			editWidget.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

			editWidget.KeyDown += new KeyEventHandler(OnEditWidget_KeyDown);

			Debug.Assert(GetControlFromPosition(_indexOfWidget, insertAtRow) == null);

			//test
			editWidget.TabIndex = insertAtRow;

			// At this point, multitext controls were being displayed on the screen.
			// We weren't able to get around this by simply using SuspendLayout and ResumeLayout
			// But finally made MultiText default to have a size of 1,1.
			Controls.Add(editWidget, _indexOfWidget, insertAtRow);

		   return editWidget;
		}

		void OnEditWidget_KeyDown(object sender, KeyEventArgs e)
		{
			OnKeyDown(e);
		}

		private void _fadeInTimer_Tick(object sender, EventArgs e)
		{
			foreach (Control c in Controls)
			{
				if (c.Controls.Count < 2)
					continue;
//                WeSayTextBox tb = c.Controls[_indexOfTextBox] as WeSayTextBox;
//                if (tb == null)
//                    continue;
//
//                tb.FadeInSomeMore((Label) c.Controls[_indexOfLabel]);
			}
		}

		public void MoveInsertionPoint(int row)
		{
			if(0 > row || row >= RowCount)
			{
				throw new ArgumentOutOfRangeException("row", row, "row must be between 0 and Count-1 inclusive");
			}
//            Panel p = (Panel)ActualControls[RowToControlIndex(row)];
//            Control c = GetEditControlFromReferenceControl(p);
			Control c = GetControlFromPosition(1, row);

			WeSayTextBox tb;

			if (c is MultiTextControl)
			{
				MultiTextControl multText = (MultiTextControl)c;
				tb = multText.TextBoxes[0];
				tb.Focus();
				tb.Select(1000, 0);//go to end
			}
			else if (c is WeSayTextBox)
			{
				tb = (WeSayTextBox)c;
				tb.Focus();
				tb.Select(1000, 0);//go to end
			}
			else
			{
				c.Focus();
			}
		}

		public void OnBinding_ChangeOfWhichItemIsInFocus(object sender, CurrentItemEventArgs e)
		{
			ChangeOfWhichItemIsInFocus(sender, e);
		}

		/// <summary>
		/// for tests
		/// </summary>
		public Control GetEditControlFromRow(int row)
		{
			return GetControlFromPosition(1, row);
		}


		/// <summary>
		/// for tests
		/// </summary>
		public Label GetLabelControlFromRow(int row)
		{
			return (Label) GetControlFromPosition(0, row);
		}
	}
}
