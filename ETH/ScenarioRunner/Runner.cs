using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;

namespace ETH.ScenarioRunner
{
	public interface IRunner
	{
		void Run(string testRunId, string scenarioId, string[] scenarioArguments);
	}

	public class Runner : IRunner
	{
		readonly ILifetimeScope container;
		readonly IScenarioTypeFinder scenarioTypeFinder;

		public Runner(ILifetimeScope container, IScenarioTypeFinder scenarioTypeFinder)
		{
			this.container = container;
			this.scenarioTypeFinder = scenarioTypeFinder;
		}

		public void Run(string testRunId, string scenarioId, string[] scenarioArguments)
		{
			// TODO: use testRunId
			Type scenarioType;
			MethodInfo scenarioMethod;
			FindScenario(scenarioId, out scenarioType, out scenarioMethod);
		
			var scenario = container.Resolve(scenarioType);
			var arguments = new object[scenarioMethod.GetParameters().Length];
			if (arguments.Length > 0 && (scenarioArguments == null || arguments.Length != scenarioArguments.Length))
			{
				throw new InvalidOperationException(
					"Invalid arguments provided, use -a to specify arguments.");
			}
			if (arguments.Length > 0) arguments = scenarioArguments;
			scenarioMethod.Invoke(scenario, arguments);
		}

		void FindScenario(string scenarioId, out Type scenarioType, out MethodInfo scenarioMethod)
		{
			var scenarioIdSplit = scenarioId.Split('.');

			if (scenarioIdSplit.Length != 2)
			{
				throw new InvalidOperationException(
					"Scenario must be in the format: ScenarioName.Variation");
			}

			var scenarioTypeName = scenarioIdSplit[0];
			var scenarioMethodName = scenarioIdSplit[1];

			scenarioType = scenarioTypeFinder.AllScenarioTypes
			                             .SingleOrDefault(type => type.Name == scenarioTypeName);

			if (scenarioType == null)
			{
				throw new InvalidOperationException(
					string.Format("Unable to find scenario called {0}", scenarioTypeName));
			}

			scenarioMethod = scenarioType.GetMethods()
			                             .SingleOrDefault(m => m.Name == scenarioMethodName);

			if (scenarioMethod == null)
			{
				throw new InvalidOperationException(
					string.Format("Unable to find test called {0} for {1}.",
					              scenarioMethodName,
					              scenarioTypeName));
			}
		}
	}
}