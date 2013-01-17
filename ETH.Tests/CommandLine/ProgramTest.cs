﻿using System;
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
				public void NoSubOptions()
				{
					var injector = new MockInjector();
					var program = injector.Create<Program>();
					var output = injector.GetMock<IOutput>();
					var options = new Options
					{
						TestRun = "testrun1"
					};

					program.Run(options)
					       .Should().Be(1);

					output.Verify(o => 
						o.String(It.Is<string>(s => 
							s.Contains("additional argument"))));
				}

				[Fact]
				public void ScenarioOption()
				{
					var injector = new MockInjector();
					var program = injector.Create<Program>();
					var runner = injector.GetMock<IRunner>();
					var options = new Options
					{
						TestRun = "butts",
						Scenario = "whee"
					};

					program.Run(options)
					       .Should().Be(0);

					runner.Verify(r => r.Run("butts", "whee", null));
				}

				[Fact]
				public void ScenarioOptionWithArguments()
				{
					var injector = new MockInjector();
					var program = injector.Create<Program>();
					var runner = injector.GetMock<IRunner>();
					var options = new Options
					{
						TestRun = "heh",
						Scenario = "moo",
						ScenarioArguments = new[] {"a1", "a2"}
					};

					program.Run(options)
					       .Should().Be(0);

					runner.Verify(r => r.Run("heh", "moo", new[] {"a1", "a2"}));
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
						TestRun = "r",
						Scenario = "s"
					};

					var exception = new InvalidOperationException("moo");
					runner.Setup(r =>
					             r.Run(It.IsAny<string>(),
					                   It.IsAny<string>(),
					                   It.IsAny<string[]>()))
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