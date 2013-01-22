using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETH.Scenarios
{
	public class EHC_EndToEnd : EndToEndScenario
	{
		public void ToThirdParty_Accept(string applicationNumber = null)
		{
			ProposeCreateApplication.Send(applicationNumber);
			AcceptCreateApplication.Receive();
			DeclareDetermination.Receive();
		}

		public void ToThirdParty_Reject(string applicationNumber = null)
		{
			ProposeCreateApplication.Send(applicationNumber);
			RejectCreateApplication.Receive();
		}
	}
}
