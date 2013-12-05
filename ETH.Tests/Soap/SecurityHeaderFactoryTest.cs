using System;
using ETH.CommandLine;
using ETH.Soap;
using ETH.Util;
using Moq;
using Xunit;
using FluentAssertions;
using System.ServiceModel.Channels;

namespace ETH.Tests.Soap
{
	public class SecurityHeaderFactoryTest
	{
		public class Create
		{
			[Fact]
			public void Default()
			{
				var injector = new MockInjector();
				var factory = injector.Create<SecurityHeaderFactory>();
				var endpointProvider = injector.GetMock<IEndpointProvider>();
				var dateTimeProvider = injector.GetMock<IDateTimeProvider>();
				var randomProvider = injector.GetMock<IRandomNumberGeneratorProvider>();

				endpointProvider.Setup(e => e.Username).Returns("someuser");
				endpointProvider.Setup(e => e.Password).Returns("somepass");
				dateTimeProvider.Setup(d => d.Now).Returns(new DateTime(2013, 01, 20));
				randomProvider.Setup(r => r.GetBytes(It.IsAny<byte[]>()))
				              .Callback<byte[]>(bytes =>
				              {
					              for (int i = 0; i < bytes.Length; i++)
					              {
						              bytes[i] = 7;
					              }
				              });

				var header = factory.Create();

				header.Name.Should().Be("Security");
				header.Namespace.Should().Be("http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");

				const string expectedXml =
@"<Security xmlns=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <UsernameToken>
    <Username>someuser</Username>
    <Password Type=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText"">somepass</Password>
    <Nonce EncodingType=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary"">BwcHBwcHBwcHBwcHBwcHBwcHBwcHBwcHBwcHBwcHBwcHBwcHBwcHBwcHBwcHBwcHBwcHBwcHBwcHBwcHBwcHBw==</Nonce>
    <Created xmlns=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"">2013-01-20T00:00:00.000Z</Created>
  </UsernameToken>
</Security>";

				header.ToString().Should().Be(expectedXml);
			}
		}

		public class CreateWithoutUserToken
		{
			[Fact]
			public void Default()
			{
				var injector = new MockInjector();
				var factory = injector.Create<SecurityHeaderFactory>();

				var security = new Security { Timestamp = new Timestamp { Created = "2012-11-07T01:41:35.821Z" } };

				var requestMessage = Message.CreateMessage(MessageVersion.Default, "moo");
				MessageHeader securityHeader = MessageHeader.CreateHeader("Security",
										"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd",
										security,
										new CfMessagingSerializer(typeof(Security)));
				requestMessage.Headers.Add(securityHeader);
				var header = factory.CreateWithoutUserToken(requestMessage);

				header.Name.Should().Be("Security");
				header.Namespace.Should().Be("http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");


				string expectedXml =
@"<Security xmlns=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Timestamp xmlns=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"">
    <Created>2012-11-07T01:41:35.821Z</Created>
  </Timestamp>
</Security>";
				header.ToString().Should().Be(expectedXml);
			}
		}

	}
}