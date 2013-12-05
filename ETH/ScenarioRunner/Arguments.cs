using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ETH.ScenarioRunner
{
	static class Arguments
	{
		public static IEnumerable<object> GetArguments(this ParameterInfo[] parameters, IEnumerable<string> providedArguments)
		{
			int i = 0;
			foreach(var parameter in parameters)
			{
				object result;
				if (providedArguments != null && providedArguments.Count() > i)
				{
					result = providedArguments.ElementAt(i);
					i++;
				}
				else
				{
					result  = parameter.DefaultValue;
				}
				yield return result;
			}
		}

	}
}
