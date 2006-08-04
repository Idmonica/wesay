using System;
using System.Collections.Generic;
using System.Text;

namespace WeSay.Language
{
	class WritingSystem
	{
		protected System.Collections.Generic.Dictionary<string, string> _forms;
		public WritingSystem()
		{
			_forms = new Dictionary<string, string>();
		}

		public string GetAlternative(string writingSystem)
		{
			string form = _forms[writingSystem];
			if (form == null)
			{
				return null;
			}
			else
				return form;
		}
	}
}
