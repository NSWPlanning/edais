using System.IO;
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

		public Options()
		{
			LogFileName = Path.Combine(Directory.GetCurrentDirectory(), "ETHLog.txt");
		}

		[Option("t", "test", MutuallyExclusiveSet = RunSet, HelpText = "The scenario to run.")]
		public string Scenario { get; set; }

		[OptionArray("a", "arg", HelpText = "Scenario arguments, separated by spaces. (e.g. -a APP-00000001 user@localhost.test)")]
		public string[] ScenarioArguments { get; set; }

		[Option("l", "list", MutuallyExclusiveSet = MainSet, HelpText = "List all scenarios.")]
		public bool ListScenarios { get; set; }

		[Option("p", "party", HelpText = "Receiver party")]
		public string ReceiverParty { get; set; }

		//[Option("i", "info", MutuallyExclusiveSet = MainSet, HelpText = "Retrieve info for a particular scenario.")]
		public string ScenarioInfo { get; set; }

		[Option("u", "listen", HelpText = "URL to listen on.", DefaultValue = DefaultUrl)]
		public string ListenUrl { get; set; }

		[OptionArray("c", "client-url", HelpText = "Endpoint(s) for client, separated by spaces.", DefaultValue = null)]
		public string[] ClientUrl { get; set; }

		[Option("m", null, DefaultValue = true, HelpText = "Use SOAP 1.2 instead of 1.1.")]
		public bool UseSoap11 { get; set; }

		[Option(null, "user", HelpText = "Username for requests/responses.", DefaultValue = DefaultUser)]
		public string Username { get; set; }

		[Option(null, "pass", HelpText = "Password for requests/responses.", DefaultValue = DefaultPass)]
		public string Password { get; set; }

		[Option("s", "skip", HelpText = "Skip authenticaction.", DefaultValue = false)]
		public bool SkipAuthentication { get; set; }

		[Option("f", "log", HelpText = "Log filename.")]
		public string LogFileName { get; set; }

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(
				this, 
				current => HelpText.DefaultParsingErrorsHandler(this, current));
		}
	}
}