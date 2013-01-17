using ETH.Http;
using ETH.Util;

namespace ETH.Scenarios
{
	public abstract class ScenarioType
	{
		public IServer Server { get; set; }
		public IClient Client { get; set; }
		public ITestDataLoader Data { get; set; }
	}
}