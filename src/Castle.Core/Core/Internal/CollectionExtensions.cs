// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Core.Internal
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;

#if SILVERLIGHT
	using System.Linq;
#endif

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class CollectionExtensions
	{
		public static bool Any<T>(this T[] items, Predicate<T> predicate)
		{
#if SILVERLIGHT
			if (items == null)
				throw new ArgumentNullException("items");

			if (predicate == null)
				throw new ArgumentNullException("predicate");
			return items.Any(predicate.Invoke);
#else
			return Array.Exists(items, predicate);
#endif
		}

		public static T[] ConcatAll<T>(params T[][] arrays)
		{
			var temp = new List<T>();
			foreach (var array in arrays)
			{
				if (array == null)
				{
					continue;
				}
				temp.AddRange(array);
			}
			return temp.ToArray();
		}

		public static TResult[] ConvertAll<T, TResult>(this T[] items, Converter<T, TResult> transformation)
		{
#if SILVERLIGHT
			return items.Select(transformation.Invoke).ToArray();
#else
			return Array.ConvertAll(items, transformation);
#endif
		}

		public static T Find<T>(this T[] items, Predicate<T> predicate)
		{
#if SILVERLIGHT
			if (items == null)
				throw new ArgumentNullException("items");

			if (predicate == null)
				throw new ArgumentNullException("predicate");
			return items.FirstOrDefault(predicate.Invoke);
#else
			return Array.Find(items, predicate);
#endif
		}

		public static T[] FindAll<T>(this T[] items, Predicate<T> predicate)
		{
#if SILVERLIGHT
			return items.Where(predicate.Invoke).ToArray();
#else
			return Array.FindAll(items, predicate);
#endif
		}

		public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
		{
			if (items == null)
			{
				return;
			}

			foreach (var item in items)
			{
				action(item);
			}
		}

		/// <summary>
		///   Checks whether or not collection is null or empty. Assumes colleciton can be safely enumerated multiple times.
		/// </summary>
		/// <param name = "this"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty(this IEnumerable @this)
		{
			return @this == null || @this.GetEnumerator().MoveNext() == false;
		}

		public static bool None<T>(this IEnumerable<T> items, Predicate<T> predicate)
		{
			return items.Any(predicate.Invoke) == false;
		}

		public static bool None<T>(this T[] items, Predicate<T> predicate)
		{
#if SILVERLIGHT
			if (items == null)
				throw new ArgumentNullException("items");

			if (predicate == null)
				throw new ArgumentNullException("predicate");
			return items.Any(predicate.Invoke) == false;
#else
			return Array.Exists(items, predicate) == false;
#endif
		}

		/// <summary>
		/// Splits collection based on <paramref name="collection"/>. Returns a <see cref="Tuple{T1, T2}"/> where Item1 contains elements matched by the predicate and Item2 contains remaining elements.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection"></param>
		/// <param name="condition"></param>
		/// <returns></returns>
		public static Tuple<T[], T[]> Split<T>(this IEnumerable<T> collection, Predicate<T> condition)
		{
			var matching = new List<T>();
			var nonMatching = new List<T>();

			foreach (var item in collection)
			{
				if (condition(item))
				{
					matching.Add(item);
				}
				else
				{
					nonMatching.Add(item);
				}
			}
			return Tuple.Create(matching.ToArray(), nonMatching.ToArray());
		}
	}
}
