using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NUnit.Framework;
using WeSay.CommonTools;
using WeSay.Foundation.Dashboard;
using WeSay.LexicalModel;

namespace WeSay.App.Tests
{
	[TestFixture]
	public class DashBoardTests
	{
		private LexEntryRepository _lexEntryRepository;
		private string _filePath;

		[SetUp]
		public void Setup()
		{
			_filePath = Path.GetTempFileName();
			_lexEntryRepository = new LexEntryRepository(_filePath);

			Form window = new Form();
			window.Size = new Size(800, 600);

			_lexEntryRepository.CreateItem();

			Dash dash = new Dash(_lexEntryRepository, null);
			dash.ThingsToMakeButtonsFor = GetButtonItems();
			dash.Dock = DockStyle.Fill;
			window.Controls.Add(dash);
			window.BackColor = dash.BackColor;
			dash.Activate();
			Application.Run(window);
		}

		[TearDown]
		public void Teardown()
		{
			_lexEntryRepository.Dispose();
			File.Delete(_filePath);
		}

		[Test]
		[Ignore("not really a test")]
		public void Run() {}

		private List<IThingOnDashboard> GetButtonItems()
		{
			List<IThingOnDashboard> buttonItems = new List<IThingOnDashboard>();
			buttonItems.Add(new ThingThatGetsAButton(DashboardGroup.Gather, "Semantic Domains"));
			buttonItems.Add(new ThingThatGetsAButton(DashboardGroup.Gather, "PNG Word List"));
			buttonItems.Add(
					new ThingThatGetsAButton(DashboardGroup.Describe, "Nuhu Sapoo Definitions"));
			buttonItems.Add(new ThingThatGetsAButton(DashboardGroup.Describe, "Example Sentences"));
			buttonItems.Add(new ThingThatGetsAButton(DashboardGroup.Describe, "English Definitions"));
			buttonItems.Add(
					new ThingThatGetsAButton(DashboardGroup.Describe,
											 "Translate Examples To English"));
			buttonItems.Add(
					new ThingThatGetsAButton(DashboardGroup.Describe,
											 "Dictionary Browse && Edit",
											 ButtonStyle.IconFixedWidth,
											 null
							/*CommonTools.Properties.Resources.blueDictionary*/));
			buttonItems.Add(new ThingThatGetsAButton(DashboardGroup.Refine, "Identify Base Forms"));
			buttonItems.Add(new ThingThatGetsAButton(DashboardGroup.Refine, "Review"));

			//            buttonItems.Add(new ThingThatGetsAButton(DashboardGroup.Share, "Print", ButtonStyle.IconVariableWidth, Addin..Properties.Resources.greenPrinter));
			//            buttonItems.Add(new ThingThatGetsAButton(DashboardGroup.Share, "Email", ButtonStyle.IconVariableWidth, CommonTools.Properties.Resources.greenEmail));
			//            buttonItems.Add(new ThingThatGetsAButton(DashboardGroup.Share, "Synchronize", ButtonStyle.IconVariableWidth, CommonTools.Properties.Resources.greenSynchronize));

			return buttonItems;
		}
	}

	internal class ThingThatGetsAButton: IThingOnDashboard
	{
		private readonly DashboardGroup _group;
		private readonly string _localizedLabel;
		private Font _font;
		private ButtonStyle _style;
		private readonly Image _image;

		public ThingThatGetsAButton(DashboardGroup group,
									string localizedLabel,
									ButtonStyle style,
									Image image)
		{
			_image = image;
			DashboardButtonStyle = style;
			_group = group;
			_localizedLabel = localizedLabel;
			Font = new Font("Arial", 10);
		}

		public ThingThatGetsAButton(DashboardGroup group, string localizedLabel)
				: this(group, localizedLabel, ButtonStyle.VariableAmount, null) {}

		//todo: this belongs on the button, which knows better what it has planned
		public int WidthToDisplayFullSizeLabel
		{
			get
			{
				return
						TextRenderer.MeasureText(LocalizedLabel,
												 Font,
												 new Size(int.MaxValue, int.MaxValue),
												 TextFormatFlags.LeftAndRightPadding).Width;
			}
		}

		#region IThingOnDashboard Members

		public DashboardGroup Group
		{
			get { return DashboardGroup.Describe; }
		}

		#endregion

		public string LocalizedLabel
		{
			get { return _localizedLabel; }
		}

		public Font Font
		{
			get { return _font; }
			set { _font = value; }
		}

		public ButtonStyle DashboardButtonStyle
		{
			get { return _style; }
			set { _style = value; }
		}

		#region IThingOnDashboard Members

		public Image DashboardButtonImage
		{
			get { return _image; }
		}

		#endregion
	}
}