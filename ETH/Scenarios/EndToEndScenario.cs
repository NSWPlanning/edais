using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace ETH.Scenarios
{
	public abstract class EndToEndScenario : Scenario
	{
		protected ProposeCreateApplication ProposeCreateApplication
		{
			get
			{
				return Container.Resolve<ProposeCreateApplication>();
			}
		}

		protected AcceptCreateApplication AcceptCreateApplication
		{
			get
			{
				return Container.Resolve<AcceptCreateApplication>();
			}
		}

		protected DeclareDetermination DeclareDetermination
		{
			get
			{
				return Container.Resolve<DeclareDetermination>();
			}
		}
	}
}
