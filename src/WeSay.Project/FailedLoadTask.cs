using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WeSay.Project
{
	public class FailedLoadTask :ITask
	{
		private string _label;
		private string _description;

		public FailedLoadTask(string label, string description)
		{
			_label = label;
			_description = description;
		}

		public void Activate()
		{
		}

		public void Deactivate()
		{
		}

		#region ITask Members

		public void GoToUrl(string url)
		{
			throw new NotImplementedException();
		}

		#endregion

		public bool IsActive
		{
			get { return false; }
		}

		public string Label
		{
			get { return String.Format("Failed To Load: {0}", _label); }
		}

		public string Description
		{
			get { return String.Format("Error: {0}", _description); }
		}

		public bool MustBeActivatedDuringPreCache
		{
			get { return false; }
		}

		public void RegisterWithCache(ViewTemplate viewTemplate)
		{

		}

		public Control Control
		{
			get {
				TextBox t = new TextBox();
				t.Multiline = true;
				t.Dock = DockStyle.Fill;
				t.Text =
					String.Format(
						"Could not load the task '{0}'. Possibly, the setup in the admin program can be used to fix this.  The error was: [{1}]",
						_label, _description);
				return t;
			}
		}

		public bool IsPinned
		{
			get { return false; }
		}

		const int CountNotApplicable = -1;

		public int Count
		{
			get
			{
				return CountNotApplicable;
			}
		}

		public int ExactCount
		{
			get
			{
				return CountNotApplicable;
			}
		}

		/// <summary>
		/// Gives a sense of the overall size of the task versus what's left to do
		/// </summary>
		public int ReferenceCount
		{
			get
			{
				return 0;
			}
		}
	}
}
