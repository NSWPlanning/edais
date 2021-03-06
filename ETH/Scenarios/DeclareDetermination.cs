﻿using ETH.Http;
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

			Server.Respond("DeclareDetermination", ReceiptAcknowledgementSignalType, result);
		}

		public void Send(string applicationNumber = null)
		{
			var message = DeclareSaveDeterminationNotification;
			if (!string.IsNullOrEmpty(applicationNumber)) message.Application.ApplicationNumber.Value = applicationNumber;
			message.StandardBusinessMessageHeader.ReceiverPartner.Contact[0].EmailAddress = "mary@example.com";
			message.StandardBusinessMessageHeader.SenderPartner.Contact[0].EmailAddress = "mary@example.com";
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
				SetReceiverParty(message.StandardBusinessMessageHeader);
				return message;
			}
		}
	}
}
