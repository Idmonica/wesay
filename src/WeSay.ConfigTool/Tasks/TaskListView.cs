using System;
using System.Windows.Forms;
using WeSay.Project;

namespace WeSay.ConfigTool.Tasks
{
	public partial class TaskListView: ConfigurationControlBase
	{
		private Autofac.IContext _context;
		public TaskListPresentationModel Model { get; set; }

		public TaskListView(): base("set up tasks for the user")
		{

			InitializeComponent();
			splitContainer1.Resize += splitContainer1_Resize;
		}


		private void splitContainer1_Resize(object sender, EventArgs e)
		{
			try
			{
				//this is part of dealing with .net not adjusting stuff well for different dpis
				splitContainer1.Dock = DockStyle.None;
				splitContainer1.Width = Width - 25;
			}
			catch (Exception)
			{
				//swallow
			}
		}

		private void TaskList_Load(object sender, EventArgs e)
		{
			if (DesignMode)
			{
				return;
			}

			foreach (ITaskConfiguration task in Model.Tasks)
			{
				bool showCheckMark = task.IsVisible || (!task.IsOptional);

				_taskList.Items.Add(task, showCheckMark);
			}
			if (_taskList.Items.Count > 0)
			{
				_taskList.SelectedIndex = 0;
			}
		}

		private void _taskList_SelectedIndexChanged(object sender, EventArgs e)
		{
			splitContainer1.Panel2.Controls.Clear();
			ITaskConfiguration configuration = _taskList.SelectedItem as ITaskConfiguration;
			if (configuration == null)
			{
				return;
			}

//      autofac's generated factory stuff wasn't working with our version of autofac, so
//  i abandoned this
//            Control c = null;
//            try
//            {
//                //look for a factory that makes controls for this kind of task configuration
//                  _context.Resolve(configuration.TaskName);
//            }
//            catch (Exception)
//            {
//            }
//            if(c!=null)
//            {
//                splitContainer1.Panel2.Controls.Add(c);
//            }
			splitContainer1.Panel2.Controls.Add(ConfigTaskControlFactory.Create(configuration));

		}

		private void _taskList_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			ITaskConfiguration i = _taskList.SelectedItem as ITaskConfiguration;
			if (i == null)
			{
				return;
			}
			if (!i.IsOptional)
			{
				e.NewValue = CheckState.Checked;
			}
			i.IsVisible = e.NewValue == CheckState.Checked;
		}

		private void textBox1_TextChanged(object sender, EventArgs e) {}

		private void _description_TextChanged(object sender, EventArgs e) {}
	}
}