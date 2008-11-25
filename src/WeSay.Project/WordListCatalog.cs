﻿using System;
using System.Collections.Generic;
using System.Text;

namespace WeSay.Project
{
	/// <summary>
	/// this catalog can eventually come by inspecting word lists found in a folder or something
	/// ideally, the files containing the lists would be self describing (and be lift files, with
	/// all the meta data for each entry).
	/// </summary>
	public class WordListCatalog : Dictionary<string, WordListDescription>
	{
		public WordListCatalog()
		{
			//SILCAWL
			Add("DuerksenWords.txt", new WordListDescription("en", "SILCA Word List", "Gather words using the SIL Comparative African Wordlist", "Collect new words by translating from words in another language. This is a list of 1700 words."));
			Add("PNGWords.txt", new WordListDescription("en", "PNG Word List", "Gather words using the PNG Word List", "Collect new words by translating from words in another language. This is a list of 900 words and phrases."));
		}
	}

	public class WordListDescription
	{
		public string WritingSystemId { get; set; }
		public string Label { get; set; }
		public string LongLabel { get; set; }
		public string Description { get; set; }

		public WordListDescription(string writingSystemId, string label, string longLabel, string description)
		{
			WritingSystemId = writingSystemId;
			Label = label;
			LongLabel = longLabel;
			Description = description;
		}
	}
}
