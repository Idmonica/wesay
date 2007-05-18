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
		/// used in the admin to make sure what we write out is based on the latest master inventory
		/// </summary>
		/// <remarks>
		/// The algorithm here is to fill the list with all of the fields from the master inventory.
		/// If a field is also found in the users existing inventory, turn on the checkbox,
		/// and set all of the writing systems to match what the user had before.
		/// </remarks>
		/// <param name="masterTemplate"></param>
		/// <param name="usersTemplate"></param>
		public static void SynchronizeInventories(ViewTemplate masterTemplate, ViewTemplate usersTemplate)
		{
			if(masterTemplate == null)
			{
				throw new ArgumentNullException();
			}
			if(usersTemplate == null)
			{
				throw new ArgumentNullException();
			}
			foreach (Field masterField in masterTemplate)
			{
				Field userField = usersTemplate.GetField(masterField.FieldName);
				if (userField != null)
				{
					Field.ModifyMasterFromUser( masterField, userField);
				}
			}
			//add custom fields (or out of date fields <--todo!)
			foreach (Field userField in usersTemplate)
			{
				Field masterField = masterTemplate.GetField(userField.FieldName);
				if (masterField == null)
				{
					masterTemplate.Add(userField);
				}
			}
		}

		public static ViewTemplate MakeMasterTemplate(WritingSystemCollection writingSystems)
		{
			List<String> defaultVernacularSet = new List<string>();
			defaultVernacularSet.Add(WritingSystem.IdForUnknownVernacular);

			List<String> defaultAnalysisSet = new List<string>();
			defaultAnalysisSet.Add(WritingSystem.IdForUnknownAnalysis);

			ViewTemplate masterTemplate = new ViewTemplate();

			Field lexicalFormField = new Field(Field.FieldNames.EntryLexicalForm.ToString(), "LexEntry", defaultVernacularSet);
			lexicalFormField.DisplayName = "Word";
			lexicalFormField.Visibility = CommonEnumerations.VisibilitySetting.Visible;
			masterTemplate.Add(lexicalFormField);

			Field glossField = new Field(Field.FieldNames.SenseGloss.ToString(), "LexSense", defaultAnalysisSet);
			glossField.DisplayName = "Gloss";
			glossField.Visibility = CommonEnumerations.VisibilitySetting.Visible;
			masterTemplate.Add(glossField);

			Field posField = new Field("POS", "LexSense", defaultAnalysisSet);
			posField.DisplayName = "POS";
			posField.Description = "The grammatical category of the entry (Noun, Verb, etc.).";
			posField.DataTypeName = "Option";
			posField.OptionsListFile = "PartsOfSpeech.xml";
			masterTemplate.Add(posField);

			Field exampleField = new Field(Field.FieldNames.ExampleSentence.ToString(), "LexExampleSentence", defaultVernacularSet);
			exampleField.DisplayName = "Example Sentence";
			exampleField.Visibility = CommonEnumerations.VisibilitySetting.Visible;
			masterTemplate.Add(exampleField);

			Field translationField = new Field(Field.FieldNames.ExampleTranslation.ToString(), "LexExampleSentence", defaultAnalysisSet);
			translationField.DisplayName = "Translation";
			translationField.Visibility = CommonEnumerations.VisibilitySetting.Visible;
			masterTemplate.Add(translationField);

			Field ddp4Field = new Field("SemanticDomainDdp4", "LexSense", defaultAnalysisSet);
			ddp4Field.DisplayName = "Sem Dom";
			ddp4Field.Description = "The semantic domain using Ron Moe's Dictionary Development Process version 4.";
			ddp4Field.DataTypeName = "OptionCollection";
			ddp4Field.OptionsListFile = "Ddp4.xml";
			masterTemplate.Add(ddp4Field);

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
