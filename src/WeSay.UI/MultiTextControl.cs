using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WeSay.Foundation;
using WeSay.Language;

namespace WeSay.UI
{
	public partial class MultiTextControl : TableLayoutPanel
	{
		private IList<WritingSystem> _writingSystemsForThisFIeld;
		private List<WeSayTextBox> _textBoxes;
		private bool _showAnnotationWidget;
		private CommonEnumerations.VisibilitySetting _visibility= CommonEnumerations.VisibilitySetting.Visible;
		private static int _widthForWritingSystemLabels=-1;
		private static WritingSystemCollection _allWritingSystems;
		private static Font _writingSystemLabelFont;

		public MultiTextControl() :this(null)
		{
		}

		public MultiTextControl(WritingSystemCollection allWritingSystems)
		{
			_allWritingSystems = allWritingSystems;
			this.components = new Container();
			InitializeComponent();
			_textBoxes = new List<WeSayTextBox>();
//            this.BackColor = System.Drawing.Color.Crimson;
			SuspendLayout();

			this.ParentChanged += new EventHandler(OnParentChanged);

			_writingSystemLabelFont = new Font(FontFamily.GenericSansSerif, 9);
			ColumnCount = 3;

			if (-1 == MultiTextControl.WidthForWritingSystemLabels)
			{
				//happens when this is from a hand-placed designer piece,
				//in which case we don't really care about aligning anyhow
				ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));//ws label
			}
			else
			{
				ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, MultiTextControl.WidthForWritingSystemLabels));
			}
			ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));//text
			ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));//annotation widget

			if (Environment.OSVersion.Platform != PlatformID.Unix)
			{
				SetAutoSizeToGrowAndShrink();
			}

			ResumeLayout(false);
		}

		private void SetAutoSizeToGrowAndShrink()
		{
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
		}


		///<remarks>This can't be done during construction... we have to wait until
		///we actually have a parent to do this.</remarks>
		private void OnParentChanged(object sender, EventArgs e)
		{
			if (this.Parent !=null && _visibility == CommonEnumerations.VisibilitySetting.ReadOnly)
			{
				BackColor = this.Parent.BackColor;
				foreach (WeSayTextBox box in _textBoxes)
				{
					box.BackColor = this.Parent.BackColor;
					box.TabStop = false;
				}
			}
		}


		public MultiTextControl(IList<WritingSystem> writingSystems, MultiText multiTextToCopyFormsFrom, string nameForTesting,
			bool showAnnotationWidget, WritingSystemCollection allWritingSystems, CommonEnumerations.VisibilitySetting visibility)
			: this(allWritingSystems)
		{
			Name = nameForTesting+"-mtc";
			_writingSystemsForThisFIeld = writingSystems;
			_showAnnotationWidget = showAnnotationWidget;
			_visibility = visibility;
			 BuildBoxes(multiTextToCopyFormsFrom);
		}



		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public List<WeSayTextBox> TextBoxes
		{
			get
			{
				return _textBoxes;
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public MultiText MultiText
		{
			get
			{
				//we don't have a binding that would keep an internal multitext up to date.
				//This seems cleaner and sufficient, at the moment.
				MultiText mt = new MultiText();
				foreach (WeSayTextBox box in TextBoxes )
				{
					mt.SetAlternative(box.WritingSystem.Id, box.Text);
				}
				return mt;
			}
		}

		private void BuildBoxes(MultiText multiText)
		{
			SuspendLayout();

			if (Controls.Count > 0)
			{
				Controls.Clear();
				RowCount = 0;
				RowStyles.Clear();
			}

			foreach (WritingSystem writingSystem in WritingSystemsForThisField)
			{
				RowCount++;
				RowStyles.Add(new RowStyle(SizeType.AutoSize));

				WeSayTextBox box = AddTextBox(writingSystem, multiText);

				Label label = AddWritingSystemLabel(box);
				label.Click += new EventHandler(subControl_Click);

				Controls.Add(label, 0, RowCount);
				Controls.Add(box, 1, RowCount);
				this.components.Add(box);//so it will get disposed of when we are

				if (_showAnnotationWidget) //false for ghosts
				{
					//TODO: THIS IS TRANSITIONAL CODE... AnnotationWidget should probably become a full control (or go away)
					AnnotationWidget aw =
						new AnnotationWidget(multiText, writingSystem.Id, box.Name + "-annotationWidget");
					Control annotationControl = aw.MakeControl(new Size());//p.Size);
					annotationControl.Click += new EventHandler(subControl_Click);
					annotationControl.Anchor = AnchorStyles.Right | AnchorStyles.Top;
					Controls.Add(annotationControl, 2, RowCount);

					this.components.Add(annotationControl); //so it will get disposed of when we are
				}
				//else
				//{
				//    SetColumnSpan(box, 2);
				//}
			}

			ResumeLayout(false);
		}


		protected override void OnMouseClick(MouseEventArgs e)
		{
			base.OnMouseClick(e);
			foreach (Control c in Controls)
			{
				if (c.TabStop)
				{
					FocusWithInsertionPointAtEnd(c);
					break;
				}
			}
		}

		void subControl_Click(object sender, EventArgs e)
		{
			Control c = GetControlFromPosition(1, GetRow((Control)sender));
			FocusWithInsertionPointAtEnd(c);
		}

		private static void FocusWithInsertionPointAtEnd(Control c) {
			c.Focus();
			TextBox tb = c as TextBox;
			if(tb != null)
			{
				tb.Select(1000, 0);//go to end}
			}
		}

		/// <summary>
		/// We want all the texts to line up, so we have to take into account the maximum
		/// width of all the ws's that might be on the form, not just in this multitext
		/// </summary>
		protected static int WidthForWritingSystemLabels
		{
			get
			{
				if (_widthForWritingSystemLabels == -1)
				{
					//null happens when this is from a hand-placed designer piece,
					//in which case we don't really care about aligning anyhow
					if (_allWritingSystems != null)
					{
						foreach (WritingSystem ws in _allWritingSystems.Values)
						{
							Size size = TextRenderer.MeasureText(ws.Abbreviation, _writingSystemLabelFont);

							if (size.Width > _widthForWritingSystemLabels)
							{
								_widthForWritingSystemLabels = size.Width;
							}
						}
					}
				}
				return _widthForWritingSystemLabels;
			}
		}
		static private Label AddWritingSystemLabel(WeSayTextBox box)
		{
			Label label = new Label();
			label.Text = box.WritingSystem.Abbreviation;
			label.ForeColor = Color.LightGray;
			label.Anchor = AnchorStyles.Left | AnchorStyles.Top;
			label.Font = _writingSystemLabelFont;

			label.AutoSize = true;

//            Graphics g = CreateGraphics();
//            int descent = box.Font.FontFamily.GetCellDescent(box.Font.Style);
//            int descentPixel = (int) (box.Font.Size * descent / box.Font.FontFamily.GetEmHeight(box.Font.Style));
//
//            //todo: this only takes into account the textbox descent, not the label's!
//            label.Location = new Point(0, (int) (box.Bottom -
//                                                 ( g.MeasureString(label.Text, label.Font).Height + descentPixel )) );

			//todo: switch to TextRenderer.Measure
			int labelAscentInPixels = (int)(label.Font.Size * label.Font.FontFamily.GetCellAscent(label.Font.Style) / label.Font.FontFamily.GetEmHeight(box.Font.Style));
			int contentAscentInPixels = (int)(box.Font.Size * box.Font.FontFamily.GetCellAscent(box.Font.Style) / label.Font.FontFamily.GetEmHeight(box.Font.Style));
			int howMuchFartherDownToPlaceLabelThanText = Math.Max(0, contentAscentInPixels - labelAscentInPixels);

			label.Margin = new Padding(0, box.Top + howMuchFartherDownToPlaceLabelThanText,0,0);
			return label;
		}

		private WeSayTextBox AddTextBox(WritingSystem writingSystem, MultiText multiText)
		{
			WeSayTextBox box = new WeSayTextBox(writingSystem, Name);
			box.ReadOnly = (_visibility == CommonEnumerations.VisibilitySetting.ReadOnly);
			box.Multiline = true;
			box.WordWrap = true;
			//box.Enabled = !box.ReadOnly;


			_textBoxes.Add(box);
			box.Name = Name.Replace("-mtc","") + "_" + writingSystem.Id; //for automated tests to find this particular guy
			box.Text = multiText[writingSystem.Id];

			box.Dock = DockStyle.Fill;
			box.TextChanged += new EventHandler(OnTextOfSomeBoxChanged);
			box.KeyDown += new KeyEventHandler(OnKeyDownInSomeBox);

			return box;
		}


		void OnKeyDownInSomeBox(object sender, KeyEventArgs e)
		{
		  OnKeyDown(e);
//
////            if (Environment.OSVersion.Platform != PlatformID.Unix)
////            {
////                SetSuppressKeyPress(e, true);
////            }
//            switch (e.KeyCode)
//            {
//                case Keys.Return:
//                    e.Handled = true;
//                    if (SpecialKeyPress != null)
//                    {
//                        SpecialKeyPress.Invoke(this, e.KeyCode);
//                    }
//                    break;
//
//                default:
//                    e.Handled = false;
////                    if (Environment.OSVersion.Platform != PlatformID.Unix)
////                    {
////                        SetSuppressKeyPress(e, false);
////                    }
//                    break;
//            }
		}

		void OnTextOfSomeBoxChanged(object sender, EventArgs e)
		{
			OnTextChanged(e);
			//if (this.TextChanged != null)
			//{
			//    TextChanged.Invoke(sender, e);
			//}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IList<WritingSystem> WritingSystemsForThisField
		{
			get { return _writingSystemsForThisFIeld; }
			set
			{
				_writingSystemsForThisFIeld = value;
				BuildBoxes(MultiText);
			}
		}

		public bool ShowAnnotationWidget
		{
			get { return this._showAnnotationWidget; }
			set {
				if (this._showAnnotationWidget != value)
				{
					this._showAnnotationWidget = value;
					BuildBoxes(MultiText);
				}
			}
		}

		public void ClearAllText()
		{
			foreach (WeSayTextBox box in _textBoxes)
			{
				box.Text = "";
			}
		}
	}
}
