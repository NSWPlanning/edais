using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETH.Scenarios
{
	public class Council_EndToEnd : Scenario
	{
		public void AsCertifier()
		{
			// Wait to receive a ProposeCreate and send back an ack
			// Send a ProposeAccept and receive an ack
			// Wait to receive a DeclareDetermination and send back an ack
			
			var accept = new AcceptCreateApplication();
			accept.Send();

		}
	}
}
