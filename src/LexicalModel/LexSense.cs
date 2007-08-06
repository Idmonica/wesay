using System.Collections.Generic;
using System.ComponentModel;
using WeSay.Data;
using WeSay.Foundation;
using WeSay.Language;

namespace WeSay.LexicalModel
{
	public sealed class LexSense : WeSayDataObject
	{
		private SenseGlossMultiText _gloss;
		private InMemoryBindingList<LexExampleSentence> _exampleSentences;

		new public class WellKnownProperties : WeSayDataObject.WellKnownProperties
		{

			static public string PartOfSpeech = "POS";
			static public string SemanticDomainsDdp4 = "SemanticDomainDdp4";
			static public string Definition = "definition";
			static public bool Contains(string fieldName)
			{
				List<string> list = new List<string>(new string[] { PartOfSpeech, Definition, SemanticDomainsDdp4 });
				return list.Contains(fieldName);
			}
		} ;

		public LexSense(WeSayDataObject parent)
			: base(parent)
		{
			_gloss = new SenseGlossMultiText(this);
			_exampleSentences = new InMemoryBindingList<LexExampleSentence>();
			WireUpEvents();
		}

		/// <summary>
		/// Used when a list of these items adds via "AddNew", where we have to have a default constructor.
		/// The parent is added in an event handler, on the parent, which is called by the list.
		/// </summary>
		public LexSense(): this(null)
		{
		}

		protected override void WireUpEvents()
		{
			base.WireUpEvents();
			WireUpChild(_gloss);
			WireUpList(_exampleSentences, "exampleSentences");
		}

		public MultiText Gloss
		{
			get { return _gloss; }
		}

		public IBindingList ExampleSentences
		{
			get { return _exampleSentences; }
		}

		public override bool IsEmpty
		{
			get {
				return Gloss.Empty &&
					   ExampleSentences.Count == 0 &&
					   !HasProperties;
			}
		}

		public override void CleanUpAfterEditting()
		{
			base.CleanUpAfterEditting();
			foreach (LexExampleSentence sentence in _exampleSentences)
			{
				sentence.CleanUpAfterEditting();
			}
			CleanUpEmptyObjects();
		}
		public override void CleanUpEmptyObjects()
		{
			base.CleanUpEmptyObjects();

			for (int i = 0; i < this._exampleSentences.Count; i++)
			{
				_exampleSentences[i].CleanUpEmptyObjects();
			}

			// remove any example sentences that are empty
			int count = this._exampleSentences.Count;

			for (int i = count - 1; i >= 0 ; i--)
			{
				if(this._exampleSentences[i].IsEmpty)
				{
					this._exampleSentences.RemoveAt(i);
				}
			}
			if(count != this._exampleSentences.Count)
			{
				Palaso.Reporting.Logger.WriteMinorEvent("Empty example removed");
				OnEmptyObjectsRemoved();
			}
		}
	}

	/// <summary>
	/// See comment in MultiText.cs for an explanation of this class.
	/// </summary>
	public class SenseGlossMultiText : MultiText
	{
		public SenseGlossMultiText(LexSense parent)
			: base(parent)
		{
		}

		public new LexSense Parent
		{
			get { return base.Parent as LexSense; }
		}
	}
}
