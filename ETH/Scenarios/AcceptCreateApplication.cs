﻿using System.Reflection;
using System.ServiceModel;
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
				SetReceiverParty(message.StandardBusinessMessageHeader);
				return message;
			}
		}

		public void Receive()
		{
			var request = Server.Receive();
			var result = Soap.ToData<AcceptCreateApplicationTransactionType>(request);

			result.Application.RunCommonTests();
			result.StandardBusinessMessageHeader.RunCommonTests();

			Server.Respond("Accept", ReceiptAcknowledgementSignalType, request);
		}

		public void Send(string applicationNumber = null)
		{
			var message = AcceptCreateApplicationTransactionType;
			if (applicationNumber != null) message.Application.ApplicationNumber.Value = applicationNumber;
			var response = Client.Send(
				"http://example.xml.gov.au/CreateApplication_Initiator.2.3.0r2/Accept",
				message);

			Soap.ToData<ReceiptAcknowledgementSignalType>(response)
			    .ReceiptAcknowledgement.RunCommonTests();
		}
	}
};