using CommandLine;
using CommandLine.Text;
using ETH.ScenarioRunner;
using System;
using System.Reflection;

namespace ETH.CommandLine
{	
	class Options : CommandLineOptionsBase
	{
		const string DefaultUrl = "https://*:8181/";
		const string RunSet = "run";
		const string MainSet = "main";
		const string DefaultUser = "testuser";
		const string DefaultPass = "testpass";

		[Option("r", "run", MutuallyExclusiveSet = MainSet, HelpText = "Test run identifier.")]
		public string TestRun { get; set; }

		[Option("t", "test", MutuallyExclusiveSet = RunSet, HelpText = "The scenario to run.")]
		public string Scenario { get; set; }

		[OptionArray("a", "arg", HelpText = "Scenario arguments.")]
		public string[] ScenarioArguments { get; set; }

		[Option("e", "results", MutuallyExclusiveSet = RunSet, HelpText = "Output the results of a test run.")]
		public bool Results { get; set; }

		[Option("c", "clear", MutuallyExclusiveSet = RunSet, HelpText = "Clear results for a specific test run.")]
		public bool ClearResults { get; set; }

		[Option("l", "list", MutuallyExclusiveSet = MainSet, HelpText = "List all scenarios.")]
		public bool ListScenarios { get; set; }

		[Option("i", "info", MutuallyExclusiveSet = MainSet, HelpText = "Retrieve info for a particular scenario.")]
		public string ScenarioInfo { get; set; }

		[Option("u", "listen", HelpText = "URL to listen on.", DefaultValue = DefaultUrl)]
		public string ListenUrl { get; set; }

		[Option("c", "client-url", HelpText = "Endpoint for client", DefaultValue = null)]
		public string ClientUrl { get; set; }

		[Option("m", null, HelpText = "Use SOAP 1.1 instead of 1.2.")]
		public bool UseSoap11 { get; set; }

		[Option(null, "user", HelpText = "Username for requests/responses.", DefaultValue = DefaultUser)]
		public string Username { get; set; }

		[Option(null, "pass", HelpText = "Password for requests/responses.", DefaultValue = DefaultPass)]
		public string Password { get; set; }

		[Option("s", "skip", HelpText = "Skip authenticaction.", DefaultValue = false)]
		public bool SkipAuthentication { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(
				this, 
				current => HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}
}