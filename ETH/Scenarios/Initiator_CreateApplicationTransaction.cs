using System;
using System.Reflection;
using System.Xml.Serialization;
using ETH.Util;
using eDAIS;
using ETH.Soap;
using FluentAssertions;

namespace ETH.Scenarios
{
	public class Initiator_CreateApplicationTransaction_Accept : Scenario
	{
		AcceptCreateApplicationTransactionType AcceptCreateApplicationTransactionType
		{
			get
			{
				return Data.FromXml<AcceptCreateApplicationTransactionType>(
					"AcceptCreateApplicationTransactionType.xml");
			}
		}

		ReceiptAcknowledgementSignalType ReceiptAcknowledgementSignalType
		{
			get
			{
				return Data.FromXml<ReceiptAcknowledgementSignalType>(
					"ReceiptAcknowledgementSignal.xml");
			}
		}

		public void Receive()
		{
			Server.Receive()
				.ToData<AcceptCreateApplicationTransactionType>()
				.ShouldBeEquivalentTo(AcceptCreateApplicationTransactionType);

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
}