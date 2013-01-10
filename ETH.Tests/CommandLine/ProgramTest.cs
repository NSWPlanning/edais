using System;
using System.IO;
using Autofac;
using ETH.CommandLine;
using ETH.ScenarioRunner;
using FluentAssertions;
using Moq;
using Xunit;

namespace ETH.Tests.CommandLine
{
	public class ProgramTest
	{
		public class Run
		{
			[Fact]
			public void NoOptions()
			{
				var program = new Program(null, null);
				program.Run(new Options(), new StringWriter()).Should().Be(1);
			}

			public class TestRunGiven
			{
				[Fact]
				public void NoSubOptions()
				{
					var program = new Program(null, null);
					var options = new Options
					{
						TestRun = "testrun1"
					};
					var output = new StringWriter();

					program.Run(options, output)
					       .Should().Be(1);

					output.ToString()
					      .Should().Contain("additional argument");
				}

				[Fact]
				public void ScenarioOption()
				{
					var runner = new Mock<IRunner>();
					var program = new Program(runner.Object, new Mock<IEndpointProvider>().Object);
					var options = new Options
					{
						TestRun = "butts",
						Scenario = "whee"
					};

					program.Run(options, null)
					       .Should().Be(0);

					runner.Verify(r => r.Run("butts", "whee", null));
				}

				[Fact]
				public void ScenarioOptionWithArguments()
				{
					var runner = new Mock<IRunner>();
					var program = new Program(runner.Object, new Mock<IEndpointProvider>().Object);
					var options = new Options
					{
						TestRun = "heh",
						Scenario = "moo",
						ScenarioArguments = new[] {"a1", "a2"}
					};

					program.Run(options, null)
					       .Should().Be(0);

					runner.Verify(r => r.Run("heh", "moo", new[] {"a1", "a2"}));
				}

				[Fact]
				public void ScenarioOptionThrowingException()
				{
					var runner = new Mock<IRunner>();
					var program = new Program(runner.Object, new Mock<IEndpointProvider>().Object);
					var options = new Options
					{
						TestRun = "r",
						Scenario = "s"
					};
					var output = new StringWriter();

					runner.Setup(r =>
					             r.Run(It.IsAny<string>(),
					                   It.IsAny<string>(),
					                   It.IsAny<string[]>()))
					      .Throws(new InvalidOperationException("moo"));

					program.Run(options, output)
					       .Should().Be(1);

					output.ToString()
					      .Should().Contain("moo");
				}
			}
		}
	}
}