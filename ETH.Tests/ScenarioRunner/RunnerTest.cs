using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using ETH.ScenarioRunner;
using ETH.Scenarios;
using Moq;
using Xunit;
using FluentAssertions;

namespace ETH.Tests.ScenarioRunner
{
	public class RunnerTest
	{
		public class Run
		{
			public class DummyScenario : Scenario
			{
				public virtual void SomeScenario()
				{
				}
			}

			[Fact]
			public void FindsAndRunsScenario()
			{
				var scope = new Mock<ILifetimeScope>();
				var finder = new Mock<IScenarioTypeFinder>();
				var runner = new Runner(scope.Object, finder.Object);
				var scenario = new Mock<DummyScenario>();

				finder.Setup(f => f.AllScenarioTypes).Returns(new[] { typeof(DummyScenario) });
				scope.SetupResolve<ILifetimeScope, Scenario>(scenario.Object);

				runner.Run("DummyScenario.SomeScenario", new string[0]);

				scenario.Verify(s => s.SomeScenario(), Times.Once());
			}

			[Fact]
			public void FailsWhenInvalidScenarioFormat()
			{
				var scope = new Mock<ILifetimeScope>();
				var finder = new Mock<IScenarioTypeFinder>();
				var runner = new Runner(scope.Object, finder.Object);

				runner.Invoking(r => r.Run("DummyScenarioSomeScenario", new string[0]))
				      .ShouldThrow<InvalidOperationException>()
					  .WithMessage("format", ComparisonMode.Substring);
			}

			[Fact]
			public void FailsWhenCantFindType()
			{
				var scope = new Mock<ILifetimeScope>();
				var finder = new Mock<IScenarioTypeFinder>();
				var runner = new Runner(scope.Object, finder.Object);

				runner.Invoking(r => r.Run("DummyScenario.SomeScenario", new string[0]))
					  .ShouldThrow<InvalidOperationException>()
					  .WithMessage("find scenario", ComparisonMode.Substring);
			}

			[Fact]
			public void FailsWhenCantFindMethod()
			{
				var scope = new Mock<ILifetimeScope>();
				var finder = new Mock<IScenarioTypeFinder>();
				var runner = new Runner(scope.Object, finder.Object);

				finder.Setup(f => f.AllScenarioTypes).Returns(new[] { typeof(DummyScenario) });

				runner.Invoking(r => r.Run("DummyScenario.SomeOtherScenario", new string[0]))
					  .ShouldThrow<InvalidOperationException>()
					  .WithMessage("find test", ComparisonMode.Substring);
			}
		}
	}

	public static class MockExtensions
	{
		public static void SetupResolve<TContext, TService>(this Mock<TContext> context, object result)
			where TContext : class, IComponentContext
		{
			var componentRegistration = It.IsAny<IComponentRegistration>();
			context.Setup(c =>
			              c.ComponentRegistry.TryGetRegistration(
				              It.Is<Service>(s => ((TypedService)s).ServiceType.IsSubclassOf(typeof(TService))), 
				              //It.IsAny<Service>(), 
				              out componentRegistration))
			       .Returns(true);
			context.Setup(s => s.ResolveComponent(componentRegistration, It.IsAny<IEnumerable<Parameter>>()))
			       .Returns(result);
		}
	}
}