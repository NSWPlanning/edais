using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETH.Scenarios
{
	public class EHC_EndToEnd : EndToEndScenario
	{
		public void ToThirdParty_Accept(string applicationNumber = null, string certifierEmail = null)
		{
			ProposeCreateApplication.Send(applicationNumber, certifierEmail);
			AcceptCreateApplication.Receive();
			DeclareDetermination.Receive();
		}

		public void ToThirdParty_Reject(string applicationNumber = null, string certifierEmail = null)
		{
			ProposeCreateApplication.Send(applicationNumber, certifierEmail);
			RejectCreateApplication.Receive();
		}
	}
}
