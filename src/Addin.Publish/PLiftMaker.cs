

using System;
using WeSay.Data;
using WeSay.LexicalModel;
using WeSay.LexicalModel.Db4o_Specific;
using WeSay.Project;

namespace Addin.Publish
{
	public class PLiftMaker
	{
		public PLiftMaker()
		{
		}


		public void Make(InMemoryRecordList<LexEntry> entries, ViewTemplate template, string path)
		{
			LiftExporter exporter = new LiftExporter(path);
			exporter.Template = template;
			foreach (LexEntry entry in entries)
			{
				exporter.Add(entry); // .Add(entries, i, howManyAtATime);
			}
			exporter.End();
		}
	}
}