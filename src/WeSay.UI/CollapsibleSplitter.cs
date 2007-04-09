/*
 * Adapted from code taken from
 * http://www.codeproject.com/cs/miscctrl/collapsiblesplitter.asp
 * which bears the following notice:
 *

	Windows Forms Collapsible Splitter Control for .Net
	(c)Copyright 2002-2003 NJF (furty74@yahoo.com). All rights reserved.

*/

using System.Drawing.Drawing2D;

namespace WeSay.UI
{
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Windows.Forms;

	#region Enums
	/// <summary>
	/// Enumeration to sepcify the visual style to be applied to the CollapsibleSplitter control
	/// </summary>
	public enum VisualStyles
	{
		Mozilla = 0,
		XP,
		Win9x,
		DoubleDots,
		Lines
	}

	#endregion

	/// <summary>
	/// A custom collapsible splitter that can resize, hide and show associated form controls
	/// </summary>
	[ToolboxBitmap(typeof(CollapsibleSplitter))]
	[DesignerAttribute(typeof(CollapsibleSplitterDesigner))]
	public class CollapsibleSplitter : Splitter
	{
		#region Private Properties

		// declare and define some base properties
		private Control controlToHide;
		private VisualStyles visualStyle;

	private Border3DStyle border3DStyle;
	private BorderStyle borderStyle;

		private Color _backgroundColorEnd;

	private int? lastGoodSplitPosition;
	private int hiddenControlWidth;
	private int hiddenControlHeight;

	private int gripLength;
	private int minSize;

	  #endregion
	public CollapsibleSplitter()
	{
	  base.MinSize = 0;
	  lastGoodSplitPosition = null;
	  border3DStyle = Border3DStyle.Flat;
	  base.BorderStyle = BorderStyle.None;
	  BorderStyle = BorderStyle.Fixed3D;
	  gripLength = 90;
	  minSize = 25;
	}

		#region Public Properties
	//
	// Summary:
	//     Gets or sets the minimum distance that must remain between the splitter control
	//     and the container edge that the control is docked to.
	//
	// Returns:
	//     The minimum distance, in pixels, between the System.Windows.Forms.Splitter
	//     control and the container edge that the control is docked to. The default
	//     is 25.
	[Bindable(true)]
	[Category("Collapsing Options")]
	[Localizable(true)]
	[DefaultValue(25)]
	public new int MinSize
	{
	  get{ return this.minSize;}
	  set{this.minSize = value;}
	}

		/// <summary>
		/// The initial state of the Splitter. Set to True if the control to hide is not visible by default
		/// </summary>
		[Bindable(true)]
		[Category("Collapsing Options")]
		[DefaultValue("False")]
		[Description("The initial state of the Splitter. Set to True if the control to hide is not visible by default")]
		public bool IsCollapsed
		{
			get
			{
				if(this.controlToHide!= null)
					return !this.controlToHide.Visible;
				else
					return true;
			}
		}

		/// <summary>
		/// The System.Windows.Forms.Control that the splitter will collapse
		/// </summary>
		[Bindable(true)]
	  [Category("Collapsing Options")]
		[DefaultValue("")]
		[Description("The System.Windows.Forms.Control that the splitter will collapse")]
		public Control ControlToHide
		{
			get{ return this.controlToHide; }
			set{
		this.controlToHide = value;
		this.hiddenControlHeight = controlToHide.Height;
		this.hiddenControlWidth = controlToHide.Width;
	  }
		}

		/// <summary>
		/// The visual style that will be painted on the control
		/// </summary>
		[Bindable(true)]
	  [Category("Collapsing Options")]
	  [DefaultValue("VisualStyles.XP")]
		[Description("The visual style that will be painted on the control")]
		public VisualStyles VisualStyle
		{
			get{ return this.visualStyle; }
			set
			{
				this.visualStyle = value;
				this.Invalidate();
			}
		}

	/// <summary>
	/// An optional border style to paint on the control. Set to Flat for no border
	/// </summary>
	[Bindable(true)]
	[Category("Collapsing Options")]
	[DefaultValue("System.Windows.Forms.Border3DStyle.Flat")]
	[Description("An optional border style to paint on the control. Set to Flat for no border")]
	public Border3DStyle BorderStyle3D
	{
	  get
	  {
		return this.border3DStyle;
	  }
	  set
	  {
		this.border3DStyle = value;
		this.Invalidate();
	  }
	}

