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
using System.Collections.Generic;
using NLog;
using NLog.Config;
using NLog.Targets;
using Utility.Logging;
using Utility.Logging.NLog.Autofac;
using ServiceStack.Text;

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

			SetupLogging(args, options);
			Container = CreateContainer();
			var program = Container.Resolve<Program>();
			return program.Run(options);
		}

		static void SetupLogging(string[] args, Options options)
		{
			var configuration = new LoggingConfiguration();
			var fileTarget = new FileTarget
			{
				AutoFlush = true,
				FileName = options.LogFileName,
				Layout = @"${date:format=yyyy-MM-dd HH\:mm\:ss.fff} ${logger} ${message}"
			};
			configuration.AddTarget("file", fileTarget);			
			configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, fileTarget));

			LogManager.Configuration = configuration;

			LogManager.GetCurrentClassLogger().Info("Arguments: {0}", args.Dump());
			LogManager.GetCurrentClassLogger().Info("Options: {0}", options.Dump());
		}

		public Program(
			IRunner scenarioRunner, 
			IEndpointProvider endpointProvider, 
			IOutput output, 
			IScenarioTypeFinder scenarioTypeFinder)
		{
			this.scenarioRunner = scenarioRunner;
			this.endpointProvider = endpointProvider;
			this.output = output;
			this.scenarioTypeFinder = scenarioTypeFinder;
		}

		public int Run(Options options)
		{
			if (options.TestRun != null)
			{
				if (options.Scenario != null)
				{
					endpointProvider.ServerBaseUrl = options.ListenUrl;
					if (options.ClientUrl != null && options.ClientUrl.Length > 0) endpointProvider.ClientEndpoint = new Queue<string>(options.ClientUrl);
					endpointProvider.Username = options.Username;
					endpointProvider.Password = options.Password;
					endpointProvider.SkipAuthentication = options.SkipAuthentication;

					if (options.UseSoap11)
					{
						endpointProvider.MessageVersion = MessageVersion.Soap11;
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
					foreach (var method in scenarioTypeFinder.Methods[scenario])
					{
						var parameters = new List<string>();
						for (int i = 0; i < method.GetParameters().Length; i++)
						{
							parameters.Add(method.GetParameters()[i].Name);
						}
						helpText.AddPreOptionsLine(string.Format("|- {0}({1})", method.Name, string.Join(",", parameters)));
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

			builder.RegisterType<Server>().As<IServer>().SingleInstance();
			builder.RegisterType<Client>().As<IClient>().SingleInstance();
			builder.RegisterType<Runner>().As<IRunner>().SingleInstance();
			builder.RegisterType<SoapDecoder>().As<ISoapDecoder>();
			builder.RegisterType<TestDataLoader>().As<ITestDataLoader>();
			builder.RegisterType<WebRequestFactory>().As<IWebRequestFactory>();
			builder.RegisterType<DateTimeProvider>().As<IDateTimeProvider>();
			builder.RegisterType<RandomNumberGeneratorProvider>().As<IRandomNumberGeneratorProvider>();
			builder.RegisterType<SecurityHeaderFactory>().As<ISecurityHeaderFactory>();

			builder.RegisterSource(new ScenarioRegistrationSource());

			builder.Register(context => new HttpListener().ActLike<IHttpListener>())
				   .As<IHttpListener>();
			builder.Register(context => new ScenarioTypeFinder(typeof(Program).Assembly))
				   .As<IScenarioTypeFinder>();

			builder.RegisterType<Output>()
				.As<IOutput>()
				.WithParameter("outputStream", Console.OpenStandardOutput())
				.SingleInstance();
			//builder.RegisterInstance(new Output(Console.OpenStandardOutput()))
			//	   .SingleInstance()
			//	   .As<IOutput>();

			builder.RegisterType<Program>()
				   .SingleInstance();
			builder.RegisterType<EndpointProvider>()
				   .SingleInstance()
				   .As<IEndpointProvider>();

			builder.RegisterModule<NLogLoggerAutofacModule>();

			return builder.Build();
		}

		class OutputModule : Autofac.Module
		{
			 
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
