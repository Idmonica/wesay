using System;
using System.Collections.Generic;
using System.Text;
using Palaso.Text;
using WeSay.Foundation;
using WeSay.Foundation.Options;
using WeSay.LexicalModel;

namespace WeSay.Project
{
	public class PLiftExporter: LiftExporter
	{
		private readonly ViewTemplate _viewTemplate;
		private readonly LexEntryRepository _lexEntryRepository;
		private readonly IList<string> _headwordWritingSystemIds;

		public PLiftExporter(StringBuilder builder,
							 bool produceFragmentOnly,
							 LexEntryRepository lexEntryRepository,
							 ViewTemplate viewTemplate): base(builder, produceFragmentOnly)
		{
			this._lexEntryRepository = lexEntryRepository;
			this._viewTemplate = viewTemplate;
			_headwordWritingSystemIds = _viewTemplate.GetHeadwordWritingSystemIds();
		}

		public PLiftExporter(string path,
							 LexEntryRepository lexEntryRepository,
							 ViewTemplate viewTemplate): base(path)
		{
			this._lexEntryRepository = lexEntryRepository;
			this._viewTemplate = viewTemplate;
			_headwordWritingSystemIds = _viewTemplate.GetHeadwordWritingSystemIds();
		}

		private Options _options = Options.DereferenceRelations | Options.DereferenceOptions |
								   Options.DetermineHeadword;


		[Flags]
		public enum Options
		{
			NormalLift = 0,
			DereferenceRelations = 1,
			DereferenceOptions = 2,
			DetermineHeadword = 4
		} ;

		/// <summary>
		/// Set this if you want the output filtered to the visible fields and the writing system orders respected
		/// </summary>
		public ViewTemplate Template
		{
			get { return _viewTemplate; }
		}

		public Options ExportOptions
		{
			get { return _options; }
			set { _options = value; }
		}

		public override void Add(LexEntry entry)
		{
			WritingSystem headWordWritingSystem = _viewTemplate.HeadwordWritingSystems[0];
			int h = _lexEntryRepository.GetHomographNumber(entry, headWordWritingSystem);
			Add(entry, h);
		}

		private void WriteDisplayNameFieldForOption(OptionRef optionRef, string fieldName)
		{
			OptionsList list = WeSayWordsProject.Project.GetOptionsList(fieldName);
			if (list != null)
			{
				Option posOption = list.GetOptionFromKey(optionRef.Value);
				if (posOption == null)
				{
					return;
				}
				if (posOption.Name == null)
				{
					return;
				}

				LanguageForm[] labelForms =
						posOption.Name.GetOrderedAndFilteredForms(
								_viewTemplate.GetField(fieldName).WritingSystemIds);

				if (labelForms != null && labelForms.Length > 0)
				{
					Writer.WriteStartElement("field");
					Writer.WriteAttributeString("type",
												fieldName == "POS" ? "grammatical-info" : fieldName);
					Add(labelForms, false);
					Writer.WriteEndElement();
				}
			}
		}

		/// <summary>
		/// nb: this is used both for the headword of an article, but also for the target of a relation.
		/// </summary>
		private void WriteHeadWordField(LexEntry entry, string outputFieldName)
		{
			//                headword.SetAlternative(HeadWordWritingSystemId, entry.GetHeadWordForm(HeadWordWritingSystemId));

			MultiText headword = new MultiText();
			foreach (string writingSystemId in _headwordWritingSystemIds)
			{
				headword.SetAlternative(writingSystemId, entry.GetHeadWordForm(writingSystemId));
			}
			WriteMultiTextAsArtificialField(outputFieldName, headword);
		}


		/// <summary>
		/// use this for multitexts that were somehow constructed during export, with no corresponding single property
		/// </summary>
		private void WriteMultiTextAsArtificialField(string outputFieldName, MultiTextBase text)
		{
			if (!MultiTextBase.IsEmpty(text))
			{
				Writer.WriteStartElement("field");

				Writer.WriteAttributeString("type", outputFieldName);

				if (!MultiTextBase.IsEmpty(text))
				{
					Add(text.Forms, true);
				}

				Writer.WriteEndElement();
			}
		}

		protected override void WriteRelationTarget(LexRelation relation)
		{
			if (0 == (ExportOptions & Options.DereferenceRelations))
			{
				return;
			}

			string key = relation.Key;
			if(string.IsNullOrEmpty(key))
			{
				return;
			}

			LexEntry target = this._lexEntryRepository.GetLexEntryWithMatchingId(key);
			if (target != null)
			{
				WriteHeadWordField(target, "headword-of-target");
			}
		}

		protected override void WritePosCore(OptionRef pos)
		{
			if (0 != (_options & Options.DereferenceOptions))
			{
				WriteDisplayNameFieldForOption(pos, LexSense.WellKnownProperties.PartOfSpeech);
			}
			else
			{
				base.WritePosCore(pos);
			}
		}

		protected override void WriteHeadword(LexEntry entry)
		{
			if (0 != (_options & Options.DetermineHeadword))
			{
				WriteHeadWordField(entry, "headword");
			}
		}

		protected override void WriteOptionRef(string key, OptionRef optionRef)
		{
			if (optionRef.Value.Length > 0)
			{
				if (0 != (ExportOptions & Options.DereferenceOptions))
				{
					WriteDisplayNameFieldForOption(optionRef, key);
				}
				else
				{
					base.WriteOptionRef(key, optionRef);
				}
			}
		}

		protected override bool ShouldOutputProperty(string property)
		{
			Field f = Template.GetField(property);
			if (f == null)
			{
				return false;
			}
			return (f.Enabled);
		}

		protected override LanguageForm[] GetOrderedAndFilteredForms(MultiTextBase text,
																	 string propertyName)
		{
			Field f = Template.GetField(propertyName);
			if (f == null)
			{
				return text.Forms;
			}
			return text.GetOrderedAndFilteredForms(f.GetTextOnlyWritingSystemIds(_viewTemplate.WritingSystems));
		}
	}
}