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

		public void Receive()
		{
			var result = Server.Receive()
				.ToData<ProposeCreateApplicationTransactionType>();

			result.Application.RunCommonTests();
			result.StandardBusinessMessageHeader.RunCommonTests();

			result.Specialisation.Should().NotBeNull();
			result.Specialisation.Count().Should().Be(2);
			result.Specialisation[0].Jurisdiction.Should().NotBeNull();
			result.Specialisation[0].Version.Should().NotBeNull();
			result.Specialisation[0].Extension.Should().NotBeNull();
			result.Specialisation[0].Extension.Length.Should().Be(1);
			result.Specialisation[0].Extension[0].Name.Should().Be("Application");
			var applicationExtension = result.Specialisation[0].Extension[0].ConvertNode<Application1>();
			applicationExtension.Type.Should().NotBeNull();
			applicationExtension.DevelopmentCode.Should().NotBeNull();
			applicationExtension.DevelopmentCode.Length.Should().BeGreaterThan(0);
			applicationExtension.BCAClass.Should().NotBeNull();
			applicationExtension.RefNumber.Should().NotBeNull();

			result.Specialisation[1].Jurisdiction.Should().NotBeNull();
			result.Specialisation[1].Version.Should().NotBeNull();
			result.Specialisation[1].Extension.Should().NotBeNull();
			result.Specialisation[1].Extension.Length.Should().Be(1);
			result.Specialisation[1].Extension[0].Name.Should().Be("SubjectLand");
			var subjectLandExtension = result.Specialisation[1].Extension[0].ConvertNode<SubjectLand1>();
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
		}

		

		public void Send()
		{
			Client.Send(
				"http://example.xml.gov.au/CreateApplication_Initiator.2.3.0r2/ProposeCreate",
				ProposeCreateApplicationTransactionType)
					.ToData<ReceiptAcknowledgementSignalType>()
					.ShouldBeEquivalentTo(ReceiptAcknowledgementSignalType);
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
