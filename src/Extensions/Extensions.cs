using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Extensions
{
	public static class Extensions
	{
		public static bool In<T>(this T t, params T[] values)
		{
			return values.Contains(t);
		}

		public static T Tap<T>(this T t, Action<T> action)
		{
			action(t);
			return t;
		}

		public static T2 Map<T1, T2>(this T1 t, Func<T1, T2> mapper)
		{
			return mapper(t);
		}

		private static readonly JsonSerializerSettings DefaultJsonSettings = new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver()
		};

		public static string ToJson<T>(this T t)
		{
			return JsonConvert.SerializeObject(t, DefaultJsonSettings);
		}

		private static readonly ConcurrentDictionary<Type, XmlSerializer> XmlSerializers = new ConcurrentDictionary<Type, XmlSerializer>();

		public static string ToXml<T>(this T t, Action<XmlSerializerNamespaces> namespaces = null)
		{
			var ns = new XmlSerializerNamespaces();
			namespaces?.Invoke(ns);

			var serializer = XmlSerializers.GetOrAdd(typeof(T), x => new XmlSerializer(x));

			using var writer = new Utf8StringWriter();
			serializer.Serialize(writer, t, ns);
			return writer.ToString();
		}

		public static string ReplaceAll(this string seed, char[] chars, string replacement)
		{
			return chars.Aggregate(seed, (str, c) => str.Replace(c.ToString(), replacement));
		}

		public static IDictionary<TKey, TValue> AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
			TKey key, TValue value)
		{
			if (value is Array values && values.Length == 0)
			{
				dictionary.Remove(key);
				return dictionary;
			}

			if (dictionary.ContainsKey(key))
			{
				dictionary[key] = value;
			}
			else
			{
				dictionary.Add(key, value);
			}

			return dictionary;
		}

		public static T To<T>(this string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return default;
			}

			return JsonConvert.DeserializeObject<T>(value);
		}

		public static T To<T>(this Stream stream)
		{
			if (stream == null)
			{
				return default;
			}

			if (!stream.CanRead)
			{
				return default;
			}

			using var sr = new StreamReader(stream);
			using var rdr = new JsonTextReader(sr);

			return new JsonSerializer().Deserialize<T>(rdr);
		}

		public static string Truncate(this string value, int length)
		{
			if (string.IsNullOrEmpty(value))
			{
				return value;
			}

			return value.Substring(0, Math.Min(length, value.Length));
		}

		public static T Coalesce<T>(this T @this, params T[] values) where T : class
		{
			return @this ?? values.FirstOrDefault(value => value != null);
		}

		public static string ToBase64(this string value)
		{
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
		}

		public static IEnumerable<T> Flatten<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>> getChildren)
		{
			var stack = new Stack<T>();
			foreach (var item in items)
			{
				stack.Push(item);
			}

			while (stack.Count > 0)
			{
				var current = stack.Pop();
				yield return current;

				var children = getChildren(current);
				if (children == null)
				{
					continue;
				}

				foreach (var child in children)
				{
					stack.Push(child);
				}
			}
		}

		public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> source, int chunkSize)
		{
			return source
				.Select((value, index) => new { index, value })
				.GroupBy(x => x.index / chunkSize)
				.Select(x => x.Select(v => v.value).ToList())
				.ToList();
		}

		public static IEnumerable<T> AddAndReturn<T>(this IEnumerable<T> collection, T newValue)
		{
			return new List<T>(collection) { newValue };
		}

		public class Utf8StringWriter : StringWriter
		{
			public override Encoding Encoding => Encoding.UTF8;
		}
	}
}
