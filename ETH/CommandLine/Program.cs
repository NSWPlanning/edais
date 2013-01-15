using System;
using System.Net;
using System.ServiceModel.Channels;
using Autofac;
using CommandLine;
using ETH.Http;
using ETH.OutputModels;
using ETH.ScenarioRunner;
using ETH.Soap;
using ETH.Util;
using ImpromptuInterface;
using System.Reflection;
using CommandLine.Text;

namespace ETH.CommandLine
{
	class Program
	{
		readonly IRunner scenarioRunner;
		readonly IEndpointProvider endpointProvider;
		readonly IScenarioTypeFinder scenarioTypeFinder;
		readonly IOutput output;

		public static IContainer Container { get; private set; } // TODO: refactor out

		static int Main(string[] args)
		{
			var parser = GetParser();
			var options = new Options();

			if (!parser.ParseArguments(args, options)) return 1;

			Container = CreateContainer();
			var program = Container.Resolve<Program>();
			return program.Run(options);
		}

		public Program(IRunner scenarioRunner, IEndpointProvider endpointProvider, IOutput output, IScenarioTypeFinder scenarioTypeFinder)
		{
			this.scenarioRunner = scenarioRunner;
			this.endpointProvider = endpointProvider;
			this.output = output;
			this.scenarioTypeFinder = scenarioTypeFinder;
		}

		public int Run(Options options)
		{
			// TODO: extract output to its own class, so we can handle Json there.
			// TODO: handle endpoint option
			if (options.TestRun != null)
			{
				if (options.Scenario != null)
				{
					endpointProvider.ServerBaseUrl = options.ListenUrl;
					endpointProvider.ClientEndpoint = options.ClientUrl;

					if (options.UseSoap11)
					{
						endpointProvider.MessageVersion = MessageVersion.Soap11WSAddressingAugust2004;
					}

					try
					{
						scenarioRunner.Run(options.TestRun, options.Scenario, options.ScenarioArguments);
						output.Display(new ResultModel
						{
							Result = Result.Pass
						});
					}
					catch (Exception e)					
					{
						output.Display(new ResultModel
						{
							Result = Result.Fail,
							Message = e.ToString()
						});
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
					output.String("You must provide an additional argument with the run.");				
					return 1;
				}
			}
			else if (options.ListScenarios)
			{
				var helpText = new HelpText();
				foreach (var scenario in scenarioTypeFinder.Methods.Keys)
				{
					helpText.AddPreOptionsLine(scenario);
					foreach(var method in scenarioTypeFinder.Methods[scenario])
					{
						helpText.AddPreOptionsLine(string.Format("|- {0}", method.Name));
					}
					helpText.AddPreOptionsLine(Environment.NewLine);
				}
				output.String(helpText);
				return 1;
			}
			else if (options.ScenarioInfo != null)
			{
				throw new NotImplementedException();
			}
			else
			{
				output.String(options.GetUsage());
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

			builder.RegisterInstance(new Output(Console.OpenStandardOutput()))
			       .SingleInstance()
			       .As<IOutput>();

			builder.RegisterType<Program>()
			       .SingleInstance();
			builder.RegisterType<EndpointProvider>()
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
	}
}
