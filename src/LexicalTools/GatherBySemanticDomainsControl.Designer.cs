using System.Drawing;
using System.Windows.Forms;
using WeSay.Ui.Animation;

namespace WeSay.LexicalTools
{
	partial class GatherBySemanticDomainsControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("blah");
			System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("stuff");
			this._domainName = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this._listViewWords = new System.Windows.Forms.ListBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this._instructionLabel = new System.Windows.Forms.Label();
			this._question = new System.Windows.Forms.Label();
			this._description = new System.Windows.Forms.Label();
			this._vernacularBox = new WeSay.UI.MultiTextControl();
			this._btnPrevious = new ArrowButton.ArrowButton();
			this._btnNext = new ArrowButton.ArrowButton();
			this._btnAddWord = new ArrowButton.ArrowButton();
			this._questionIndicator = new WeSay.UI.ProgressIndicator();
		  this._animatedText = new Label();
		  this._animator = new Animator();
			this.SuspendLayout();
			//
			// _domainName
			//
			this._domainName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._domainName.BackColor = System.Drawing.SystemColors.Control;
			this._domainName.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this._domainName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._domainName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._domainName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._domainName.Location = new System.Drawing.Point(13, 47);
			this._domainName.Name = "_domainName";
			this._domainName.Size = new System.Drawing.Size(441, 27);
			this._domainName.TabIndex = 20;
			this._domainName.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this._domainName_DrawItem);
			this._domainName.SelectedIndexChanged += new System.EventHandler(this._domainName_SelectedIndexChanged);
			this._domainName.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this._domainName_MeasureItem);
			//
			// label3
			//
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(10, 355);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(37, 13);
			this.label3.TabIndex = 19;
			this.label3.Text = "Word";
			//
			// _listViewWords
			//
			this._listViewWords.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._listViewWords.ColumnWidth = 100;
			this._listViewWords.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._listViewWords.ItemHeight = 20;
			this._listViewWords.Items.AddRange(new object[] {
			listViewItem1,
			listViewItem2});
			this._listViewWords.Location = new System.Drawing.Point(15, 233);
			this._listViewWords.MultiColumn = true;
			this._listViewWords.Name = "_listViewWords";
			this._listViewWords.Size = new System.Drawing.Size(622, 104);
			this._listViewWords.Sorted = true;
			this._listViewWords.TabIndex = 17;
			this._listViewWords.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this._listViewWords_KeyPress);
			this._listViewWords.Click += new System.EventHandler(this._listViewWords_Click);
		  //
			//_animatedText
		  //
		  this._animatedText.AutoSize = true;
		  this._animatedText.Name = "_animatedText";
		  this._animatedText.Visible = false;
		  this._animatedText.BackColor = Color.White;

			//
			// label5
			//
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.ForeColor = System.Drawing.Color.DarkGray;
			this.label5.Location = new System.Drawing.Point(535, 54);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(102, 15);
			this.label5.TabIndex = 16;
			this.label5.Text = "(Page Down Key)";
			//
			// label4
			//
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.ForeColor = System.Drawing.Color.DarkGray;
			this.label4.Location = new System.Drawing.Point(573, 352);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(67, 15);
			this.label4.TabIndex = 14;
			this.label4.Text = "(Enter Key)";
			//
			// _instructionLabel
			//
			this._instructionLabel.AutoSize = true;
			this._instructionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
			this._instructionLabel.ForeColor = System.Drawing.Color.DarkGray;
			this._instructionLabel.Location = new System.Drawing.Point(11, 7);
			this._instructionLabel.Name = "_instructionLabel";
			this._instructionLabel.Size = new System.Drawing.Size(399, 20);
			this._instructionLabel.TabIndex = 15;
			this._instructionLabel.Text = "Try thinking of words you use to talk about these things.";
			//
			// _question
			//
			this._question.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._question.AutoEllipsis = true;
			this._question.BackColor = System.Drawing.Color.MistyRose;
			this._question.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._question.Location = new System.Drawing.Point(45, 177);
			this._question.Name = "_question";
			this._question.Size = new System.Drawing.Size(592, 51);
			this._question.TabIndex = 23;
			//
			// _description
			//
			this._description.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._description.AutoEllipsis = true;
			this._description.BackColor = System.Drawing.Color.MistyRose;
			this._description.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._description.Location = new System.Drawing.Point(15, 85);
			this._description.Name = "_description";
			this._description.Size = new System.Drawing.Size(622, 90);
			this._description.TabIndex = 24;
			//
			// _vernacularBox
			//
			this._vernacularBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
