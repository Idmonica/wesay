using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using Palaso.Text;
using WeSay.Foundation;

namespace Palaso.Text
{
	public class MultiTextBase : INotifyPropertyChanged
	{
		/// <summary>
		/// We have this pesky "backreference" solely to enable fast
		/// searching in our current version of db4o (5.5), which
		/// can find strings fast, but can't be queried for the owner
		/// quickly, if there is an intervening collection.  Since
		/// each string in WeSay is part of a collection of writing
		/// system alternatives, that means we can't quickly get
		/// an answer, for example, to the question Get all
		/// the Entries that contain a senes which matches the gloss "cat".
		///
		/// Using this field, we can do a query asking for all
		/// the LanguageForms matching "cat".
		/// This can all be done in a single, fast query.
		///  In code, we can then follow the
		/// LanguageForm._parent up to the MultiTextBase, then this _parent
		/// up to it's owner, etc., on up the hierarchy to get the Entries.
		///
		/// Subclasses should provide a property which set the proper class.
		///
		/// 23 Jan 07, note: starting to switch to using these for notifying parent of changes, too.
		/// </summary>

		/// <summary>
		/// For INotifyPropertyChanged
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		private LanguageForm[] _forms;
		public MultiTextBase()
		{
			_forms = new LanguageForm[0];
		}



		public void Add(Object objectFromSerializer) {}

		public static bool IsEmpty(MultiTextBase mt)
		{
			return mt == null || mt.Empty;
		}

		public static MultiTextBase Create(Dictionary<string, string> forms)
		{
			MultiTextBase m = new MultiTextBase();
			CopyForms(forms, m);
			return m;
		}

		protected static void CopyForms(Dictionary<string, string> forms, MultiTextBase m)
		{
			if (forms != null && forms.Keys != null)
			{
				foreach (string key in forms.Keys)
				{
					LanguageForm f = m.Find(key);
					if (f != null)
					{
						f.Form = forms[key];
					}
					else
					{
						m.SetAlternative(key, forms[key]);
					}
				}
			}
		}

		public void SetAnnotationOfAlternativeIsStarred(string id, bool isStarred)
		{
			LanguageForm alt = Find(id);
			if (isStarred)
			{
				if (alt == null)
				{
					AddLanguageForm(new LanguageForm(id, String.Empty, this));
					alt = Find(id);
					Debug.Assert(alt != null);
				}
				alt.IsStarred = true;
			}
			else
			{
				if (alt != null)
				{
					if (alt.Form == String.Empty) //non-starred and empty? Nuke it.
					{
						RemoveLanguageForm(alt);
					}
					else
					{
						alt.IsStarred = false;
					}
				}
				else
				{
					//nothing to do.  Missing altertive == not starred.
				}
			}
			NotifyPropertyChanged(id);
		}


		[XmlArrayItem(typeof (LanguageForm), ElementName = "tobedetermined")]
		public string this[string writingSystemId]
		{
			get { return GetExactAlternative(writingSystemId); }
			set { SetAlternative(writingSystemId, value); }
		}

		public LanguageForm Find(string writingSystemId)
		{
			foreach (LanguageForm f in Forms)
			{
				if (f.WritingSystemId == writingSystemId)
				{
					return f;
				}
			}
			return null;
		}

		/// <summary>
		/// Throws exception if alternative does not exist.
		/// </summary>
		/// <param name="writingSystemId"></param>
		/// <returns></returns>
//        public string GetExactAlternative(string writingSystemId)
//        {
//            if (!Contains(writingSystemId))
//            {
//                throw new ArgumentOutOfRangeException("Use Contains() to first check if the MultiTextBase has a language form for this writing system.");
//            }
//
//            return GetBestAlternative(writingSystemId, false, null);
//        }
		public bool ContainsAlternative(string writingSystemId)
		{
			return (Find(writingSystemId) != null);
		}

		/// <summary>
		/// Get exact alternative or String.Empty
		/// </summary>
		/// <param name="writingSystemId"></param>
		/// <returns></returns>
		public string GetExactAlternative(string writingSystemId)
		{
			return GetAlternative(writingSystemId, false, null);
		}

		/// <summary>
		/// Gives the string of the requested id if it exists, else the 'first'(?) one that does exist, else Empty String
		/// </summary>
		/// <returns></returns>
		public string GetBestAlternative(string writingSystemId)
		{
			return GetAlternative(writingSystemId, true, string.Empty);
		}

		public string GetBestAlternative(string writingSystemId, string notFirstChoiceSuffix)
		{
			return GetAlternative(writingSystemId, true, notFirstChoiceSuffix);
		}

		/// <summary>
		/// Get a string out
		/// </summary>
		/// <returns>the string of the requested id if it exists,
		/// else the 'first'(?) one that does exist + the suffix,
		/// else the given suffix </returns>
		private string GetAlternative(string writingSystemId, bool doShowSomethingElseIfMissing,
									  string notFirstChoiceSuffix)
		{
			LanguageForm alt = Find(writingSystemId);
			if (null == alt)
			{
				if (doShowSomethingElseIfMissing)
				{
					return GetFirstAlternative() + notFirstChoiceSuffix;
				}
				else
				{
					return string.Empty;
				}
			}
			string form = alt.Form;
			if (form == null || (form.Trim().Length == 0))
			{
				if (doShowSomethingElseIfMissing)
				{
					return GetFirstAlternative() + notFirstChoiceSuffix;
				}
				else
				{
					return string.Empty;
				}
			}
			else
			{
				return form;
			}
		}