	/// <summary>
	/// An optional height or width of the grabber that is painted on the control.
	/// </summary>
	[Bindable(true)]
	[Category("Collapsing Options")]
	[DefaultValue("90")]
	[Description("An optional height or width of the grip that is painted on the control.")]
	public int GripLength
	  {
		get { return this.gripLength; }
		set { this.gripLength = value; }
	  }

	  #endregion

		#region Public Methods


	  public void ToggleState()
		{
			this.ToggleSplitter();
		}

		#endregion

		#region Overrides
	protected override void OnSplitterMoved(SplitterEventArgs sevent)
	{
	  base.OnSplitterMoved(sevent);
	  if(this.SplitPosition < this.MinSize)
	  {
		this.ToggleSplitter();
	  }
		else
	  {
		lastGoodSplitPosition = this.SplitPosition;
	  }
	}

	protected override void OnSplitterMoving(SplitterEventArgs sevent)
	{
	  if (lastGoodSplitPosition == null)
	  {
		lastGoodSplitPosition = this.SplitPosition;
	  }
	  base.OnSplitterMoving(sevent);
	}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			this.Invalidate();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (this.controlToHide != null)
	  {
		if (this.controlToHide.Visible)
		{
		  base.OnMouseDown(e);
		}
		else
		{
		  base.OnMouseDown(e);
		  base.OnMouseUp(e);
		}
	  }
		}

