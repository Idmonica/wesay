﻿using System.Drawing;
using NUnit.Framework;
using Palaso.WritingSystems;
using WeSay.LexicalModel.Foundation;

namespace WeSay.LexicalModel.Tests.Foundation
{
	[TestFixture]
	public class WritingSystemInfoTests
	{
		[Test]

		public void CreateFont_Default_GetReturnsGenericSansSerif()
		{
			var ws = new WritingSystemDefinition();
			Assert.AreEqual(FontFamily.GenericSansSerif, WritingSystemInfo.CreateFont(ws).FontFamily);
		}

		[Test]
		public void CreateFont_Default_GetFontSizeIs12()
		{
			var ws = new WritingSystemDefinition();
			Assert.AreEqual(12, WritingSystemInfo.CreateFont(ws).Size);
		}

		[Test]
		public void CreateFont_WithFontName_NameSetToFontName()
		{
			var ws = new WritingSystemDefinition
						 {
							 DefaultFontName = FontFamily.GenericSerif.Name
						 };
			// Assert the precondition
			Assert.AreNotEqual(FontFamily.GenericSansSerif.Name, ws.DefaultFontName);
			// Assert the test
			Assert.AreEqual(WritingSystemInfo.CreateFont(ws).Name, ws.DefaultFontName);
		}
	}
}