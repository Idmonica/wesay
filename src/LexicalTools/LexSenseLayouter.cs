using System.Windows.Forms;
using Palaso.Reporting;
using WeSay.Data;
using WeSay.Foundation;
using WeSay.LexicalModel;
using WeSay.Project;
using WeSay.UI;
using System.ComponentModel;

namespace WeSay.LexicalTools
{
	/// <summary>
	/// <see cref="Layouter"/>
	/// </summary>
	public class LexSenseLayouter : Layouter
	{
		public LexSenseLayouter(DetailList builder, ViewTemplate viewTemplate, IRecordListManager recordListManager)
			: base(builder, viewTemplate, recordListManager)
		{
		}


		internal override int AddWidgets(IBindingList list, int index, int insertAtRow)
		{
			int rowCount = 0;
			DetailList.SuspendLayout();
			try
			{

				LexSense sense = (LexSense) list[index];
				Field field = ActiveViewTemplate.GetField(Field.FieldNames.SenseGloss.ToString());
				if (field != null && field.GetDoShow(sense.Gloss, this.ShowNormallyHiddenFields))
				{
					Control glossControl = MakeBoundControl(sense.Gloss, field);
					string label = StringCatalog.Get("~Meaning");
					LexEntry entry = sense.Parent as LexEntry;
					if (entry != null) // && entry.Senses.Count > 1)
					{
						label += " " + (entry.Senses.IndexOf(sense) + 1);
					}
					Control glossRowControl = DetailList.AddWidgetRow(label, true, glossControl, insertAtRow, false);
					++rowCount;
					insertAtRow = DetailList.GetRow(glossRowControl);
				}

				rowCount += AddCustomFields(sense, insertAtRow + rowCount);

				LexExampleSentenceLayouter exampleLayouter =
					new LexExampleSentenceLayouter(DetailList, ActiveViewTemplate);
				exampleLayouter.ShowNormallyHiddenFields = ShowNormallyHiddenFields;

				rowCount = AddChildrenWidgets(exampleLayouter, sense.ExampleSentences, insertAtRow, rowCount);

				//add a ghost for another example if we don't have one or we're in the "show all" mode
			   //removed because of its effect on the Add Examples task, where
				//we'd like to be able to add more than one
				//if (ShowNormallyHiddenFields || sense.ExampleSentences.Count == 0)
				{
					rowCount += exampleLayouter.AddGhost(sense.ExampleSentences, insertAtRow + rowCount);
				}
			}
			catch (ConfigurationException e)
			{
				Palaso.Reporting.ErrorReport.ReportNonFatalMessage(e.Message);
			}
			DetailList.ResumeLayout(true);
			return rowCount;
		}

		public int AddGhost(IBindingList list, bool isHeading)
		{
//            int rowCount = 0;
//           Field field;
//           //TODO: only add this if there is no empty gloss in an existing sense (we
//           //run into this with the MissingInfoTask, where we don't want to see two empty gloss boxes (one a ghost)
//           if (viewTemplate.TryGetField(Field.FieldNames.SenseGloss.ToString(), out field))
//           {
//               foreach (string writingSystemId in field.WritingSystemIds)
//               {
//                   WritingSystem writingSystem = BasilProject.Project.WritingSystems[writingSystemId];
//
//                   WeSayTextBox entry = new WeSayTextBox(writingSystem);
//                   GhostBinding g = MakeGhostBinding(list, "Gloss", writingSystem, entry);
//                   g.ReferenceControl = DetailList.AddWidgetRow(StringCatalog.GetListOfType("New Meaning"), true, entry);
//                   ++rowCount;
//               }
//           }
//            return rowCount;

			int insertAtRow = -1;
			string label = StringCatalog.Get("~Meaning", "This label is shown once, but has two roles.  1) it labels contains the gloss, and 2) marks the beginning of the set of fields which make up a sense. So, in english, if we labelled this 'gloss', it would describe the field well but wouldn't label the section well.");
			if(list.Count > 0)
			{
				label += " "+(list.Count + 1);
			}
			return MakeGhostWidget<LexSense>(list, insertAtRow, Field.FieldNames.SenseGloss.ToString(), label, "Gloss", isHeading);

		}

		public static bool HasSenseWithEmptyGloss(IBindingList list)
		{
			foreach (LexSense sense in list)
			{
				if (sense.Gloss.Count == 0)
				{
					return true;
				}
			}
			return false;
		}

	}
}
