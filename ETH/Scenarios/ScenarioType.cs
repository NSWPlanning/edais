using Autofac;
using ETH.Http;
using ETH.Soap;
using ETH.Util;

namespace ETH.Scenarios
{
	public abstract class ScenarioType
	{
		public IServer Server { get; set; }
		public IClient Client { get; set; }
		public ISoapDecoder Soap { get; set; }
		public ITestDataLoader Data { get; set; }
		public ILifetimeScope Container { get; set; }
	}
}