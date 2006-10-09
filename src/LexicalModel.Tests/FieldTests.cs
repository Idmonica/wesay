using System;
using NUnit.Framework;

namespace WeSay.LexicalModel.Tests
{
	[TestFixture]
	public class FieldTests
	{
		[SetUp]
		public void Setup()
		{

		}

		[TearDown]
		public void TearDown()
		{

		}

		[Test]
		public void CreateNoWritingSystem()
		{
			Field field = new Field("fieldName", new string[] { });
			Assert.IsNotNull(field);
		}

		[Test]
		public void CreateSingleWritingSystem()
		{
			Field field = new Field("fieldName", new string[] { "writingSystemId" });
			Assert.IsNotNull(field);
		}

		[Test]
		public void CreateMultipleWritingSystems()
		{
			Field field = new Field("fieldName", new string[] {"writingSystemId1", "writingSystemId2", "writingSystemId3"});
			Assert.IsNotNull(field);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Create_NullFieldName_Throws()
		{
			new Field(null, new string[] { "writingSystem" });
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Create_NullWritingSystem_Throws()
		{
			new Field("fieldName", null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Create_NullWritingSystems_Throws()
		{
			new Field("fieldName", new string[] { null, null });
		}

		[Test]
		public void HasWritingSystem_Null_False()
		{
			Field field = new Field("fieldName", new string[] {"writingSystemId1", "writingSystemId2", "writingSystemId3"});
			Assert.IsFalse(field.HasWritingSystem(null));
		}

		[Test]
		public void HasWritingSystem_Exists_True()
		{
			Field field = new Field("fieldName", new string[] {"writingSystemId1", "writingSystemId2", "writingSystemId3"});
			Assert.IsTrue(field.HasWritingSystem("writingSystemId3"));
		}

		[Test]
		public void HasWritingSystem_NotExists_False()
		{
			Field field = new Field("fieldName", new string[] {"writingSystemId1", "writingSystemId2", "writingSystemId3"});
			Assert.IsFalse(field.HasWritingSystem("3"));
			Assert.IsFalse(field.HasWritingSystem(String.Empty));
		}

		[Test]
		public void GetFieldName_InitializedFromConstructor()
		{
			Field field = new Field("fieldName", new string[] { "writingSystemId" });
			Assert.AreEqual("fieldName", field.FieldName);
		}

		[Test]
		public void GetWritingSystems()
		{
			Field field = new Field("fieldName", new string[] {"writingSystemId1", "writingSystemId2", "writingSystemId3"});
			Assert.AreEqual(3, field.WritingSystemIds.Count);
			Assert.Contains("writingSystemId1", field.WritingSystemIds);
		}

	}
}