using System;
using System.IO;
using Autofac;
using ETH.CommandLine;
using ETH.OutputModels;
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
				var injector = new MockInjector();
				var program = injector.Create<Program>();
				program.Run(new Options()).Should().Be(1);
			}

			public class TestRunGiven
			{
				[Fact]
				public void ScenarioOption()
				{
					var injector = new MockInjector();
					var program = injector.Create<Program>();
					var runner = injector.GetMock<IRunner>();
					var options = new Options
					{
						Scenario = "whee"
					};

					program.Run(options)
					       .Should().Be(0);

					runner.Verify(r => r.Run("whee", null, null));
				}

				[Fact]
				public void ScenarioOptionWithArguments()
				{
					var injector = new MockInjector();
					var program = injector.Create<Program>();
					var runner = injector.GetMock<IRunner>();
					var options = new Options
					{
						Scenario = "moo",
						ScenarioArguments = new[] {"a1", "a2"}
					};

					program.Run(options)
					       .Should().Be(0);

					runner.Verify(r => r.Run("moo", new[] {"a1", "a2"}, null));
				}

				[Fact]
				public void ScenarioOptionThrowingException()
				{
					var injector = new MockInjector();
					var program = injector.Create<Program>();
					var runner = injector.GetMock<IRunner>();
					var output = injector.GetMock<IOutput>();
					var options = new Options
					{
						Scenario = "s"
					};

					var exception = new InvalidOperationException("moo");
					runner.Setup(r =>
					             r.Run(It.IsAny<string>(),
					                   It.IsAny<string[]>(), null))
					      .Throws(exception);

					program.Run(options)
					       .Should().Be(1);

					output.Verify(o => o.Display(It.Is<ResultModel>(r =>
						r.Result == Result.Fail && r.Message == exception.ToString())));
				}
			}
		}
	}
}