//            this._vernacularBox.BackColor = System.Drawing.Color.Green;
			this._vernacularBox.Location = new System.Drawing.Point(49, 344);
			this._vernacularBox.Name = "_vernacularBox";
			this._vernacularBox.ShowAnnotationWidget = false;
			this._vernacularBox.Size = new System.Drawing.Size(439, 30);
			this._vernacularBox.TabIndex = 1;
			this._vernacularBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this._boxVernacularWord_KeyDown);
			//
			// _btnPrevious
			//
			this._btnPrevious.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._btnPrevious.ArrowEnabled = true;
			this._btnPrevious.HoverEndColor = System.Drawing.Color.Blue;
			this._btnPrevious.HoverStartColor = System.Drawing.Color.White;
			this._btnPrevious.Location = new System.Drawing.Point(462, 50);
			this._btnPrevious.Name = "_btnPrevious";
			this._btnPrevious.NormalEndColor = System.Drawing.Color.White;
			this._btnPrevious.NormalStartColor = System.Drawing.Color.White;
			this._btnPrevious.Rotation = 270;
			this._btnPrevious.Size = new System.Drawing.Size(24, 24);
			this._btnPrevious.StubbyStyle = false;
			this._btnPrevious.TabIndex = 12;
			this._btnPrevious.Click += new System.EventHandler(this._btnPrevious_Click);
			//
			// _btnNext
			//
			this._btnNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._btnNext.ArrowEnabled = true;
			this._btnNext.HoverEndColor = System.Drawing.Color.Blue;
			this._btnNext.HoverStartColor = System.Drawing.Color.White;
			this._btnNext.Location = new System.Drawing.Point(487, 41);
			this._btnNext.Name = "_btnNext";
			this._btnNext.NormalEndColor = System.Drawing.Color.White;
			this._btnNext.NormalStartColor = System.Drawing.Color.White;
			this._btnNext.Rotation = 90;
			this._btnNext.Size = new System.Drawing.Size(43, 43);
			this._btnNext.StubbyStyle = false;
			this._btnNext.TabIndex = 13;
			this._btnNext.Click += new System.EventHandler(this._btnNext_Click);
			//
			// _btnAddWord
			//
			this._btnAddWord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._btnAddWord.ArrowEnabled = true;
			this._btnAddWord.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._btnAddWord.HoverEndColor = System.Drawing.Color.Blue;
			this._btnAddWord.HoverStartColor = System.Drawing.Color.White;
			this._btnAddWord.Location = new System.Drawing.Point(487, 321);
			this._btnAddWord.Name = "_btnAddWord";
			this._btnAddWord.NormalEndColor = System.Drawing.Color.White;
			this._btnAddWord.NormalStartColor = System.Drawing.Color.White;
			this._btnAddWord.Rotation = 270;
			this._btnAddWord.Size = new System.Drawing.Size(80, 80);
			this._btnAddWord.StubbyStyle = true;
			this._btnAddWord.TabIndex = 10;
			this._btnAddWord.Text = "   +";
			this._btnAddWord.Click += new System.EventHandler(this._btnAddWord_Click);
			//
			// _questionIndicator
			//
			this._questionIndicator.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._questionIndicator.AutoSize = true;
			this._questionIndicator.BulletColor = System.Drawing.Color.Azure;
			this._questionIndicator.BulletColorEnd = System.Drawing.Color.MediumBlue;
			this._questionIndicator.BulletPadding = new System.Windows.Forms.Padding(1);
			this._questionIndicator.Location = new System.Drawing.Point(15, 179);
			this._questionIndicator.Name = "_questionIndicator";
			this._questionIndicator.Rows = 5;
			this._questionIndicator.Size = new System.Drawing.Size(14, 35);
			this._questionIndicator.TabIndex = 0;
			this._questionIndicator.TabStop = false;

			CubicBezierCurve c = new CubicBezierCurve(new PointF(0, 0),
	  new PointF(0.5f, 0f), new PointF(.5f, 1f), new PointF(1, 1));
			this._animator.PointFromDistanceFunction = c.GetPointOnCurve;

		  this._animator.Animate += new Animator.AnimateEventDelegate(_animator_Animate);
		  this._animator.Duration = 750;
		  this._animator.Finished += new System.EventHandler(_animator_Finished);
		  this._animator.FrameRate = 30;
		  this._animator.SpeedFunction = Animator.SpeedFunctions.SinSpeed;

			//
			// GatherBySemanticDomainsControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._animatedText);
			this.Controls.Add(this._questionIndicator);
			this.Controls.Add(this._description);
			this.Controls.Add(this._question);
			this.Controls.Add(this._vernacularBox);
			this.Controls.Add(this._domainName);
			this.Controls.Add(this.label3);
			this.Controls.Add(this._listViewWords);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this._instructionLabel);
			this.Controls.Add(this._btnPrevious);
			this.Controls.Add(this._btnNext);
			this.Controls.Add(this._btnAddWord);
			this.Name = "GatherBySemanticDomainsControl";
			this.Size = new System.Drawing.Size(654, 386);
			this.BackColorChanged += new System.EventHandler(this.GatherWordListControl_BackColorChanged);
			this.ResumeLayout(false);
			this.PerformLayout();

		}



		#endregion

	  private Animator _animator;

		private WeSay.UI.MultiTextControl _vernacularBox;
		private System.Windows.Forms.ComboBox _domainName;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ListBox _listViewWords;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label4;
	  private System.Windows.Forms.Label _instructionLabel;
		private ArrowButton.ArrowButton _btnPrevious;
		private ArrowButton.ArrowButton _btnNext;
		private ArrowButton.ArrowButton _btnAddWord;
		private System.Windows.Forms.Label _question;
		private System.Windows.Forms.Label _description;
		private WeSay.UI.ProgressIndicator _questionIndicator;
	  private System.Windows.Forms.Label _animatedText;
	}
}
