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
				SetReceiverParty(message.StandardBusinessMessageHeader);
				return message;
			}
		}

		public void Receive()
		{
			var request = Server.Receive();
			var result = Soap.ToData<RejectCreateApplicationTransactionType>(request);

			result.Application.RunProposeCreateTests();
			result.StandardBusinessMessageHeader.RunCommonTests();

			Server.Respond("Reject", ReceiptAcknowledgementSignalType, request);
		}

		public void Send(string applicationNumber = null)
		{
			var message = RejectCreateApplicationTransactionType;
			if (applicationNumber != null) message.Application.ApplicationNumber.Value = applicationNumber;
			var response = Client.Send(
				"http://example.xml.gov.au/CreateApplication_Initiator.2.3.0r2/Reject",
				message);
			var receipt = Soap.ToData<ReceiptAcknowledgementSignalType>(response);
			receipt.ReceiptAcknowledgement.RunCommonTests();
		}
	}
}