	protected override void OnResize(EventArgs e)
	{
	  this.Invalidate();
	  base.OnResize(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
			this.Cursor = Cursors.Default;

	  if (controlToHide != null)
	  {
		if (!this.controlToHide.Visible)
		{
		  this.Cursor = Cursors.Hand;
		}
		else
		{
		  if (IsSplitterVertical())
		  {
			this.Cursor = Cursors.VSplit;
		  }
		  else
		  {
			this.Cursor = Cursors.HSplit;
		  }
		}
	  }
	  base.OnMouseMove(e);
	}

	  private bool IsSplitterVertical()
	  {
		return this.Dock == DockStyle.Left || this.Dock == DockStyle.Right;
	  }

	protected override void OnMouseClick(MouseEventArgs e)
	{
	  if (controlToHide != null)
	  {
		if (!controlToHide.Visible)
		{
		  ToggleSplitter();
		  base.OnMouseClick(e);
		}
	  }
	}

	  protected override void OnMouseDoubleClick(MouseEventArgs e)
	{
	  if (controlToHide != null && controlToHide.Visible)
	  {
		ToggleSplitter();
	  }
	  base.OnMouseDoubleClick(e);
	}

		private void ToggleSplitter()
		{
	  if (controlToHide.Visible)
	  {
		this.hiddenControlHeight = controlToHide.Height;
		controlToHide.Height = 0;
		this.hiddenControlWidth = controlToHide.Width;
		controlToHide.Width = 0;
		controlToHide.Visible = false;
	  }
	  else
	  {
		controlToHide.Visible = true;
		controlToHide.Height = this.hiddenControlHeight;
		controlToHide.Width = this.hiddenControlWidth;
		this.SplitPosition = Math.Max(minSize,
									 (lastGoodSplitPosition ?? -1));
	  }
		}

		#endregion

#region Implementation

	#region Paint the control

		protected override void OnPaint(PaintEventArgs e)
		{

		  // create a Graphics object
			Graphics g = e.Graphics;

	  if(IsSplitterVertical())
		  {
		// force the width to 8px so that everything always draws correctly
		Width = Math.Max(Width, 8);
	  }
		  else
		  {
		// force the height to 8px
		Height = Math.Max(Height,8);
		  }
	  //create our offscreen bitmap
	  Bitmap b = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);
	  Graphics bg = Graphics.FromImage(b);

	  // find the rectangle for the splitter and paint it
			Rectangle r = this.ClientRectangle;

			#region Vertical Splitter
			// Check the docking style and create the control rectangle accordingly
	  if (IsSplitterVertical())
			{
				// draw the dots for our control image using a loop
				int x = r.X+((r.Width - 2) / 2) ;
		int y = r.Y + ((r.Height - gripLength) / 2);

				switch(visualStyle)
				{
		  case VisualStyles.Mozilla:
					{
			  int dotCount = gripLength / 3;
					  for (int i = 0; i < dotCount; i++)
					  {
						// light dot
						bg.DrawLine(new Pen(SystemColors.ControlLightLight), x, y + (i * 3), x + 1, y + 1 + (i * 3));
						// dark dot
						bg.DrawLine(new Pen(SystemColors.ControlDarkDark), x + 1, y + 1 + (i * 3), x + 2, y + 2 + (i * 3));
						// overdraw the background color as we actually drew 2px diagonal lines, not just dots
						bg.DrawLine(new Pen(this.BackColor), x + 2, y + 1 + (i * 3), x + 2, y + 2 + (i * 3));
					  }
					}
			break;

		  case VisualStyles.DoubleDots:
					{
			  int dotCount = gripLength / 3;
					  for (int i = 0; i < dotCount; i++)
					  {
						// light dot
						bg.DrawRectangle(new Pen(SystemColors.ControlLightLight), x, y + 1 + (i * 3), 1, 1);
						// dark dot
						bg.DrawRectangle(new Pen(SystemColors.ControlDark), x - 1, y + (i * 3), 1, 1);
						i++;
						// light dot
						bg.DrawRectangle(new Pen(SystemColors.ControlLightLight), x + 2, y + 1 + (i * 3), 1, 1);
						// dark dot
						bg.DrawRectangle(new Pen(SystemColors.ControlDark), x + 1, y + (i * 3), 1, 1);
					  }
					}
			break;

					case VisualStyles.Win9x:

						bg.DrawLine(new Pen(SystemColors.ControlLightLight), x, y, x + 2, y);
			bg.DrawLine(new Pen(SystemColors.ControlLightLight), x, y, x, y + gripLength );
			bg.DrawLine(new Pen(SystemColors.ControlDark), x + 2, y, x + 2, y + gripLength);
			bg.DrawLine(new Pen(SystemColors.ControlDark), x, y + gripLength, x + 2, y + gripLength);
						break;

		  case VisualStyles.XP:
					{
			  int dotCount = gripLength / 5;

					  for (int i = 0; i < dotCount; i++)
					  {
						// light dot
						bg.DrawRectangle(new Pen(SystemColors.ControlLight), x, y + (i * 5), 2, 2);
						// light light dot
						bg.DrawRectangle(new Pen(SystemColors.ControlLightLight), x + 1, y + 1 + (i * 5), 1, 1);
						// dark dark dot
						bg.DrawRectangle(new Pen(SystemColors.ControlDarkDark), x, y + (i * 5), 1, 1);
						// dark fill
						bg.DrawLine(new Pen(SystemColors.ControlDark), x, y + (i * 5), x, y + (i * 5) + 1);
						bg.DrawLine(new Pen(SystemColors.ControlDark), x, y + (i * 5), x + 1, y + (i * 5));
					  }
					}
			break;

					case VisualStyles.Lines:
					{
			  int lineCount = gripLength / 2;
					  for(int i=0; i < lineCount; i++)
					  {
						bg.DrawLine(new Pen(SystemColors.ControlDark), x, y + (i*2), x + 2, y + (i*2));
					  }
					}
						break;
				}
			}

				#endregion

				// Horizontal Splitter support added in v1.2
				#region Horizontal Splitter

			else
			{
				// draw the dots for our control image using a loop
		int x = r.X + ((r.Width - gripLength) / 2);
				int y = r.Y + ((r.Height - 2) / 2);

				switch(visualStyle)
				{
		  case VisualStyles.Mozilla:
					{
					  int dotCount = gripLength / 3;
					  for (int i = 0; i < dotCount; i++)
					  {
						// light dot
						bg.DrawLine(new Pen(SystemColors.ControlLightLight), x + (i * 3), y, x + 1 + (i * 3), y + 1);
						// dark dot
						bg.DrawLine(new Pen(SystemColors.ControlDarkDark), x + 1 + (i * 3), y + 1, x + 2 + (i * 3), y + 2);
						// overdraw the background color as we actually drew 2px diagonal lines, not just dots
						bg.DrawLine(new Pen(this.BackColor), x + 1 + (i * 3), y + 2, x + 2 + (i * 3), y + 2);
					  }
					}
			break;

		  case VisualStyles.DoubleDots:
					{
					  int dotCount = gripLength / 3;
					  for (int i = 0; i < dotCount; i++)
					  {
						// light dot
						bg.DrawRectangle(new Pen(SystemColors.ControlLightLight), x + 1 + (i * 3), y, 1, 1);
						// dark dot
						bg.DrawRectangle(new Pen(SystemColors.ControlDark), x + (i * 3), y - 1, 1, 1);
						i++;
						// light dot
						bg.DrawRectangle(new Pen(SystemColors.ControlLightLight), x + 1 + (i * 3), y + 2, 1, 1);
						// dark dot
						bg.DrawRectangle(new Pen(SystemColors.ControlDark), x + (i * 3), y + 1, 1, 1);
					  }
					}
			break;

					case VisualStyles.Win9x:

						bg.DrawLine(new Pen(SystemColors.ControlLightLight), x, y, x, y + 2);
			bg.DrawLine(new Pen(SystemColors.ControlLightLight), x, y, x + gripLength, y);
			bg.DrawLine(new Pen(SystemColors.ControlDark), x, y + 2, x + gripLength, y + 2);
			bg.DrawLine(new Pen(SystemColors.ControlDark), x + gripLength, y, x + gripLength, y + 2);
						break;

					case VisualStyles.XP:
					{
					  int dotCount = gripLength/5;
					  for(int i=0; i < dotCount; i++)
					  {
						// light dot
						bg.DrawRectangle(new Pen(SystemColors.ControlLight), x + (i*5), y, 2, 2 );
						// light light dot
						bg.DrawRectangle(new Pen(SystemColors.ControlLightLight), x + 1 + (i*5), y + 1, 1, 1 );
						// dark dark dot
						bg.DrawRectangle(new Pen(SystemColors.ControlDarkDark), x +(i*5), y, 1, 1 );
						// dark fill
						bg.DrawLine(new Pen(SystemColors.ControlDark), x + (i*5), y, x + (i*5) + 1, y);
						bg.DrawLine(new Pen(SystemColors.ControlDark), x + (i*5), y, x + (i*5), y + 1);
					  }
					}
						break;

		  case VisualStyles.Lines:
					{
					  int lineCount = gripLength / 2;
					  for (int i = 0; i < lineCount; i++)
					  {
						bg.DrawLine(new Pen(SystemColors.ControlDark), x + (i * 2), y, x + (i * 2), y + 2);
					  }
					}
			break;
				}
			}

				#endregion

		  g.DrawImage(b, 0, 0);
	  bg.Dispose();
	  b.Dispose();

			// dispose the Graphics object
			g.Dispose();
		}

