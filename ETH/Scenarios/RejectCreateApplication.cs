using System;
using System.Reflection;
using System.Xml.Serialization;
using ETH.Util;
using eDAIS;
using ETH.Soap;
using FluentAssertions;

namespace ETH.Scenarios
{
	public class RejectCreateApplication : Scenario
	{
		RejectCreateApplicationTransactionType RejectCreateApplicationTransactionType
		{
			get
			{
				var message = Data.FromXml<RejectCreateApplicationTransactionType>("RejectCreateApplicationTransaction.xml");
				return message;
			}
		}

		public void Receive()
		{
			var result = Server.Receive()
				.ToData<RejectCreateApplicationTransactionType>();

			result.Application.RunProposeCreateTests();
			result.StandardBusinessMessageHeader.RunCommonTests();

			Server.Respond("Reject", ReceiptAcknowledgementSignalType);
		}

		public void Send(string applicationNumber = null)
		{
			var message = RejectCreateApplicationTransactionType;
			if (applicationNumber != null) message.Application.ApplicationNumber.Value = applicationNumber;
			var receipt = Client.Send(
				"http://example.xml.gov.au/CreateApplication_Initiator.2.3.0r2/Reject",
				message)
					.ToData<ReceiptAcknowledgementSignalType>();
			receipt.ReceiptAcknowledgement.RunCommonTests();
		}
	}
}
