using System;

namespace WeSay.Data.Tests
{
	public class SimpleIntFilter : IFilter<SimpleIntTestClass>
	{
		int _lowerBound;
		int _upperBound;
		private bool _useInverseFilter;

		public SimpleIntFilter(int lowerBound, int upperBound)
		{
			_lowerBound = lowerBound;
			_upperBound = upperBound;
		}
		#region IFilter<SimpleIntTestClass> Members

		public Predicate<SimpleIntTestClass> FilteringPredicate
		{
			get
			{
				if(!UseInverseFilter)
				{
					return filter1;
				}
				return filter2;
			}
		}

		public string Key
		{
			get
			{
				return ToString() + _lowerBound.ToString() + "-" + _upperBound.ToString();
			}
		}

		public bool UseInverseFilter
		{
			get { return this._useInverseFilter; }
			set { this._useInverseFilter = value; }
		}

		private bool filter1(SimpleIntTestClass simpleIntTest)
		{
			return (_lowerBound <= simpleIntTest.I && simpleIntTest.I <= _upperBound);
		}

		private bool filter2(SimpleIntTestClass simpleIntTest)
		{
			return (_lowerBound > simpleIntTest.I || simpleIntTest.I > _upperBound);
		}

		#endregion
	}
}
