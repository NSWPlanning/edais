using System.Diagnostics;
using System.IO;
using FluentAssertions;
using FluentAssertions.Primitives;
using Newtonsoft.Json;

namespace ETH.Tests
{
	public static class TestJsonExtensions
	{
		public static AndConstraint<StringAssertions> BeJson<T>(this StringAssertions assertions, T expected)
		{
			var result = JsonConvert.DeserializeAnonymousType(assertions.Subject, expected);
			result.ShouldBeEquivalentTo(expected);
			return assertions.NotBeBlank();
		}

		public static void ShouldBeJson<T>(this string source, T expected)
		{
			source.Should().BeJson(expected);
		}

		public static void ShouldHaveJsonLine<T>(this StreamReader reader, T expected)
		{
			reader.ReadLine().ShouldBeJson(expected);
		}

		public static void ShouldHaveJsonLine<T>(this Process process, T expected)
		{
			process.StandardOutput.ShouldHaveJsonLine(expected);
		}

		public static object DeserializeJson(this string source)
		{
			return JsonConvert.DeserializeObject(source);
		}
	}
}