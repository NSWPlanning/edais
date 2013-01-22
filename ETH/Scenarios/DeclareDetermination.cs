using ETH.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ETH.Soap;
using eDAIS;
using FluentAssertions;
using ETH.Util;

namespace ETH.Scenarios
{
	public class DeclareDetermination : Scenario
	{
		public void Receive()
		{
			var result = Server.Receive();
			Soap.ToData<DeclareSaveDeterminationNotificationType>(result);

			Server.Respond("DeclareDetermination", ReceiptAcknowledgementSignalType);
		}

		public void Send()
		{
			Send(null);
		}

		internal void Send(ApplicationInformation applicationInformation)
		{
			var message = DeclareSaveDeterminationNotification;
			if (applicationInformation != null) message.Application.ApplicationNumber.Value = applicationInformation.ApplicationNumber;
			var response = Client.Send(
				"http://example.xml.gov.au/DeclareDetermination_Responder.2.3.0r2/Declare",
				message);
			Soap.ToData<ReceiptAcknowledgementSignalType>(response)
				.ReceiptAcknowledgement.RunCommonTests();
		}

		DeclareSaveDeterminationNotificationType DeclareSaveDeterminationNotification
		{
			get
			{
				var message = Data.FromXml<DeclareSaveDeterminationNotificationType>("DeclareSaveDeterminationNotification.xml");
				return message;
			}
		}
	}
}
