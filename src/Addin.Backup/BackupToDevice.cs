using System;
using System.Drawing;
using System.Windows.Forms;
using Mono.Addins;
using Palaso.UI.WindowsForms.i8n;
using WeSay.AddinLib;
using WeSay.Foundation;

namespace Addin.Backup
{

	[Extension]
	public class BackupToDevice : IWeSayAddin//, IWeSayProjectAwareAddin
	{

		#region IWeSayAddin Members

		public Image ButtonImage
		{
			get
			{
				return Resources.backupToDeviceImage;
			}
		}

		public bool Available
		{
			get
			{
				return true;
			}
		}

		public string LocalizedName
		{
			get
			{
				return StringCatalog.Get("~Backup To Device","Label for usb flash-drive backup action");
			}
		}

		public string ShortDescription
		{
			get
			{
				return StringCatalog.Get("~Saves a backup on an external device, like a USB key.","description of usb backup action");
			}
		}



		public string ID
		{
			get
			{
				return "ManualBackupToDevice";
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public void Launch(Form parentForm, ProjectInfo projectInfo)
		{
			BackupDialog d = new BackupDialog(projectInfo);
			d.ShowDialog(parentForm);
		}
		#endregion

	}
}
