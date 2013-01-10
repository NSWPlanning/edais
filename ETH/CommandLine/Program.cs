using System;
using System.IO;
using System.Net;
using Autofac;
using CommandLine;
using ETH.Http;
using ETH.ScenarioRunner;
using ETH.Soap;
using ETH.Util;
using ImpromptuInterface;

namespace ETH.CommandLine
{
	public interface IEndpointProvider
	{
		string ServerBaseUrl { get; }
		string ClientEndpoint { get; }
	}

	class Program : IEndpointProvider
	{
		readonly IRunner scenarioRunner;

		static int Main(string[] args)
		{
			var parser = GetParser();
			var options = new Options();

			if (!parser.ParseArguments(args, options)) return 1;

			var container = CreateContainer();
			var program = container.Resolve<Program>();
			return program.Run(options, Console.Out);
		}

		public Program(IRunner scenarioRunner)
		{
			this.scenarioRunner = scenarioRunner;
		}

		public int Run(Options options, TextWriter output)
		{
			// TODO: extract output to its own class, so we can handle Json there.
			// TODO: handle endpoint option
			if (options.TestRun != null)
			{
				if (options.Scenario != null)
				{
					ServerBaseUrl = options.ListenUrl;

					try
					{
						scenarioRunner.Run(options.TestRun, options.Scenario, options.ScenarioArguments);
					}
					catch (Exception e)
					{
						output.WriteLine(e.ToString());
						return 1;
					}

					
				}
				else if (options.Results)
				{
					throw new NotImplementedException();
				}
				else if (options.ClearResults)
				{
					throw new NotImplementedException();
				}
				else
				{
					output.WriteLine("You must provide an additional argument with the run.");				
					return 1;
				}
			}
			else if (options.ListScenarios)
			{
				throw new NotImplementedException();
			}
			else if (options.ScenarioInfo != null)
			{
				throw new NotImplementedException();
			}
			else
			{
				output.Write(options.GetUsage());
				return 1;
			}

			return 0;
		}

		static IContainer CreateContainer()
		{
			var builder = new ContainerBuilder();

			builder.RegisterType<Server>().As<IServer>();
			builder.RegisterType<Client>().As<IClient>();
			builder.RegisterType<Runner>().As<IRunner>();
			builder.RegisterType<TestDataLoader>().As<ITestDataLoader>();
			builder.RegisterSource(new ScenarioRegistrationSource());
			builder.Register(context => new HttpListener().ActLike<IHttpListener>())
			       .As<IHttpListener>();
			builder.Register(context => new ScenarioTypeFinder(typeof(Program).Assembly))
			       .As<IScenarioTypeFinder>();
			builder.RegisterType<Program>()
			       .SingleInstance()
			       .As<IEndpointProvider>();

			return builder.Build();
		}

		static CommandLineParser GetParser()
		{
			var parserSettings = new CommandLineParserSettings
			{
				MutuallyExclusive = true
			};
			return new CommandLineParser(parserSettings);
		}

		public string ServerBaseUrl { get; private set; }
		public string ClientEndpoint { get; private set; }
	}
}
