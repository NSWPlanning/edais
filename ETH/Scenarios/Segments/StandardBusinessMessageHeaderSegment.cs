using eDAIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace ETH.Scenarios
{
	public static class StandardBusinessMessageHeaderSegment
	{
		public static void RunCommonTests(this StandardBusinessMessageHeader header)
		{
			header.DocumentIdentification.Should().NotBeNull();
			header.DocumentIdentification.InstanceID.Should().NotBeNull();
			header.DocumentIdentification.InstanceID.Value.Should().NotBeNullOrEmpty();
			//Guid instanceId;
			//Guid.TryParse(header.DocumentIdentification.InstanceID.Value, out instanceId).Should().BeTrue("InstanceID should be a GUID");
			header.DocumentIdentification.CreationDateTime.Should().NotBeNull();
			header.BusinessScope.Should().NotBeNull();
			header.BusinessScope.Length.Should().Be(1);
			var businessScope = header.BusinessScope[0];
			businessScope.CorrelationInformation.Should().NotBeNull();
			if (businessScope.CorrelationInformation.Length > 0)
			{
				businessScope.CorrelationInformation.Length.Should().Be(1);
				businessScope.CorrelationInformation[0].RequestingDocumentCreationDateTime.Should().NotBeNull();
				businessScope.CorrelationInformation[0].RequestingDocumentInstanceID.Should().NotBeNull();
			}
			businessScope.Type.Should().NotBeNull();
			TestPartner(header.ReceiverPartner);
			TestPartner(header.SenderPartner);
		}

		static void TestPartner(Partner partner)
		{
			partner.Should().NotBeNull();
			partner.Authority.Should().NotBeNull();
			partner.Contact.Should().NotBeNull();
			partner.Contact.Length.Should().Be(1);
			partner.Contact[0].Contact.Should().NotBeNull();
			partner.Contact[0].ContactTypeIdentifier.Should().NotBeNull();
			partner.Contact[0].EmailAddress.Should().NotBeNullOrEmpty();
			partner.Contact[0].TelephoneNumber.Should().NotBeNull();
			partner.ID.Should().NotBeNull();
			partner.ProcessRole.Should().NotBeNull();
		}
	}
}
