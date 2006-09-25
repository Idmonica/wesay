using System;
using System.IO;
using System.Reflection;
using System.Xml;
using WeSay.Language;

namespace WeSay.UI
{
	public class BasilProject : IProject, IDisposable
	{
	  private static BasilProject _singleton;

	  protected static BasilProject Singleton
	  {
		get
		{
		  return BasilProject._singleton;
		}
		set
		{
		  BasilProject._singleton = value;
		}
	  }
		private WritingSystemCollection _writingSystems;
		private string _projectDirectoryPath;
		private string _stringCatalogSelector;

		public event EventHandler HackedEditorsSaveNow;

		public static BasilProject Project
		{
			get
			{
				if (_singleton == null)
				{
				  throw new InvalidOperationException("BasilProject Not initialized. For tests, call BasilProject.InitializeForTests().");
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


		public BasilProject()
		{
			Project =this;
			_writingSystems = new WritingSystemCollection();
		}

		public virtual void Load(string projectDirectoryPath)
		{
			Load(projectDirectoryPath, false);
		}

		/// <summary>
		/// Tests can use this version
		/// </summary>
		/// <param name="projectDirectoryPath"></param>
		/// <param name="dontInitialize"></param>
		public virtual void Load(string projectDirectoryPath, bool dontInitialize)
		{
			this._projectDirectoryPath = projectDirectoryPath;
			if (!dontInitialize)
			{
				InitStringCatalog();
				InitWritingSystems();
			}
		}

		public virtual void Create(string projectDirectoryPath)
		{
			this._projectDirectoryPath = projectDirectoryPath;
			Directory.CreateDirectory(this.CommonDirectory);
			InitStringCatalog();
			InitWritingSystems();
			Save();
		}

		public virtual void Save()
		{
		   Save(_projectDirectoryPath);
		   if (HackedEditorsSaveNow != null)
		   {
			   HackedEditorsSaveNow.Invoke(this, null);
		   }
		}

	   public virtual void Save(string projectDirectoryPath)
		{
			XmlWriter writer = XmlWriter.Create(this.PathToWritingSystemPrefs);
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
			BasilProject project = new BasilProject();
			project.Load(GetPretendProjectDirectory());
//            project.InitWritingSystems();
//            project.InitStringCatalog();
			project.StringCatalogSelector = "en";
		}

		public static string GetPretendProjectDirectory()
		{
			return Path.Combine(GetTopAppDirectory(), "SampleProjects/PRETEND");
		}

		public WritingSystemCollection WritingSystems
		{
			get
			{
				return _writingSystems;
			}
		}

		protected string ProjectDirectoryPath
		{
			get
			{
				return _projectDirectoryPath;
			}
		}

		public string PathToWritingSystemPrefs
		{
			get
			{
				return Path.Combine(CommonDirectory, "writingSystemPrefs.xml");
			}
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

			else return null;
		}


		public string PathToStringCatalogInProjectDir
		{
			get
			{
				return Path.Combine(CommonDirectory, _stringCatalogSelector+".po");
			}
		}

		public string ApplicationCommonDirectory
		{
			get
			{
				return Path.Combine(GetTopAppDirectory(), "common");
			}
		}

		protected static string GetTopAppDirectory()
		{
			string path = Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "");
			//go up to dir containing the executable
			path = Directory.GetParent(path).FullName;
			if (path.ToLower().IndexOf("output") > -1)
			{
				//go up to output
				path = Directory.GetParent(path).FullName;
				//go up to directory containing output
				path = Directory.GetParent(path).FullName;
			}
			return path;
		}

		private string CommonDirectory
		{
			get
			{
				return Path.Combine(_projectDirectoryPath, "common");
			}
		}

		public string Name
		{
			get
			{
				//we don't really want to give this directory out... this is just for a test
				return Path.GetFileName(this._projectDirectoryPath);
			}
		}


		public void Dispose()
		{
			if (_singleton == this)
			{
				_singleton = null;
			}
		}

		 public void InitWritingSystems()
		{
			if (File.Exists(this.PathToWritingSystemPrefs))
			{
				_writingSystems.Load(this.PathToWritingSystemPrefs);
			}
		}


		public string StringCatalogSelector
		{
			get { return _stringCatalogSelector; }
			set { _stringCatalogSelector = value; }
		}


		public void InitStringCatalog()
		{
			try
			{
				string p = this.LocateStringCatalog();
				if (p == null)
				{
					new StringCatalog();
				}
				else
				{
					new StringCatalog(p);
				}
			}
			catch(FileNotFoundException )
			{
				//todo:when we add logging, this would be a good place to log a problem
				new StringCatalog();
			}
		}


	}
}
