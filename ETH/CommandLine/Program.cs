﻿using System;
using System.Diagnostics;
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
		readonly ILogger logger;
		readonly IOutput output;

		public static IContainer Container { get; private set; } // TODO: refactor out

		static int Main(string[] args)
		{
			var parser = GetParser();
			var options = new Options();

			if (!parser.ParseArguments(args, options))
			{
				Console.Out.Write(options.GetUsage());
				return 1;
			}

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
				Layout = @"[${date:format=yyyy-MM-dd HH\:mm\:ss.fff}] [${logger}] ${message}"
			};
			configuration.AddTarget("file", fileTarget);			
			configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, fileTarget));

			var consoleTarget = new ColoredConsoleTarget
			{
				ErrorStream = true
			};
			configuration.AddTarget("console", consoleTarget);
			configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, consoleTarget));

			LogManager.Configuration = configuration;

			LogManager.GetCurrentClassLogger().Info("Arguments: {0}", args.Dump());
			LogManager.GetCurrentClassLogger().Info("Options: {0}", options.Dump());
		}

		public Program(
			IRunner scenarioRunner, 
			IEndpointProvider endpointProvider, 
			IOutput output, 
			IScenarioTypeFinder scenarioTypeFinder, 
			ILogger logger)
		{
			this.scenarioRunner = scenarioRunner;
			this.endpointProvider = endpointProvider;
			this.output = output;
			this.scenarioTypeFinder = scenarioTypeFinder;
			this.logger = logger;
		}

		public int Run(Options options)
		{
			if (options.Scenario != null)
			{
				endpointProvider.ServerBaseUrl = options.ListenUrl;
				if (options.ClientUrl != null && options.ClientUrl.Length > 0)
					endpointProvider.ClientEndpoint = new Queue<string>(options.ClientUrl);
				endpointProvider.Username = options.Username;
				endpointProvider.Password = options.Password;
				endpointProvider.SkipAuthentication = options.SkipAuthentication;

				if (options.UseSoap11)
				{
					endpointProvider.MessageVersion = MessageVersion.Soap11WSAddressing10;
				}

				try
				{
					scenarioRunner.Run(options.Scenario, options.ScenarioArguments, options.ReceiverParty);
					output.Display(new ResultModel
					{
						Result = Result.Pass
					});
				}
				catch (Exception e)
				{
					var baseException = e.GetBaseException();
					var failException = baseException as FailException;
					logger.Error(string.Format("Exception:\n{0}", e));
					if (failException != null)
					{
						logger.Error(string.Format("Wrapped Exception:\n{0}", failException.WrappedException));
					}
					output.Display(new ResultModel
					{
						Result = Result.Fail,
						Message = (failException ?? e).ToString()
					});
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
				output.String(helpText, log: false);
				return 1;
			}
			else if (options.ScenarioInfo != null)
			{
				throw new NotImplementedException();
			}
			else
			{
				output.String(options.GetUsage(), log: false);
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
			builder.RegisterType<ContentTypeProvider>().As<IContentTypeProvider>();
			builder.RegisterType<MessageProvider>().As<IMessageProvider>();
			builder.RegisterType<XmlDocumentProvider>().As<IXmlDocumentProvider>();

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
