using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.i8n;
using WeSay.Foundation;
using WeSay.LexicalModel;
using WeSay.Project;

namespace WeSay.LexicalTools
{
	public abstract class TaskBase: ITask
	{
		public const int CountNotRelevant = -1;
		public const int CountNotComputed = -2;

		private readonly LexEntryRepository _lexEntryRepository;
		private readonly string _label;
		private readonly string _description;
		private readonly bool _isPinned;
		private readonly string _cachePath;
		private readonly string _cacheFilePath;
		private int _remainingCount;
		private int _referenceCount;

		public TaskBase(string label,
						string description,
						bool isPinned,
						LexEntryRepository lexEntryRepository)
		{
			if (label == null)
			{
				throw new ArgumentNullException("label");
			}
			if (description == null)
			{
				throw new ArgumentNullException("description");
			}
			if (lexEntryRepository == null)
			{
				throw new ArgumentNullException("lexEntryRepository");
			}
			_lexEntryRepository = lexEntryRepository;
			_label = label;
			_description = description;
			_isPinned = isPinned;

			_cachePath = WeSayWordsProject.Project.PathToCache;
			_cacheFilePath = Path.Combine(_cachePath, MakeSafeName(Label + ".cache"));

			ReadCacheFile();
		}

		public virtual string Description
		{
			get { return StringCatalog.Get(_description); }
		}

		private bool _isActive = false;

		public virtual void Activate()
		{
			if (IsActive)
			{
				throw new InvalidOperationException(
						"Activate should not be called when object is active.");
			}
			IsActive = true;
		}

		public bool MustBeActivatedDuringPreCache
		{
			get { return true; }
		}

		private static string MakeSafeName(string fileName)
		{
			foreach (char invalChar in Path.GetInvalidFileNameChars())
			{
				fileName = fileName.Replace(invalChar.ToString(), "");
			}
			return fileName;
		}

		private void WriteCacheFile()
		{
			try
			{
				if (!Directory.Exists(_cachePath))
				{
					Directory.CreateDirectory(_cachePath);
				}
				using (StreamWriter sw = File.CreateText(_cacheFilePath))
				{
					sw.Write(_remainingCount + ", " + _referenceCount);
				}
			}
			catch
			{
				Console.WriteLine("Could not write cache file: " + _cacheFilePath);
			}
		}

		private void ReadCacheFile()
		{
			_remainingCount = CountNotComputed;
			_referenceCount = CountNotComputed;
			try
			{
				if (File.Exists(_cacheFilePath))
				{
					using (StreamReader sr = new StreamReader(_cacheFilePath))
					{
						string s;
						s = sr.ReadToEnd();
						string[] values = s.Split(',');
						if (values.Length > 1) //old style didn't have reference
						{
							bool gotIt = int.TryParse(values[1], out _referenceCount);
							Debug.Assert(gotIt);
						}
						if (values.Length > 0) //old style didn't have reference
						{
							bool gotIt = int.TryParse(values[0], out _remainingCount);
							Debug.Assert(gotIt);
						}
					}
				}
			}
			catch
			{
				// Console.WriteLine("Could not read cache file: " + cacheFilePath);
			}
		}

		public virtual void Deactivate()
		{
			if (!IsActive)
			{
				throw new InvalidOperationException(
						"Deactivate should only be called once after Activate.");
			}
			IsActive = false;
		}

		#region ITask Members

		public virtual void GoToUrl(string url)
		{
			throw new NotImplementedException();
		}

		#endregion

		protected void VerifyTaskActivated()
		{
			if (!IsActive)
			{
				throw new InvalidOperationException("Task must be activated first");
			}
		}

		public string Label
		{
			get { return StringCatalog.Get(_label); }
		}

		/// <summary>
		/// The control associated with this task
		/// </summary>
		/// <remarks>Non null only when task is activated</remarks>
		public abstract Control Control { get; }

		public bool IsPinned
		{
			get { return _isPinned; }
		}

		/// <summary>
		/// Gives a sense of how much work is left to be done
		/// </summary>
		public int GetRemainingCount()
		{
			int count = ComputeCount(false);
			if (count != CountNotComputed)
			{
				_remainingCount = count;
				WriteCacheFile();
			}
			return _remainingCount;
		}

		/// <summary>
		/// Should Return CountNotComputed if expensive to figure out
		/// and force==false
		/// </summary>
		/// <returns>An integer indicating how much work is left to do</returns>
		protected abstract int ComputeCount(bool returnResultEvenIfExpensive);

		public int ExactCount
		{
			get
			{
				_remainingCount = ComputeCount(true);
				WriteCacheFile();
				return _remainingCount;
			}
		}

		/// <summary>
		/// Gives a sense of the overall size of the task
		/// </summary>
		public int GetReferenceCount()
		{
			int count = ComputeReferenceCount();
			if (count != CountNotComputed)
			{
				_referenceCount = count;
				WriteCacheFile();
			}

			return _referenceCount;
		}

		/// <summary>
		/// Should Return CountNotComputed if expensive to figure out
		/// </summary>
		/// <returns>
		/// An integer indicating how much work there is total
		/// </returns>
		protected abstract int ComputeReferenceCount();

		protected LexEntryRepository LexEntryRepository
		{
			get { return _lexEntryRepository; }
		}

		public bool IsActive
		{
			get { return _isActive; }
			protected set { _isActive = value; }
		}

		#region IThingOnDashboard Members

		public virtual DashboardGroup Group
		{
			get { return DashboardGroup.Describe; }
		}

		public string LocalizedLabel
		{
			get { return StringCatalog.Get(_label); }
		}

		public virtual ButtonStyle DashboardButtonStyle
		{
			get { return ButtonStyle.VariableAmount; }
		}

		public virtual Image DashboardButtonImage
		{
			get { return null; }
		}

		#endregion
	}
}