using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ETH.Soap;
using System.Threading.Tasks;
using eDAIS;
using FluentAssertions;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using ETH.Util;

namespace ETH.Scenarios
{

	public class ProposeCreateApplication : Scenario
	{
		public ApplicationInformation Receive()
		{
			var result = new ApplicationInformation();
			var proposeCreate = Soap.ToData<ProposeCreateApplicationTransactionType>(
				Server.Receive());

			proposeCreate.Application.RunCommonTests();
			result.ApplicationNumber = proposeCreate.Application.ApplicationNumber.Value;
			proposeCreate.StandardBusinessMessageHeader.RunCommonTests();

			proposeCreate.Specialisation.Should().NotBeNull();
			proposeCreate.Specialisation.Count().Should().Be(2);
			proposeCreate.Specialisation[0].Jurisdiction.Should().NotBeNull();
			proposeCreate.Specialisation[0].Version.Should().NotBeNull();
			proposeCreate.Specialisation[0].Extension.Should().NotBeNull();
			proposeCreate.Specialisation[0].Extension.Length.Should().Be(1);
			proposeCreate.Specialisation[0].Extension[0].Name.Should().Be("Application");
			var applicationExtension = proposeCreate.Specialisation[0].Extension[0].ConvertNode<Application1>();
			applicationExtension.Type.Should().NotBeNull();
			applicationExtension.DevelopmentCode.Should().NotBeNull();
			applicationExtension.DevelopmentCode.Length.Should().BeGreaterThan(0);
			applicationExtension.BCAClass.Should().NotBeNull();
			applicationExtension.RefNumber.Should().NotBeNull();

			proposeCreate.Specialisation[1].Jurisdiction.Should().NotBeNull();
			proposeCreate.Specialisation[1].Version.Should().NotBeNull();
			proposeCreate.Specialisation[1].Extension.Should().NotBeNull();
			proposeCreate.Specialisation[1].Extension.Length.Should().Be(1);
			proposeCreate.Specialisation[1].Extension[0].Name.Should().Be("SubjectLand");
			var subjectLandExtension = proposeCreate.Specialisation[1].Extension[0].ConvertNode<SubjectLand1>();
			subjectLandExtension.Should().NotBeNull();
			subjectLandExtension.ABS_Statistics.Should().NotBeNull();
			subjectLandExtension.ABS_Statistics.Floor.Should().NotBeNull();
			subjectLandExtension.ABS_Statistics.Floor.Length.Should().Be(1);
			subjectLandExtension.ABS_Statistics.Floor[0].Value.Should().NotBeNullOrEmpty();
			subjectLandExtension.ABS_Statistics.Frame.Should().NotBeNull();
			subjectLandExtension.ABS_Statistics.Frame.Length.Should().Be(1);
			subjectLandExtension.ABS_Statistics.Frame[0].Value.Should().NotBeNullOrEmpty();
			subjectLandExtension.ABS_Statistics.Roof.Should().NotBeNull();
			subjectLandExtension.ABS_Statistics.Roof.Length.Should().Be(1);
			subjectLandExtension.ABS_Statistics.Roof[0].Value.Should().NotBeNullOrEmpty();
			subjectLandExtension.ABS_Statistics.Walls.Should().NotBeNull();
			subjectLandExtension.ABS_Statistics.Walls.Length.Should().Be(1);
			subjectLandExtension.ABS_Statistics.Walls[0].Value.Should().NotBeNullOrEmpty();

			Server.Respond("ProposeCreate", ReceiptAcknowledgementSignalType);
			return result;
		}

		

		public void Send(string applicationNumber = null, string certifierEmail = null)
		{
			var message = ProposeCreateApplicationTransactionType;
			if (applicationNumber != null) message.Application.ApplicationNumber.Value = applicationNumber;
			if (certifierEmail != null) message.StandardBusinessMessageHeader.ReceiverPartner.Contact[0].EmailAddress = certifierEmail;
			var response = Client.Send(
				"http://example.xml.gov.au/CreateApplication_Responder.2.3.0r2/Propose",
				message);
			var responsData = Soap.ToData<ReceiptAcknowledgementSignalType>(response);
			if (responsData != null)
			{
				responsData.ReceiptAcknowledgement.RunCommonTests();
			}
			else
			{
				responsData.Should().NotBeNull("expected a response from the server");
			}
		}
		
		ProposeCreateApplicationTransactionType ProposeCreateApplicationTransactionType
		{
			get
			{
				var message = Data.FromXml<ProposeCreateApplicationTransactionType>("ProposeCreateApplicationTransaction.xml");
				return message;
			}
		}
	}
}
