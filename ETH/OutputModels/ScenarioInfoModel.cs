using System.Collections.Generic;

namespace ETH.OutputModels
{
	class ScenarioInfoModel
	{
		public string Scenario { get; set; }
		public string Description { get; set; }
		public IEnumerable<ScenarioParameter> Enumerable { get; set; }
	}
}