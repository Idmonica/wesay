﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using WeSay.Project;

namespace WeSay.LexicalTools.GatherByWordList
{
	public interface IGatherWordListConfig : ITaskConfiguration
	{
		string WordListFileName
		{
			get;
		}
		string WordListWritingSystemId
		{
			get;
		}
	}


	public class GatherWordListConfig : TaskConfigurationBase, IGatherWordListConfig, ITaskConfiguration , ICareThatWritingSystemIdChanged
	{
		private readonly WordListCatalog _catalog;

		public GatherWordListConfig(string xml, WordListCatalog catalog)
			:base(xml)
		{
			_catalog = catalog;
		}

		protected override IEnumerable<KeyValuePair<string, string>> ValuesToSave
		{
			get
			{
				yield return new KeyValuePair<string, string>("wordListFileName", WordListFileName);
				yield return new KeyValuePair<string, string>("wordListWritingSystemId", WordListWritingSystemId);
			}
		}

		public bool AreEquivalent(ITaskConfiguration taskConfiguration)
		{
			return taskConfiguration is GatherWordListConfig && _catalog == ((GatherWordListConfig)taskConfiguration).Catalog;
		}

		protected WordListCatalog Catalog
		{
			get { return _catalog; }
		}

		public string WordListFileName
		{
			get
			{
				return GetStringFromConfigNode("wordListFileName");
			}
		}
		public string WordListWritingSystemId
		{
			get
			{
				return GetStringFromConfigNode("wordListWritingSystemId");
			}
		}


		public override string ToString()
		{
			return LongLabel;
		}

		private WordListDescription WordList
		{
			get { return _catalog.GetOrAddWordList(WordListFileName); }
		}

		public string Label
		{
			get { return WordList.Label; }
		}

		public string LongLabel
		{
			get { return WordList.LongLabel; }
		}

		public string Description
		{
			get { return WordList.Description; }
		}

		public string RemainingCountText
		{
			get { return "Remaining Words:"; }
		}

		public string ReferenceCountText
		{
			get { return string.Empty; }
		}

		public bool IsPinned
		{
			get { return false; }
		}



		public bool IsOptional
		{
			get { return true; }
		}

		public static IGatherWordListConfig CreateForTests(string wordListFileName, string wordListWritingSystemId, WordListCatalog catalog)
		{
			string xml = String.Format(@"   <task taskName='AddMissingInfo' visible='true'>
					  <wordListFileName>{0}</wordListFileName>
					  <wordListWritingSystemId>{1}</wordListWritingSystemId>
					</task>
				", wordListFileName, wordListWritingSystemId);

			return new GatherWordListConfig(xml, catalog);
		}



		public void WritingSystemIdChanged(string from, string to)
		{
			  //TODO, (maybe?) when we become writeable
			// if(WordListWritingSystemId==from)
			//      WordListWritingSystemId=to;
			// mark dirty if necessary
		}
	}
}