using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETH.Scenarios
{
	public class Council_EndToEnd : EndToEndScenario
	{
		// Pretend to be council to test a private certifier system
		public void FromCertifier_Accept()
		{
			// Wait to receive a ProposeCreate and send back an ack
			// Send a ProposeAccept and receive an ack
			// Wait to receive a DeclareDetermination and send back an ack
			var applicationResult = ProposeCreateApplication.Receive();
			AcceptCreateApplication.Send(applicationResult.ApplicationNumber);
			DeclareDetermination.Receive();
		}

		// Pretend to be council to test the EHC
		public void FromEHC_Accept()
		{
			// Wait to receive a ProposeCreate and send back an ack
			// Send a ProposeAccept and receive an ack
			// Send a DeclareDetermination and receive an ack
			var applicationResult = ProposeCreateApplication.Receive();
			AcceptCreateApplication.Send(applicationResult.ApplicationNumber);
			DeclareDetermination.Send();
		}

		public void FromCertifier_Reject()
		{
			Reject();
		}

		public void FromEHC_Reject()
		{
			Reject();
		}

		void Reject()
		{
			var applicationResult = ProposeCreateApplication.Receive();
			RejectCreateApplication.Send(applicationResult.ApplicationNumber);
		}
	}
}
