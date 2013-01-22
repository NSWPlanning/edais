using System;
using System.Reflection;
using System.Xml.Serialization;
using ETH.Util;
using eDAIS;
using ETH.Soap;
using FluentAssertions;

namespace ETH.Scenarios
{
	public class AcceptCreateApplication : Scenario
	{
		AcceptCreateApplicationTransactionType AcceptCreateApplicationTransactionType
		{
			get
			{
				var message = Data.FromXml<AcceptCreateApplicationTransactionType>("AcceptCreateApplicationTransaction.xml");
				return message;
			}
		}

		public void Receive()
		{
			var result = Server.Receive()
				.ToData<AcceptCreateApplicationTransactionType>();

			result.Application.RunCommonTests();
			result.StandardBusinessMessageHeader.RunCommonTests();

			Server.Respond("Accept", ReceiptAcknowledgementSignalType);
		}

		public void Send(string applicationNumber = null)
		{
			var message = AcceptCreateApplicationTransactionType;
			if (applicationNumber != null) message.Application.ApplicationNumber.Value = applicationNumber;
			var receipt = Client.Send(
				"http://example.xml.gov.au/CreateApplication_Initiator.2.3.0r2/Accept",
				message)
					.ToData<ReceiptAcknowledgementSignalType>();
			receipt.ReceiptAcknowledgement.RunCommonTests();
		}
	}
};