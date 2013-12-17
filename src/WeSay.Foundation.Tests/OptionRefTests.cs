using System;
using WeSay.Foundation.Options;
using NUnit.Framework;

namespace WeSay.Foundation.Tests
{
	[TestFixture]
	public class OptionRefTests
	{
		[Test]
		public void CompareTo_Null_ReturnsGreater()
		{
			OptionRef reference = new OptionRef();
			Assert.AreEqual(1, reference.CompareTo(null));
		}

		[Test]
		public void CompareTo_OtherHasGreaterKey_ReturnsLess()
		{
			OptionRef reference = new OptionRef();
			reference.Key = "key1";
			OptionRef other = new OptionRef();
			other.Key = "key2";
			Assert.AreEqual(-1, reference.CompareTo(other));
		}

		[Test]
		public void CompareTo_OtherHasLesserKey_ReturnsGreater()
		{
			OptionRef reference = new OptionRef();
			reference.Key = "key2";
			OptionRef other = new OptionRef();
			other.Key = "key1";
			Assert.AreEqual(1, reference.CompareTo(other));
		}

		[Test]
		public void CompareTo_OtherHasSameKey_ReturnsLesser()
		{
			OptionRef reference = new OptionRef();
			reference.Key = "key1";
			OptionRef other = new OptionRef();
			other.Key = "key1";
			Assert.AreEqual(0, reference.CompareTo(other));
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CompareTo_OtherIsNotOptionRef_Throws()
		{
			OptionRef reference = new OptionRef();
			string other = "";
			Assert.AreEqual(0, reference.CompareTo(other));
		}
	}
}
