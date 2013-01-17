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

			result.Application.RunProposeCreateTests();
			result.StandardBusinessMessageHeader.RunCommonTests();

			Server.Respond("Accept", ReceiptAcknowledgementSignalType);
		}

		public void Send()
		{
			Client.Send(
				"http://example.xml.gov.au/CreateApplication_Initiator.2.3.0r2/Accept", 
				AcceptCreateApplicationTransactionType)
					.ToData<ReceiptAcknowledgementSignalType>()
					.ShouldBeEquivalentTo(ReceiptAcknowledgementSignalType);
		}
	}
};