using CommandLine;
using CommandLine.Text;

namespace ETH.CommandLine
{	
	class Options : CommandLineOptionsBase
	{
		const string defaultUrl = "http://localhost:8181/";
		const string runSet = "run";
		const string mainSet = "main";

		[Option("r", "run", MutuallyExclusiveSet = mainSet, HelpText = "Test run identifier.")]
		public string TestRun { get; set; }

		[Option("t", "test", MutuallyExclusiveSet = runSet, HelpText = "The scenario to run.")]
		public string Scenario { get; set; }

		[OptionArray("a", "arg", HelpText = "Scenario arguments.")]
		public string[] ScenarioArguments { get; set; }

		[Option("e", "results", MutuallyExclusiveSet = runSet, HelpText = "Output the results of a test run.")]
		public bool Results { get; set; }

		[Option("c", "clear", MutuallyExclusiveSet = runSet, HelpText = "Clear results for a specific test run.")]
		public bool ClearResults { get; set; }

		[Option("l", "list", MutuallyExclusiveSet = mainSet, HelpText = "List all scenarios.")]
		public bool ListScenarios { get; set; }

		[Option("i", "info", MutuallyExclusiveSet = mainSet, HelpText = "Retrieve info for a particular scenario.")]
		public string ScenarioInfo { get; set; }

		[Option("u", "listen", HelpText = "URL to listen on.", DefaultValue = defaultUrl)]
		public string ListenUrl { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(
				this, 
				current => HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}
}