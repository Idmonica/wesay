using System;

namespace WeSay.Data
{
	public interface IRecordListManager: IDisposable
	{
		void Register<T>(IFilter<T> filter) where T : class, new();
		IRecordList<T> GetListOfType<T>()  where T : class, new();
		IRecordList<T> GetListOfTypeFilteredFurther<T>(IFilter<T> filter)  where T : class, new();

		/// <summary>
		/// Call this, for example, when switching records in the gui. You don't need to know
		/// whether a commit is pending or not.
		/// </summary>
		void GoodTimeToCommit();

		event EventHandler DataCommitted;
		event EventHandler<DeletedItemEventArgs> DataDeleted;

		/// <summary>
		/// Used when importing, where we want to go fast and don't care to have a good cache if we crash
		/// </summary>
		bool DelayWritingCachesUntilDispose
		{
			get;
			set;
		}
	}
}
