using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData.Annotations;


namespace DynamicData.Kernel
{
	/// <summary>
	/// Enumerable extensions
	/// </summary>
	public static class EnumerableEx
	{
        /// <summary>
        /// Finds the index of many items as specified in the secondary enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="itemsToFind">The items to find in the source enumerable</param>
        /// <returns>
        /// A result as specified by the result selector
        /// </returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static IEnumerable<ItemWithIndex<T>> IndexOfMany<T>(this IEnumerable<T> source, IEnumerable<T> itemsToFind)
        {
            return source.IndexOfMany(itemsToFind, (t, idx) => new ItemWithIndex<T>(t, idx));
        }


        /// <summary>
        /// Finds the index of many items as specified in the secondary enumerable.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="itemsToFind">The items to find.</param>
        /// <param name="resultSelector">The result selector</param>
        /// <returns>A result as specified by the result selector</returns>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public static IEnumerable<TResult> IndexOfMany<TObject, TResult>([NotNull] this IEnumerable<TObject> source,[NotNull] IEnumerable<TObject> itemsToFind, [NotNull] Func<TObject,int,TResult> resultSelector)
	    {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (itemsToFind == null) throw new ArgumentNullException(nameof(itemsToFind));
            if (resultSelector == null) throw new ArgumentNullException(nameof(resultSelector));

            var indexed = source.Select((element,index)=>new {Element= element, Index=index});
            return itemsToFind
                        .Join(indexed,left=>left,right=>right.Element,(left,right)=>right)
                        .Select(x => resultSelector(x.Element,x.Index));
        }



		/// <summary>
		/// Returns an object with it's current index.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source">The source.</param>
		/// <returns></returns>
		internal static IEnumerable<ItemWithIndex<T>> WithIndex<T>(this IEnumerable<T> source)
		{

			return source.Select((item, index) => new ItemWithIndex<T>(item, index));
		}

		internal static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (var item in source)
			{
				action(item);
			}
		}

		internal static void ForEach<TObject>(this IEnumerable<TObject> source, Action<TObject, int> action)
		{
			int i = 0;
			foreach (var item in source)
			{
				action(item, i);
				i++;
			}
		}

		internal static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
		{
			return source ?? Enumerable.Empty<T>();
		}

		internal static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
		{
			return new HashSet<T>(source);
		}

	}
}