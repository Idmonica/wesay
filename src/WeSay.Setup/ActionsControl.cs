using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Mono.Addins;
using WeSay.AddinLib;

namespace WeSay.Setup
{
	public partial class ActionsControl : UserControl
	{
		private bool _loaded=false;

		public ActionsControl()
		{
			InitializeComponent();
		}

		private void OnVisibleChanged(object sender, EventArgs e)
		{
			if (Visible)
			{
				if (!_loaded)
				{
					LoadAddins();
					_loaded = true;
				}
				UpdateStatesOfThings();
			}
		}

		private void LoadAddins()
		{
			_listView.Items.Clear();
			if(!AddinManager.IsInitialized)
			{
				AddinManager.Initialize(Application.UserAppDataPath);
				AddinManager.Registry.Rebuild(null);
				AddinManager.Shutdown();
				AddinManager.Initialize(Application.UserAppDataPath);
				//these (at least AddinLoaded) does get called after initialize, when you
				//do a search for objects (e.g. GetExtensionObjects())
				AddinManager.AddinLoaded += new AddinEventHandler(AddinManager_AddinLoaded);
				AddinManager.AddinLoadError += new AddinErrorEventHandler(AddinManager_AddinLoadError);
				AddinManager.AddinUnloaded += new AddinEventHandler(AddinManager_AddinUnloaded);
				AddinManager.ExtensionChanged += new ExtensionEventHandler(AddinManager_ExtensionChanged);
			}

			foreach (IWeSayAddin addin in AddinManager.GetExtensionObjects(typeof(IWeSayAddin)))
			{
				AddAddin(addin);
			}
			AddAddin(new ComingSomedayAddin("Export To OpenOffice", ""));
			AddAddin(new ComingSomedayAddin("Export To Word", ""));
			AddAddin(new ComingSomedayAddin("Export To Lexique Pro", ""));
			AddAddin(new ComingSomedayAddin("Export as MDF", "Create a toolbox-compatible dictionary file."));
			AddAddin(
				new ComingSomedayAddin("Send project to developers", "Sends your project to WeSay for help/debugging."));
		}

		private void AddAddin(IWeSayAddin addin)
		{
			ListViewItem item = new ListViewItem(addin.Name);
			item.SubItems.Add(addin.ShortDescription);
			item.Tag = addin;
			if (!addin.Available)
			{
				item.ForeColor = System.Drawing.Color.Gray;
			}
			if (addin.ButtonImage != null)
			{
				item.ImageIndex = _imageList.Images.Count;
				_imageList.Images.Add(addin.ButtonImage);
			}
			_listView.Items.Add(item);
		}

		void AddinManager_ExtensionChanged(object sender, ExtensionEventArgs args)
		{
			Reporting.Logger.WriteEvent("Addin 'extensionChanged': {0}",args.Path);
		}

		void AddinManager_AddinUnloaded(object sender, AddinEventArgs args)
		{
			Reporting.Logger.WriteEvent("Addin unloaded: {0}",args.AddinId);
		}

		void AddinManager_AddinLoadError(object sender, AddinErrorEventArgs args)
		{
			Reporting.Logger.WriteEvent("Addin load error: {0}", args.AddinId);
		}

		void AddinManager_AddinLoaded(object sender, AddinEventArgs args)
		{
			Reporting.Logger.WriteEvent("Addin loaded: {0}", args.AddinId);
		}

		protected IWeSayAddin CurrentAddin
		{
			get
			{
				if (_listView.SelectedItems != null && _listView.SelectedItems.Count == 1)
				{
					IWeSayAddin addin= _listView.SelectedItems[0].Tag as IWeSayAddin;
					if (addin.Available)
						return addin;
					else return null;
				}
				return null;
			}
		}


		private void OnLaunch(object sender, EventArgs e)
		{
			if (CurrentAddin == null)
				return;

			try
			{
				CurrentAddin.Launch(Project.WeSayWordsProject.Project.ProjectDirectoryPath,
									Project.WeSayWordsProject.Project.PathToLiftFile);
			}
			catch(Exception err)
			{
				Reporting.ErrorReporter.ReportNonFatalMessage(err.Message);
			}
		}

		private void OnListView_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateStatesOfThings();
		}

		private void UpdateStatesOfThings()
		{
			_launchButton.Enabled = (CurrentAddin != null);
		}
	}
}
