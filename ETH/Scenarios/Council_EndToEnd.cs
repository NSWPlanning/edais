using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETH.Scenarios
{
	public class Council_EndToEnd : EndToEndScenario
	{
		public void FromCertifier()
		{
			// Wait to receive a ProposeCreate and send back an ack
			// Send a ProposeAccept and receive an ack
			// Wait to receive a DeclareDetermination and send back an ack
			var applicationResult = ProposeCreateApplication.Receive();
			AcceptCreateApplication.Send(applicationResult.ApplicationNumber);
			DeclareDetermination.Receive();
		}

		public void FromEHC()
		{
			// Wait to receive a ProposeCreate and send back an ack
			// Send a ProposeAccept and receive an ack
			// Send a DeclareDetermination and receive an ack
			ProposeCreateApplication.Receive();
			AcceptCreateApplication.Send();
			DeclareDetermination.Send();
		}
	}
}
