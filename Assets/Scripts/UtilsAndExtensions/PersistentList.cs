using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

#if false
namespace Nrealus.Extensions.PersistentList
{
    public class PersistentList<T> : ICollection<T>
    {

        private List<IList> kinLists = new List<IList>();

        //public static implicit operator List<T>(PersistentList<T> persistentList) => persistentList.list;

        public List<Y> ConvertPartial<Y>(Predicate<T> match) where Y : T
        {
            var res = new List<Y>();
            foreach (var v in list)
            {
                if (match(v))
                    res.Add((Y)v);
            }
            kinLists.Add(res);
            return res;
        }

        /*public List<Y> ConvertPartial<Y>(Predicate<T> match, Func<T,Y> converter)
        {
            var res = new List<Y>();
            foreach (var v in list)
            {
                if (match(v))
                    res.Add(converter(v));
            }
            kinLists.Add(res);
            return res;
        }*/

        private List<T> list = new List<T>();

        public T this[int i]
        {
            get { return list[i]; }
            set { list[i] = value; }
        }

        public int Count => list.Count;

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            list.Add(item);
            foreach(var l in kinLists)
                l.Add(item);
        }

        public void Clear()
        {
            list.Clear();
            foreach(var l in kinLists)
                l.Clear();
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public bool Remove(T item)
        {
            foreach(var l in kinLists)
                l.Remove(item);
            return list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            Remove(list[index]);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        /*public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            list.Insert(index, item);
        }*/

        /*
        public void Add(T item) => list.Add(item);
        public void AddRange(IEnumerable<T> collection) => list.AddRange(collection);
        public ReadOnlyCollection<T> AsReadOnly() => list.AsReadOnly();
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer) => list.BinarySearch(index, count, item, comparer);
        public int BinarySearch(T item) => list.BinarySearch(item);
        public int BinarySearch(T item, IComparer<T> comparer) => list.BinarySearch(item, comparer);
        public void Clear() => list.Clear();
        public bool Contains(T item) => list.Contains(item);
        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter) => list.ConvertAll(converter);
        public void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);
        public void CopyTo(T[] array) => list.CopyTo(array);
        public void CopyTo(int index, T[] array, int arrayIndex, int count) => list.CopyTo(index, array, arrayIndex, count);
        public bool Exists(Predicate<T> match) => list.Exists(match);
        public T Find(Predicate<T> match) => list.Find(match);
        public List<T> FindAll(Predicate<T> match) => list.FindAll(match);
        public int FindIndex(int startIndex, int count, Predicate<T> match) => list.FindIndex(startIndex, count, match);
        public int FindIndex(int startIndex, Predicate<T> match) => list.FindIndex(startIndex, match);
        public int FindIndex(Predicate<T> match) => list.FindIndex(match);
        public T FindLast(Predicate<T> match) => list.FindLast(match);
        public int FindLastIndex(int startIndex, int count, Predicate<T> match) => list.FindLastIndex(startIndex, count, match);
        public int FindLastIndex(int startIndex, Predicate<T> match) => list.FindLastIndex(startIndex, match);
        public int FindLastIndex(Predicate<T> match) => list.FindLastIndex(match);
        public void ForEach(Action<T> action) => list.ForEach(action);
        public Enumerator GetEnumerator() => list.GetEnumerator();
        public List<T> GetRange(int index, int count) => list.GetRange(index, count);
        public int IndexOf(T item, int index, int count) => list.IndexOf(item, index, count);
        public int IndexOf(T item, int index) => list.IndexOf(item, index);
        public int IndexOf(T item) => list.IndexOf(item);
        public void Insert(int index, T item) => list.Insert(index, item);
        public void InsertRange(int index, IEnumerable<T> collection) => list.InsertRange(index, collection);
        public int LastIndexOf(T item) => list.LastIndexOf(item);
        public int LastIndexOf(T item, int index) => list.LastIndexOf(item, index);
        public int LastIndexOf(T item, int index, int count) => list.LastIndexOf(item, index, count);
        public bool Remove(T item) => list.Remove(item);
        public int RemoveAll(Predicate<T> match) => list.RemoveAll(match);
        public void RemoveAt(int index) => list.RemoveAt(index);
        public void RemoveRange(int index, int count) => list.RemoveRange(index, count);
        public void Reverse(int index, int count) => list.Reverse(index, count);
        public void Reverse() => list.Reverse();
        public void Sort(Comparison<T> comparison) => list.Sort(comparison);
        public void Sort(int index, int count, IComparer<T> comparer) => list.Sort(index, count, comparer);
        public void Sort() => list.Sort();
        public void Sort(IComparer<T> comparer) => list.Sort(comparer);
        public T[] ToArray() => list.ToArray();
        public void TrimExcess() => list.TrimExcess();
        public bool TrueForAll(Predicate<T> match) => list.TrueForAll(match);
        */

    }
}
#endif
