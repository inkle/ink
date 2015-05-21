using NUnit.Framework;
using System;
using Inklewriter;
using Inklewriter.Runtime;

namespace Tests
{
	[TestFixture ()]
	public class StringParserTests
	{
		// Helper compile function
		protected Story CompileString(string str)
		{
			InkParser parser = new InkParser(str);
			var parsedStory = parser.Parse();
			Story story = parsedStory.ExportRuntime ();
			return story;
		}

		[Test ()]
		public void CompileHelloWorld()
		{
			Story story = CompileString ("Hello world");
			story.Begin ();
			Assert.AreEqual (story.currentText, "Hello world");
		}

		//------------------------------------------------------------------------

		[Test ()]
		public void TestStringParserABAB ()
		{
			StringParser p = new StringParser ("ABAB");
			var results = p.Interleave(
				() => p.ParseString ("A"),
				() => p.ParseString ("B"));

			var expected = new [] { "A", "B", "A", "B" };
			Assert.AreEqual(expected, results);
		}

		[Test ()]
		public void TestStringParserA ()
		{
			StringParser p = new StringParser ("A");
			var results = p.Interleave (
				() => p.ParseString ("A"),
				() => p.ParseString ("B"));

			var expected = new [] { "A" };
			Assert.AreEqual(expected, results);
		}

		[Test ()]
		public void TestStringParserB ()
		{
			StringParser p = new StringParser ("B");
			var result = p.Interleave (
				() => p.ParseString ("A"),
				() => p.ParseString ("B"));

			Assert.IsNull (result);
		}

		[Test ()]
		public void TestStringParserABAOptional ()
		{
			StringParser p = new StringParser ("ABAA");
			var results = p.Interleave (
				() => p.ParseString ("A"),
				p.Optional(() => p.ParseString ("B")));

			var expected = new [] { "A", "B", "A", "A" };
			Assert.AreEqual(expected, results);
		}

		[Test ()]
		public void TestStringParserABAOptional2 ()
		{
			StringParser p = new StringParser ("BABB");
			var results = p.Interleave (
				p.Optional(() => p.ParseString ("A")),
				() => p.ParseString ("B"));

			var expected = new [] { "B", "A", "B", "B" };
			Assert.AreEqual(expected, results);
		}


	}
}

