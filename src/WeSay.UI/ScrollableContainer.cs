﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WeSay.UI
{
	public partial class ScrollableContainer : UserControl
	{
		public ScrollableContainer()
		{
			InitializeComponent();
		}

		public void ScrollAccordingToEventArgs(MouseEventArgs e)
		{
			OnMouseWheel(e);
		}

		protected override Point ScrollToControl(System.Windows.Forms.Control activeControl)
		{
			// Returning the current location prevents the panel from
			// scrolling to the active control when the panel loses and regains focus
			return DisplayRectangle.Location;
		}
	}
}
