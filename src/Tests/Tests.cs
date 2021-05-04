using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Extensions;
using Newtonsoft.Json.Linq;
using Shouldly;
using Xunit;

namespace Tests
{
	public class Tests
	{
		[Fact]
		public void In()
		{
			2.In(1, 2, 3).ShouldBeTrue();
		}

		[Fact]
		public void Tap()
		{
			1.Tap(x => x.ShouldBe(1));
		}

		[Fact]
		public void Map()
		{
			1.Map(x => 2).ShouldBe(2);
		}

		[Fact]
		public void ToJson()
		{
			new {Hello = "World"}.ToJson().ShouldBe("{\"hello\":\"World\"}");
		}

		[Fact]
		public void ToXml()
		{
			"hello".ToXml().ShouldContain("<string>hello</string>");
		}

		[Fact]
		public void ReplaceAll()
		{
			"hello\r\n".ReplaceAll(new []{'\r', '\n'}, string.Empty).ShouldBe("hello");
		}

		[Fact]
		public void AddOrUpdate()
		{
			new Dictionary<int, string>
				{
					[1] = "hello world"
				}.AddOrUpdate(1, "Hello World")
				[1].ShouldBe("Hello World");
		}

		[Fact]
		public void To()
		{
			"{\"hello\":\"World\"}".To<JObject>()["hello"].ShouldBe("World");
		}

		[Fact]
		public void To2()
		{
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write("{\"hello\":\"World\"}");
			writer.Flush();
			stream.Position = 0;

			stream.To<JObject>()["hello"].ShouldBe("World");
		}

		[Fact]
		public void Truncate()
		{
			"Hello World".Truncate(5).ShouldBe("Hello");
		}

		[Fact]
		public void Coalesce()
		{
			((string) null).Coalesce("Hello World").ShouldBe("Hello World");
		}

		[Fact]
		public void ToBase44()
		{
			"Hello World".ToBase64().ShouldBe("SGVsbG8gV29ybGQ=");
		}

		[Fact]
		public void Flatten()
		{
			new List<Type>
				{
					new Type {Id = "1", Children = new List<Type> {new Type {Id = "10"}, new Type {Id = "11"}}},
					new Type {Id = "2", Children = new List<Type> {new Type {Id = "12"}, new Type {Id = "13"}}},
				}
				.Flatten(x => x.Children)
				.Count()
				.ShouldBe(6);
		}

		[Fact]
		public void ChunkBy()
		{
			new List<int>
				{
					1, 2, 3, 4, 5, 6, 7, 8, 9, 10
				}
				.ChunkBy(5)
				.Count()
				.ShouldBe(2);
		}

		[Fact]
		public void AddAndReturn()
		{
			new List<int>()
				.AddAndReturn(1)
				.Count()
				.ShouldBe(1);
		}

		[Fact]
		public void Zip()
		{
			"Hello World"
				.Zip()
				.Unzip()
				.Map(x => Encoding.UTF8.GetString(x))
				.ShouldBe("Hello World");
		}

		public class Type
		{
			public string Id { get; set; }
			public List<Type> Children { get; set; }
		}
	}
}