	protected override void OnPaintBackground(PaintEventArgs e)
	{
		// draw the background color for our control image
		LinearGradientBrush brush = new LinearGradientBrush(ClientRectangle,
									  BackColor,
									  (BackColorEnd == Color.Empty)?BackColor:BackColorEnd,
									  (IsSplitterVertical())?LinearGradientMode.Horizontal:LinearGradientMode.Vertical);

		e.Graphics.FillRectangle(brush, ClientRectangle);
		Border3DStyle style = this.border3DStyle;
		switch (BorderStyle)
		{
			case BorderStyle.FixedSingle:
				style = Border3DStyle.Flat;
				break;
			case BorderStyle.None:
				return;
		}
		if (IsSplitterVertical())
		{
			ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, style, Border3DSide.Left|Border3DSide.Right);
		}
		else
		{
			ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, style, Border3DSide.Top | Border3DSide.Bottom);
		}
		return;
	}

	public override DockStyle Dock
	{
	  get
	  {
		return base.Dock;
	  }
	  set
	  {
		switch (value)
		{
		  case DockStyle.Fill:
			throw new ArgumentOutOfRangeException("Fill DockStyle not allowed on Collapsible Splitter control.");
		  case DockStyle.None:
			throw new ArgumentOutOfRangeException("None DockStyle not allowed on Collapsible Splitter control.");
		  default:
			base.Dock = value;
			break;
		}
	  }
	}

		public new BorderStyle BorderStyle
		{
			get { return this.borderStyle; }
			set { this.borderStyle = value; }
		}

		public Color BackColorEnd
		{
			get { return this._backgroundColorEnd; }
			set { this._backgroundColorEnd = value; }
		}

		#endregion

		#endregion
	}

	/// <summary>
	/// A simple designer class for the CollapsibleSplitter control to remove
	/// unwanted properties at design time.
	/// </summary>
	public class CollapsibleSplitterDesigner : System.Windows.Forms.Design.ControlDesigner
	{
		protected override void PreFilterProperties(System.Collections.IDictionary properties)
		{
			properties.Remove("IsCollapsed");
			properties.Remove("BorderStyle");
			properties.Remove("Size");
		}
	}
}
