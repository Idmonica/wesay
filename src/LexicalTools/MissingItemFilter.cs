using System;
using System.Collections.Generic;
using System.Diagnostics;
using WeSay.Foundation;
using WeSay.Language;
using WeSay.LexicalModel;
using WeSay.Project;

namespace WeSay.LexicalTools
{
	public class MissingItemFilter : WeSay.Data.IFilter<LexEntry>
	{
		Field _field;

		public MissingItemFilter(Field field)
		{
			if (field == null)
			{
				throw new ArgumentNullException();
			}
			_field = field;
		}

		public MissingItemFilter(ViewTemplate viewTemplate, string fieldName)
		{
			if (viewTemplate == null)
			{
				throw new ArgumentNullException("viewTemplate");
			}
			if (fieldName == null)
			{
				throw new ArgumentNullException("fieldName");
			}
			Field field;
			if (!viewTemplate.TryGetField(fieldName, out field))
			{
				throw new ArgumentOutOfRangeException("viewTemplate", "ViewTemplate is missing " + fieldName + " field definition");
			}
			_field = field;
		}

		#region IFilter<LexEntry> Members

		public string Key
		{
			get
			{
				string key = "Missing " + _field.FieldName;
				List<string> writingSystemIds = new List<string>(_field.WritingSystemIds);
				writingSystemIds.Sort(StringComparer.InvariantCulture);
				foreach (string writingSystemId in writingSystemIds)
				{
					key += " ["+writingSystemId+"]";
				}
				key += " Filter";
				return key;
			}
		}

		public Predicate<LexEntry> FilteringPredicate
		{
			get
			{
				return IsMissingItem;
			}
		}

		private bool IsMissingDataInWritingSystem(object field)
		{
			switch (_field.DataTypeName)
			{
				case "Option":
					return ((OptionRef) field).IsEmpty;
				case "OptionCollection":
					return ((OptionRefCollection)field).IsEmpty;
				case "MultiText":
					return IsMissingWritingSystem((MultiText)field);
				default:
					Debug.Fail("unknown DataTypeName");
					return false;
			}
		}

		private bool IsMissingItem(LexEntry entry)
		{
			if (entry == null)
			{
				return false;
			}

			switch (_field.ClassName)
			{
				case "LexEntry":
					return IsMissingLexEntryField(entry);
				case "LexSense": // fall through
				case "LexExampleSentence":
					foreach (LexSense sense in entry.Senses)
					{
						if (this._field.ClassName == "LexSense")
						{
							if (IsMissingLexSenseField(sense))
							{
								return true;
							}
						}
						else
						{
							foreach (LexExampleSentence example in sense.ExampleSentences)
							{
								if (this._field.ClassName == "LexExampleSentence")
								{
									if (IsMissingLexExampleSentenceField(example))
									{
										return true;
									}
								}
							}
							if (sense.ExampleSentences.Count == 0 &&
							   (this._field.FieldName == Field.FieldNames.ExampleSentence.ToString()))
							{
								//ghost field
								return true;
							}
						}
					}
					if(entry.Senses.Count == 0 &&
					  (this._field.FieldName == Field.FieldNames.SenseGloss.ToString()))
					{
						//ghost field
						return true;
					}

					break;
				default:
					Debug.Fail("unknown ClassName");
					break;
			}
			return false;
		}

		private bool IsMissingLexExampleSentenceField(LexExampleSentence example)
		{
			if (this._field.IsCustom)
			{
				return IsMissingCustomField(example);
			}
			else
			{
				if (this._field.FieldName == Field.FieldNames.ExampleSentence.ToString())
				{
					return IsMissingWritingSystem(example.Sentence);
				}
				else if(this._field.FieldName == Field.FieldNames.ExampleTranslation.ToString())
				{
					return IsMissingWritingSystem(example.Translation);
				}
				else
				{
					Debug.Fail("unknown FieldName");
					return false;
			   }
			}
		}

		private bool IsMissingLexSenseField(LexSense sense) {
			if (this._field.IsCustom)
			{
				return IsMissingCustomField(sense);
			}
			else
			{
				if(this._field.FieldName == Field.FieldNames.SenseGloss.ToString())
				{
					return IsMissingWritingSystem(sense.Gloss);
				}
				else
				{
					Debug.Fail("unknown FieldName");
					return false;
				}
			}
		}

		private bool IsMissingLexEntryField(LexEntry entry) {
			if (this._field.IsCustom)
			{
				return IsMissingCustomField(entry);
			}
			else
			{
				if (this._field.FieldName == Field.FieldNames.EntryLexicalForm.ToString())
				{
					if(IsMissingWritingSystem(entry.LexicalForm))
					{
						return true;
					}
				}
				else
				{
					Debug.Fail("unknown FieldName");
				}
			}
			return false;
		}

		private bool IsMissingCustomField(WeSayDataObject weSayData) {
			IParentable field = weSayData.GetProperty<IParentable>(this._field.FieldName);
			if(field == null)
			{
				return true;
			}
			return IsMissingDataInWritingSystem(field);
		}

		private bool IsMissingWritingSystem(MultiText field)
		{
			foreach (string wsId in _field.WritingSystemIds)
			{
				if(field[wsId].Length == 0)
				{
					return true;
				}
			}
			return false;
		}
		#endregion
	}
}
