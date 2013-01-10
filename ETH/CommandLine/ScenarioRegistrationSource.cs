using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Autofac.Features.ResolveAnything;
using ETH.Http;
using ETH.Scenarios;
using ETH.Util;

namespace ETH.CommandLine
{
	class ScenarioRegistrationSource : IRegistrationSource
	{
		readonly AnyConcreteTypeNotAlreadyRegisteredSource inner = 
			new AnyConcreteTypeNotAlreadyRegisteredSource();

		public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
		{
			var result = inner.RegistrationsFor(service, registrationAccessor);
			foreach (var registration in result)
			{
				registration.Activating += (sender, args) =>
				{
					if (args.Instance.GetType().IsSubclassOf(typeof (Scenario)))
					{
						var scenario = (Scenario)args.Instance;
						scenario.Client = args.Context.Resolve<IClient>();
						scenario.Server = args.Context.Resolve<IServer>();
						scenario.Data = args.Context.Resolve<ITestDataLoader>();
					}
				};
				yield return registration;
			}
		}

		public bool IsAdapterForIndividualComponents
		{ 
			get { return inner.IsAdapterForIndividualComponents; } 
		}
	}
}