using System;
using System.IO;
using System.Reflection;
using System.Xml;
using Reporting;
using WeSay.Language;

namespace WeSay.Project
{
	public class BasilProject : IProject, IDisposable
	{
		private static BasilProject _singleton;
		private string _uiFontName;

		protected static BasilProject Singleton
		{
			get { return _singleton; }
			set { _singleton = value; }
		}

		private WritingSystemCollection _writingSystems;
		private string _projectDirectoryPath = string.Empty;
		private string _stringCatalogSelector = string.Empty;

		public static BasilProject Project
		{
			get
			{
				if (_singleton == null)
				{
					throw new InvalidOperationException(
							"BasilProject Not initialized. For tests, call BasilProject.InitializeForTests().");
				}
				return _singleton;
			}
			set
			{
				if (_singleton != null)
				{
					_singleton.Dispose();
				}
				_singleton = value;
			}
		}

		public static bool IsInitialized
		{
			get { return _singleton != null; }
		}

		public BasilProject()
		{
			Project = this;
			_writingSystems = new WritingSystemCollection();
		}

		public virtual void LoadFromProjectDirectoryPath(string projectDirectoryPath)
		{
			LoadFromProjectDirectoryPath(projectDirectoryPath, false);
		}

		/// <summary>
		/// Tests can use this version
		/// </summary>
		/// <param name="projectDirectoryPath"></param>
		/// <param name="dontInitialize"></param>
		public virtual void LoadFromProjectDirectoryPath(string projectDirectoryPath, bool dontInitialize)
		{
			_projectDirectoryPath = projectDirectoryPath;
			if (!dontInitialize)
			{
				InitStringCatalog();
				InitWritingSystems();
			}
		}

		public virtual void CreateEmptyProjectFiles(string projectDirectoryPath)
		{
			_projectDirectoryPath = projectDirectoryPath;
			Directory.CreateDirectory(ProjectCommonDirectory);
			InitStringCatalog();
			InitWritingSystems();
			Save();
		}

		public virtual void Save()
		{
			Save(_projectDirectoryPath);
		}

		public virtual void Save(string projectDirectoryPath)
		{
			XmlWriter writer = XmlWriter.Create(PathToWritingSystemPrefs);
			_writingSystems.Write(writer);
			writer.Close();
		}

		/// <summary>
		/// Many tests throughout the system will not care at all about project related things,
		/// but they will break if there is no project initialized, since many things
		/// will reach the project through a static property.
		/// Those tests can just call this before doing anything else, so
		/// that other things don't break.
		/// </summary>
		public static void InitializeForTests()
		{
			ErrorReport.OkToInteractWithUser = false;
			BasilProject project = new BasilProject();
			project.LoadFromProjectDirectoryPath(GetPretendProjectDirectory());
			project.StringCatalogSelector = "en";
		}

		public static string GetPretendProjectDirectory()
		{
			return Path.Combine(GetTopAppDirectory(), Path.Combine("SampleProjects", "PRETEND"));
		}

		public WritingSystemCollection WritingSystems
		{
			get { return _writingSystems; }
		}

		public string ProjectDirectoryPath
		{
			get { return _projectDirectoryPath; }
			protected set { _projectDirectoryPath = value; }
		}

		public string PathToWritingSystemPrefs
		{
			get { return GetPathToWritingSystemPrefs(ProjectCommonDirectory); }
		}

//        public string PathToOptionsLists
//        {
//            get
//            {
//                return GetPathToWritingSystemPrefs(CommonDirectory);
//            }
//        }

		private static string GetPathToWritingSystemPrefs(string parentDir)
		{
			return Path.Combine(parentDir, "writingSystemPrefs.xml");
		}

		public string LocateStringCatalog()
		{
			if (File.Exists(PathToStringCatalogInProjectDir))
			{
				return PathToStringCatalogInProjectDir;
			}

			//fall back to the program's common directory
			string path = Path.Combine(ApplicationCommonDirectory, _stringCatalogSelector + ".po");
			if (File.Exists(path))
			{
				return path;
			}

			else
			{
				return null;
			}
		}

		public string PathToStringCatalogInProjectDir
		{
			get { return Path.Combine(ProjectCommonDirectory, _stringCatalogSelector + ".po"); }
		}

		public string ApplicationCommonDirectory
		{
			get { return Path.Combine(GetTopAppDirectory(), "common"); }
		}

		public string ApplicationTestDirectory
		{
			get { return Path.Combine(GetTopAppDirectory(), "test"); }
		}

		protected static string GetTopAppDirectory()
		{
			string path;

			path = DirectoryOfExecutingAssembly;

			if (path.ToLower().IndexOf("output") > -1)
			{
				//go up to output
				path = Directory.GetParent(path).FullName;
				//go up to directory containing output
				path = Directory.GetParent(path).FullName;
			}
			return path;
		}

		public static string DirectoryOfExecutingAssembly
		{
			get
			{
				string path;
				bool unitTesting = Assembly.GetEntryAssembly() == null;
				if (unitTesting)
				{
					path = new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;
					path = Uri.UnescapeDataString(path);
				}
				else
				{
					path = Assembly.GetExecutingAssembly().Location;
				}
				return Directory.GetParent(path).FullName;
			}
		}

		protected string ProjectCommonDirectory
		{
			get { return Path.Combine(_projectDirectoryPath, "common"); }
		}

		public string Name
		{
			get
			{
				//we don't really want to give this directory out... this is just for a test
				return Path.GetFileName(_projectDirectoryPath);
			}
		}

		public virtual void Dispose()
		{
			if (_singleton == this)
			{
				_singleton = null;
			}
		}

		private void InitWritingSystems()
		{
			if (File.Exists(PathToWritingSystemPrefs))
			{
				_writingSystems.Load(PathToWritingSystemPrefs);
			}
			else
			{
				//load defaults
				_writingSystems.Load(GetPathToWritingSystemPrefs(ApplicationCommonDirectory));
			}
		}

//        /// <summary>
//        /// Get the options lists, e.g. PartsOfSpeech, from files
//        /// </summary>
//        private void InitOptionsLists()
//        {
//            Directory.
//        }

		public string StringCatalogSelector
		{
			get { return _stringCatalogSelector; }
			set { _stringCatalogSelector = value; }
		}

		protected string UiFontName
		{
			get { return _uiFontName; }
			set { _uiFontName = value; }
		}

		private void InitStringCatalog()
		{
			try
			{
				if (_stringCatalogSelector == "test")
				{
					new StringCatalog("test", UiFontName);
				}
				string p = LocateStringCatalog();
				if (p == null)
				{
					new StringCatalog(UiFontName);
				}
				else
				{
					new StringCatalog(p, UiFontName);
				}
			}
			catch (FileNotFoundException)
			{
				//todo:when we add logging, this would be a good place to log a problem
				new StringCatalog();
			}
		}
	}
}