		public string GetFirstAlternative()
		{
			foreach (LanguageForm form in Forms)
			{
				if (form.Form.Trim().Length > 0)
				{
					return form.Form;
				}
			}
			return string.Empty;
		}

		public string GetBestAlternativeString(IEnumerable<string> orderedListOfWritingSystemIds)
		{
			LanguageForm form = GetBestAlternative(orderedListOfWritingSystemIds);
			if (form == null)
				return string.Empty;
			return form.Form;
		}

		/// <summary>
		/// Try to get an alternative according to the ws's given(e.g. the enabled writing systems for a field)
		/// </summary>
		/// <param name="orderedListOfWritingSystemIds"></param>
		/// <returns>May return null!</returns>
		public LanguageForm GetBestAlternative(IEnumerable<string> orderedListOfWritingSystemIds)
		{
			foreach (string id in orderedListOfWritingSystemIds)
			{
				LanguageForm alt = Find(id);
				if (null != alt)
					return alt;
			}

//            //just send back an empty
//            foreach (string id in orderedListOfWritingSystemIds)
//            {
//                return new LanguageForm(id, string.Empty );
//            }
			return null;
		}

		public bool Empty
		{
			get { return Count == 0; }
		}

		public int Count
		{
			get { return Forms.Length; }
		}

		/// <summary>
		/// just for deserialization
		/// </summary>
		[XmlElement(typeof (LanguageForm), ElementName="form")]

		public LanguageForm[] Forms
		{
			get
			{
				Debug.Assert(_forms != null, "Forms was null. Is this an old cache?");
				if (_forms == null)
				{
					_forms = new LanguageForm[0];
				}
				return _forms;
			}
			set { _forms = value; }
		}


		public void SetAlternative(string writingSystemId, string form)
		{
			Debug.Assert(writingSystemId != null && writingSystemId.Length > 0, "The writing system id was empty.");
			Debug.Assert(writingSystemId.Trim() == writingSystemId,
						 "The writing system id had leading or trailing whitespace");

			//enhance: check to see if there has actually been a change

			LanguageForm alt = Find(writingSystemId);
			if (form == null || form.Length == 0) // we don't use space to store empty strings.
			{
				if (alt != null && !alt.IsStarred)
				{
					RemoveLanguageForm(alt);
				}
			}
			else
			{
				if (alt != null)
				{
					alt.Form = form;
				}
				else
				{
					AddLanguageForm(new LanguageForm(writingSystemId, form, this));
				}
			}

			NotifyPropertyChanged(writingSystemId);
		}

		protected void RemoveLanguageForm(LanguageForm languageForm)
		{
			Debug.Assert(Forms.Length > 0);
			LanguageForm[] forms = new LanguageForm[Forms.Length - 1];
			for (int i = 0, j = 0; i < forms.Length; i++,j++)
			{
				if (Forms[j] == languageForm)
				{
					j++;
				}
				forms[i] = Forms[j];
			}
			_forms = forms;
		}

		protected void AddLanguageForm(LanguageForm languageForm)
		{
			LanguageForm[] forms = new LanguageForm[Forms.Length + 1];
			for (int i = 0; i < Forms.Length; i++)
			{
				forms[i] = Forms[i];
			}

			//actually copy the contents, as we must now be the parent
			forms[Forms.Length] = new LanguageForm(languageForm.WritingSystemId, languageForm.Form, this);
			_forms = forms;
		}

		protected void NotifyPropertyChanged(string writingSystemId)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(writingSystemId));
			}
		}

		public IEnumerator GetEnumerator()
		{
			return Forms.GetEnumerator();
		}

		public override string ToString()
		{
			return GetFirstAlternative();
		}

		public void MergeInWithAppend(MultiTextBase incoming, string separator)
		{
			foreach (LanguageForm form in incoming)
			{
				LanguageForm f = Find(form.WritingSystemId);
				if (f != null)
				{
					f.Form += separator + form.Form;
				}
				else
				{
					AddLanguageForm(form); //this actually copies the meat of the form
				}
			}
		}

		public void MergeIn(MultiTextBase incoming)
		{
			foreach (LanguageForm form in incoming)
			{
				LanguageForm f = Find(form.WritingSystemId);
				if (f != null)
				{
					f.Form = form.Form;
				}
				else
				{
					AddLanguageForm(form); //this actually copies the meat of the form
				}
			}
		}

		public bool Equals(MultiTextBase other)
		{
			if (other.Count != Count)
			{
				return false;
			}
			foreach (LanguageForm form in other)
			{
				if (!ContainsEqualForm(form))
				{
					return false;
				}
			}
			return true;
		}

		public bool HasFormWithSameContent(MultiTextBase other)
		{
			if (other.Count == 0 && Count == 0)
			{
				return true;
			}
			foreach (LanguageForm form in other)
			{
				if (ContainsEqualForm(form))
				{
					return true;
				}
			}
			return false;
		}

		public bool ContainsEqualForm(LanguageForm other)
		{
			foreach (LanguageForm form in Forms)
			{
				if (other.Equals(form))
				{
					return true;
				}
			}
			return false;
		}
	}
}