using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ETH.Scenarios;
using System.Reflection;

namespace ETH.ScenarioRunner
{
	public interface IScenarioTypeFinder
	{
		IEnumerable<Type> AllScenarioTypes { get; }
		Dictionary<string, IEnumerable<MethodInfo>> Methods { get; }
	}

	class ScenarioTypeFinder : IScenarioTypeFinder
	{
		readonly _Assembly assembly;

		public ScenarioTypeFinder(_Assembly assembly)
		{
			this.assembly = assembly;
		}

		public IEnumerable<Type> AllScenarioTypes
		{
			get
			{
				var scenarios =
					from type in assembly.GetTypes()
					where type.IsSubclassOf(typeof(Scenario)) && !type.IsAbstract
					select type;
				return scenarios;
			}
		}

		public Dictionary<string, IEnumerable<MethodInfo>> Methods
		{
			get
			{
				var result = new Dictionary<string, IEnumerable<MethodInfo>>();
				foreach (var type in AllScenarioTypes)
				{
					result.Add(type.Name, type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
				}
				return result;
			}
		}
	}
}