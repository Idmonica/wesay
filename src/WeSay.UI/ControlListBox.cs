using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace WindowsApplication2
{
	public partial class ControlListBox : UserControl
	{
		private bool _firstOne = true;//a hack to cover my lack of understanding

		public ControlListBox()
		{
			InitializeComponent();
		}

		public void AddControlToBottom(Control control)
		{
			AddControl(control, -1);
		}

		/// <summary>
		/// Callers: consider control.AutoSizeMode = AutoSizeMode.GrowAndShrink if the control supports it.
		/// </summary>
		/// <param name="control"></param>
		/// <param name="insertAtRow"></param>
		public void AddControl(Control control, int insertAtRow)
		{
			_table.SuspendLayout();
			if (_firstOne)
			{
				_firstOne = false;
				_table.ColumnCount = 1;
				_table.Controls.Clear();
				_table.RowCount = 0;
				_table.RowStyles.Clear();
			}
			if (insertAtRow < 0)
			{
				insertAtRow = _table.RowCount;
			}
			_table.Controls.Add(control);
			_table.SetCellPosition(control, new TableLayoutPanelCellPosition(0, insertAtRow));
			_table.RowCount++;
			_table.Controls.SetChildIndex(control, insertAtRow);
			foreach (Control c in _table.Controls)
			{
				c.TabIndex = _table.Controls.GetChildIndex(c);
			}

			Layout();
			_table.ResumeLayout();
		}

		private void Layout()
		{
			float h = 0;
			_table.RowStyles.Clear();
			for (int r = 0; r < _table.RowCount; r++)
			{
				Control c = _table.GetControlFromPosition(0, r);
				RowStyle style = new RowStyle(SizeType.Absolute, c.Height + _table.Margin.Vertical);
				_table.RowStyles.Add(style);
				h += style.Height;
			}
			_table.Height = (int)h;
		 }
	}
}
