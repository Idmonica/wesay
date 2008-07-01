using System;
using System.Collections.Generic;
using WeSay.Data;

namespace WeSay.Data
{
	public interface IRepository<T>:IDisposable where T: class, new()
	{
		DateTime LastModified{ get;}

		T CreateItem();
		int CountAllItems();
		RepositoryId GetId(T item);
		T GetItem(RepositoryId id);
		void DeleteItem(T item);
		void DeleteItem(RepositoryId id);
		RepositoryId[] GetAllItems();
		void SaveItem(T item);
		bool CanQuery();
		bool CanPersist();
		void SaveItems(IEnumerable<T> items);
		ResultSet<T> GetItemsMatching(Query query);
	}
}
