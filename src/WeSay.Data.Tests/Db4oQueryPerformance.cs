using System;
using System.Collections.Generic;
using System.IO;
using Db4objects.Db4o;
using Db4objects.Db4o.Config;
using NUnit.Framework;

namespace WeSay.Data.Tests
{
	[TestFixture]
	public class Db4oQueryPerformance
	{

		class Entry
		{
			public MultiText name;
		}

		class MultiText
		{
			public LanguageForm _singleForm;
			public LanguageForm[] _forms;
			public MultiText()
			{
				_forms = new LanguageForm[2];
			}
		}

		class LanguageForm
		{
			private string _writingSystemId;
			private string _form;

			public LanguageForm(string writingSystemId, string form)
			{
				_writingSystemId = writingSystemId;
				_form = form;
			}

			public string WritingSystemId
			{
				get { return this._writingSystemId; }
			}

			public string Form
			{
				get { return this._form; }
			}
		}

		protected IObjectContainer _db;
		protected string _filePath;
		private static IConfiguration _db4oConfiguration;

		[SetUp]
		public void SetUp()
		{
			_filePath = Path.GetTempFileName();
			//Db4objects.Db4o.Config.IConfiguration db4oConfiguration = Db4oFactory.Configure();
			//Db4objects.Db4o.Config.IObjectClass objectClass = db4oConfiguration.ObjectClass(typeof(LanguageForm));
			// objectClass.ObjectField("_writingSystemId").Indexed(true);
			// objectClass.ObjectField("_form").Indexed(true);

			_db = Db4oFactory.OpenFile(_filePath);
			((YapStream)_db).GetNativeQueryHandler().QueryOptimizationFailure += new Db4objects.Db4o.Inside.Query.QueryOptimizationFailureHandler(OnQueryOptimizationFailure);

			for (int i = 0; i < 10000; i++)
			{
				Entry e = new Entry();
				e.name = new MultiText();
				e.name._forms[0] = new LanguageForm("en", "en-" + i);
				//e.name._forms[1] = new LanguageForm("fr", "fr-"+i);
				e.name._singleForm = new LanguageForm("en", i.ToString());
				_db.Set(e);
			}

			//            _db.Commit();
			//            _db.Dispose();
			//            _db = Db4objects.Db4o.Db4oFactory.OpenFile(_filePath);
		}

		[TearDown]
		public void TearDown()
		{
			this._db.Dispose();
			File.Delete(_filePath);
		}

		[Test]
		public void FindWithNativeUsingArray()
		{
			System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

			stopwatch.Start();
			IList<Entry> matches = _db.Query<Entry>(delegate(Entry e)
								  {
									  return e.name._forms[0].Form == "en-99";
								  });
			stopwatch.Stop();
			Assert.AreEqual(1, matches.Count);
			Console.WriteLine("FindWithNativeUsingArray " + stopwatch.ElapsedMilliseconds / 1000.0 + " seconds");
		}

		[Test]
		public void FindWithNativeNoArray()
		{
			System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

			stopwatch.Start();
			IList<Entry> matches = _db.Query<Entry>(delegate(Entry e)
								  {
									  return e.name._singleForm.Form == "99";
								  });
			stopwatch.Stop();
			Assert.AreEqual(1, matches.Count);
			Console.WriteLine("FindWithNativeNoArray " + stopwatch.ElapsedMilliseconds / 1000.0 + " seconds");
		}


		[Test]
		public void FindRawForm()
		{
			System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

			stopwatch.Start();
			IList<LanguageForm> matches = _db.Query<LanguageForm>(delegate(LanguageForm f)
								  {
									  return f.Form == "99";
								  });
			stopwatch.Stop();
			Assert.AreEqual(1, matches.Count);
			Console.WriteLine("FindRawForm " + stopwatch.ElapsedMilliseconds / 1000.0 + " seconds");
		}


		[Test]
		public void SimpleStringSearch()
		{
			IObjectContainer db = MakeFlatStringDatabase(true);
			System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

			stopwatch.Start();
			IList<LanguageForm> matches = db.Query<LanguageForm>(delegate(LanguageForm f)
								  {
									  return f.Form == "99";
								  });
			stopwatch.Stop();
			Assert.AreEqual(1, matches.Count);
			Console.WriteLine("SimpleStringSearch " + stopwatch.ElapsedMilliseconds / 1000.0 + " seconds");

			db.Dispose();
			_db4oConfiguration.Diagnostic().RemoveAllListeners();
		}


		class Foo
		{
			public DateTime _time;
		}

		[Test, Ignore("fails in db4o 6.0")]
		public void DateTimeShouldNotLooseKind()
		{
			using (Db4oDataSource db1 = new Db4oDataSource("test.yap"))
			{
				Foo stored = new Foo();
				stored._time = DateTime.UtcNow;
				Assert.AreEqual(DateTimeKind.Utc, stored._time.Kind);
				db1.Data.Set(stored);
				db1.Data.Close();
			}
			using (Db4oDataSource db2 = new Db4oDataSource("test.yap"))
			{
				IObjectSet results = db2.Data.Get(null);
				Foo retrieved = (Foo) results[0];
				Assert.AreEqual(DateTimeKind.Utc, retrieved._time.Kind);
				db2.Data.Close();
			}
		}

		static private IObjectContainer MakeFlatStringDatabase(bool doIndex)
		{
			string path = Path.GetTempFileName();
			_db4oConfiguration = Db4oFactory.Configure();
			if (doIndex)
			{
				IObjectClass objectClass = _db4oConfiguration.ObjectClass(typeof(LanguageForm));
				objectClass.ObjectField("_form").Indexed(true);
			}

			Db4objects.Db4o.Diagnostic.DiagnosticToConsole listener = new Db4objects.Db4o.Diagnostic.DiagnosticToConsole();
			_db4oConfiguration.Diagnostic().AddListener(listener);

			IObjectContainer db = Db4oFactory.OpenFile(path);
			((YapStream)db).GetNativeQueryHandler().QueryOptimizationFailure += new Db4objects.Db4o.Inside.Query.QueryOptimizationFailureHandler(OnQueryOptimizationFailure);

			for (int i = 0; i < 10000; i++)
			{
				LanguageForm f = new LanguageForm("en", i.ToString());
				db.Set(f);
			}
			db.Commit();
			return db;
		}


		static void OnQueryOptimizationFailure(object sender, Db4objects.Db4o.Inside.Query.QueryOptimizationFailureEventArgs args)
		{
			Console.WriteLine("Query not Optimized:");
			Console.WriteLine(args.Reason);
		}


		/* all times from running only the one test in isolation, in test runner without debugging on
		 * with index on _form
		 * FindWithNativeUsingArray 2.765 seconds
		   FindWithNativeNoArray 1.046 seconds
		 * FindRawForm 0.55 seconds
		 *
		 *
		 * no index
		 * FindWithNativeUsingArray 2.765 seconds
			FindWithNativeNoArray 1.051 seconds
		 *  FindRawForm 0.864 seconds
		 */
	}
}
