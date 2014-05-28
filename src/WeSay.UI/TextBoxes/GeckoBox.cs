﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Gecko;
using Gecko.DOM;
using Palaso.WritingSystems;
using WeSay.LexicalModel.Foundation;

namespace WeSay.UI.TextBoxes
{
	public partial class GeckoBox : GeckoBase, IWeSayTextBox, IControlThatKnowsWritingSystem
	{
		private string _pendingHtmlLoad;
		private bool _keyPressed;
		private GeckoDivElement _divElement;
		private EventHandler _textChangedHandler;

		public GeckoBox()
		{
			InitializeComponent();

			_keyPressed = false;

			var designMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
			if (designMode)
				return;

			Debug.WriteLine("New GeckoBox");

			_textChangedHandler = new EventHandler(OnTextChanged);
			this.TextChanged += _textChangedHandler;
		}

		public GeckoBox(IWritingSystemDefinition ws, string nameForLogging)
			: this()
		{
			_nameForLogging = nameForLogging;
			if (_nameForLogging == null)
			{
				_nameForLogging = "??";
			}
			Name = _nameForLogging;
			WritingSystem = ws;
		}


		protected override void Closing()
		{
			this.TextChanged -= _textChangedHandler;
			_textChangedHandler = null;
			_divElement = null;
			base.Closing();
		}

		/// <summary>
		/// called when the client changes our Control.Text... we need to them move that into the html
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnTextChanged(object sender, EventArgs e)
		{
			SetText(Text);
			AdjustHeight();
		}

		private delegate void ChangeFocusDelegate(GeckoDivElement ctl);
		protected override void OnDomFocus(object sender, GeckoDomEventArgs e)
		{
			var content = _browser.Document.GetElementById("main");
			if (content != null)
			{
				if ((content is GeckoDivElement) && (!_inFocus))
				{
					// The following is required because we get two in focus events every time this
					// is entered.  This is normal for Gecko.  But I don't want to be constantly
					// refocussing.
					_inFocus = true;
#if DEBUG
					Debug.WriteLine("Got Focus2: " + Text);
#endif
					_divElement = (GeckoDivElement)content;
					this.BeginInvoke(new ChangeFocusDelegate(changeFocus), _divElement);
				}
			}
		}
		private void changeFocus(GeckoDivElement ctl)
		{
#if DEBUG
			Debug.WriteLine("Change Focus: " + Text);
#endif
			ctl.Focus();
		}

		protected override void OnDomClick(object sender, GeckoDomEventArgs e)
		{
#if DEBUG
			Debug.WriteLine ("Got Dom Mouse Click " + Text);
#endif
			_browser.Focus();
		}


		protected override void OnDomKeyUp(object sender, GeckoDomKeyEventArgs e)
		{
			var content = _browser.Document.GetElementById("main");
			_keyPressed = true;

			//			Debug.WriteLine(content.TextContent);
			Text = content.TextContent;
		}


		protected override void OnGeckoBox_Load(object sender, EventArgs e)
		{
			_browserIsReadyToNavigate = true;
			if (_pendingHtmlLoad != null)
			{
#if DEBUG
				Debug.WriteLine("Load: " + _pendingHtmlLoad);
#endif
				_browser.LoadHtml(_pendingHtmlLoad);
				_pendingHtmlLoad = null;
			}
			else
			{
#if DEBUG
				Debug.WriteLine ("Load: Empty Line");
#endif
				SetText(""); //make an empty, editable box
			}
		}

		private void SetText(string s)
		{
			String justification = "left";
			if (_writingSystem != null && WritingSystem.RightToLeftScript)
			{
				justification = "right";
			}

			String editable = "true";
			if (ReadOnly)
			{
				editable = "false";
			}

			Font font = WritingSystemInfo.CreateFont(_writingSystem);
			var html =
				string.Format(
					"<html><head><meta charset=\"UTF-8\"></head><body style='background:#FFFFFF' id='mainbody'><div style='min-height:15px; font-family:{0}; font-size:{1}pt; text-align:{3}' id='main' name='textArea' contentEditable='{4}'>{2}</div></body></html>",
					font.Name, font.Size.ToString(), s, justification, editable);
			if (!_browserIsReadyToNavigate)
			{
				_pendingHtmlLoad = html;
			}
			else
			{
				if (!_keyPressed)
				{
#if DEBUG
					Debug.WriteLine ("SetText: " + html);
#endif
					_browser.LoadHtml(html);
				}
				_keyPressed = false;
			}
		}

		public void SetHtml(string html)
		{
			if (!_browserIsReadyToNavigate)
			{
				_pendingHtmlLoad = html;
			}
			else
			{
#if DEBUG
				Debug.WriteLine("SetHTML: " + html);
#endif
				_browser.LoadHtml(html);
			}

		}


		public bool IsSpellCheckingEnabled { get; set; }


		protected override void OnLeave(EventArgs e)
		{
			base.OnLeave(e);

			// this.BackColor = System.Drawing.Color.White;
			ClearKeyboard();
		}
		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);
			AssignKeyboardFromWritingSystem();
		}

	}
}