using System;
using System.Collections.Generic;
using System.Xml;
using Exortech.NetReflector;
using WeSay.Foundation;
using WeSay.Language;

namespace WeSay.Project
{
	[ReflectorType("viewTemplate")]
	public class ViewTemplate : List<Field>
	{
		private string _id="Default View Template";

		public ViewTemplate()
		{

		}

		/// <summary>
		/// For serialization only
		/// </summary>
		[ReflectorCollection("fields", Required = true)]
		public List<Field> Fields
		{
			get
			{
				return this;
			}
			set
			{
				Clear();
				foreach (Field  f in value)
				{
					if (f == null)
					{
						throw new ArgumentNullException("field",
														"one of the fields is null");
					}
					Add(f);
				}
			}
		}

		[ReflectorProperty("id", Required = false)]
		public string Id
		{
			get { return _id; }
			set { _id = value; }
		}

		///<summary>
		///Gets the field with the specified name.
		///</summary>
		///
		///<returns>
		///The field with the given field name.
		///</returns>
		///
		///<param name="index">The field name of the field to get.</param>
		///<exception cref="T:System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
		public Field this[string fieldName]
		{
			get
			{
				Field field;
				if(!TryGetField(fieldName, out field))
				{
					throw new ArgumentOutOfRangeException();
				}
				return field;
			}
		}

		public bool TryGetField(string fieldName, out Field field)
		{
			if(fieldName == null)
			{
				throw new ArgumentNullException();
			}
			field = Find(
						   delegate(Field f)
						   {
							   return f.FieldName == fieldName;
						   });

			if (field == default(Field))
			{
				return false;
			}
			return true;
		}

		public Field GetField(string fieldName)
		{
			Field field = null;
			TryGetField(fieldName, out field);
			return field;
		}

		public List<Field> GetCustomFields(string className)
		{
			List<Field> customFields = new List<Field>();
			foreach (Field field in this)
			{
				if (field.ClassName == className && !field.IsBuiltInViaCode)
				{
					customFields.Add(field);
				}
			}
			return customFields;
		}

		public bool Contains(string fieldName)
		{
			Field field;
			if (!TryGetField(fieldName, out field))
			{
				return false;
			}
			return field.Visibility == CommonEnumerations.VisibilitySetting.Visible;
		}

		/// <summary>
		/// Update the users' field inventory with new stuff.
		/// </summary>
		/// <remarks>
		/// used in the admin to update the users template with new fields.
		///
		/// This was written before, and should not to be confused with,
		///  MigrateConfigurationXmlIfNeeded(), which could
		/// do most of this work. It's not obvious how that would get the guess
		/// of which writing systems to assign in (but that's not a big deal).
		///
		/// The algorithm here is to fill the list with all of the fields from the master inventory.
		/// If a field is also found in the users existing inventory, turn on the checkbox,
		/// and change any settings (e.g. the writing systems) to match the users' inventory spec.
		/// Then add any custom fields from the existing inventory.
		/// </remarks>
		/// <param name="factoryTemplate"></param>
		/// <param name="usersTemplate"></param>
		public static void UpdateUserViewTemplate(ViewTemplate factoryTemplate, ViewTemplate usersTemplate)
		{
			if(factoryTemplate == null)
			{
				throw new ArgumentNullException();
			}
			if(usersTemplate == null)
			{
				throw new ArgumentNullException();
			}
			foreach (Field masterField in factoryTemplate)
			{
				Field userField = usersTemplate.GetField(masterField.FieldName);
				if (userField != null)
				{
				 //why do that???   Field.ModifyMasterFromUser(masterField, userField);
					//allow us to improve the descriptions
					userField.Description = masterField.Description;
				}
				else
				{
					masterField.Visibility= CommonEnumerations.VisibilitySetting.Invisible;//let them turn it on if they want
					usersTemplate.Fields.Add(masterField);
				}
			}
			//add custom fields (or out of date fields <--todo!)
//            foreach (Field userField in usersTemplate)
//            {
//                Field masterField = factoryTemplate.GetField(userField.FieldName);
//                if (masterField == null)
//                {
//                    factoryTemplate.Add(userField);
//                }
//            }
		}

