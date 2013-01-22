using System.Reflection;
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
				return message;
			}
		}

		public void Receive()
		{
			var result = Soap.ToData<AcceptCreateApplicationTransactionType>(Server.Receive());

			result.Application.RunCommonTests();
			result.StandardBusinessMessageHeader.RunCommonTests();

			Server.Respond("Accept", ReceiptAcknowledgementSignalType);
		}

		public void Send(string applicationNumber = null)
		{
			var message = AcceptCreateApplicationTransactionType;
			if (applicationNumber != null) message.Application.ApplicationNumber.Value = applicationNumber;
			var response = Client.Send(
				"http://example.xml.gov.au/CreateApplication_Initiator.2.3.0r2/Accept",
				AcceptCreateApplicationTransactionType);

			Soap.ToData<ReceiptAcknowledgementSignalType>(response)
			    .ReceiptAcknowledgement.RunCommonTests();
		}
	}
};