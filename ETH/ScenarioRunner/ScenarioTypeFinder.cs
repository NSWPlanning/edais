using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ETH.Scenarios;

namespace ETH.ScenarioRunner
{
	public interface IScenarioTypeFinder
	{
		IEnumerable<Type> AllScenarioTypes { get; }
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
	}
}