		public static ViewTemplate MakeMasterTemplate(WritingSystemCollection writingSystems)
		{
			List<String> defaultVernacularSet = new List<string>();
			defaultVernacularSet.Add(WritingSystem.IdForUnknownVernacular);

			List<String> defaultAnalysisSet = new List<string>();
			defaultAnalysisSet.Add(WritingSystem.IdForUnknownAnalysis);

			ViewTemplate masterTemplate = new ViewTemplate();

			Field lexicalFormField = new Field(Field.FieldNames.EntryLexicalForm.ToString(), "LexEntry", defaultVernacularSet);
			//this is here so the PoMaker scanner can pick up a comment about this label
			StringCatalog.Get("~Word", "The label for the field showing the Lexeme Form of the entry.");
			lexicalFormField.DisplayName = "~Word";
			lexicalFormField.Description = "The Lexeme Form of the entry.";
			lexicalFormField.Visibility = CommonEnumerations.VisibilitySetting.Visible;
			masterTemplate.Add(lexicalFormField);

			Field glossField = new Field(Field.FieldNames.SenseGloss.ToString(), "LexSense", defaultAnalysisSet);
			glossField.DisplayName = "~Gloss";
			glossField.Description = "Normally a single word. Shows up as the first field of the sense, across from the 'Meaning' label";
			glossField.Visibility = CommonEnumerations.VisibilitySetting.Visible;
			masterTemplate.Add(glossField);

			Field posField = new Field("POS", "LexSense", defaultAnalysisSet);
			//this is here so the PoMaker scanner can pick up a comment about this label
			StringCatalog.Get("~POS", "The label for the field showing Part Of Speech");
			posField.DisplayName = "~POS";
			posField.Description = "The grammatical category of the entry (Noun, Verb, etc.).";
			posField.DataTypeName = "Option";
			posField.OptionsListFile = "PartsOfSpeech.xml";
			masterTemplate.Add(posField);

			Field exampleField = new Field(Field.FieldNames.ExampleSentence.ToString(), "LexExampleSentence", defaultVernacularSet);
			exampleField.DisplayName = "~Example Sentence";
			exampleField.Visibility = CommonEnumerations.VisibilitySetting.Visible;
			masterTemplate.Add(exampleField);

			Field translationField = new Field(Field.FieldNames.ExampleTranslation.ToString(), "LexExampleSentence", defaultAnalysisSet);
			//this is here so the PoMaker scanner can pick up a comment about this label
			StringCatalog.Get("~Example Translation", "The label for the field showing the example sentence translated into other languages.");
			translationField.DisplayName = "Example Translation";
			translationField.Description = "The translation of the example sentence into another language.";
			translationField.Visibility = CommonEnumerations.VisibilitySetting.Visible;
			masterTemplate.Add(translationField);

			Field ddp4Field = new Field("SemanticDomainDdp4", "LexSense", defaultAnalysisSet);

			//this is here so the PoMaker scanner can pick up a comment about this label
			StringCatalog.Get("~Sem Dom", "The label for the field showing Semantic Domains");

			ddp4Field.DisplayName = "~Sem Dom";
			ddp4Field.Description = "The semantic domain using Ron Moe's Dictionary Development Process version 4.";
			ddp4Field.DataTypeName = "OptionCollection";
			ddp4Field.OptionsListFile = "Ddp4.xml";
			masterTemplate.Add(ddp4Field);

			Field superEntryField = new Field("BaseForm", "LexEntry", defaultVernacularSet,
				Field.MultiplicityType.ZeroOr1, "RelationToOneEntry");
			//this is here so the PoMaker scanner can pick up a comment about this label
			StringCatalog.Get("~Base Form", "The label for the field showing the entry which this entry is derived from, is a subentry of, etc.");
			superEntryField.DisplayName = "~Base Form";
			superEntryField.Description = "Provides a field for identifying the form from which an entry is derived.  You may use this in place of the MDF subentry.  In a future version WeSay may support directly listing derived forms from the base form.";
			superEntryField.Visibility = CommonEnumerations.VisibilitySetting.Invisible;
			masterTemplate.Add(superEntryField);

			return masterTemplate;
		}

		#region persistence

		public void Load(string path)
		{
			NetReflectorReader r = new NetReflectorReader(MakeTypeTable());
			XmlReader reader = XmlReader.Create(path);
			try
			{
				r.Read(reader, this);
			}
			finally
			{
				reader.Close();
			}
		}

		public void LoadFromString(string xml)
		{
			NetReflectorReader r = new NetReflectorReader(MakeTypeTable());
			XmlReader reader = XmlReader.Create(new System.IO.StringReader(xml));
			try
			{
				r.Read(reader, this);
			}
			finally
			{
				reader.Close();
			}
		}

		/// <summary>
		/// Does not "start" the xml doc, no close the writer
		/// </summary>
		/// <param name="writer"></param>
		public void Write(XmlWriter writer)
		{
			 NetReflector.Write(writer, this);
		}

		private NetReflectorTypeTable MakeTypeTable()
		{
			NetReflectorTypeTable t = new NetReflectorTypeTable();
			t.Add(typeof(ViewTemplate ));
			t.Add(typeof(Field));
		 //   t.Add(typeof(Field.WritingSystemId));
			return t;
		}

		#endregion
	 }

}
