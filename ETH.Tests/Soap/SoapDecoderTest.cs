using System;
using ETH.CommandLine;
using ETH.Soap;
using ETH.Util;
using Moq;
using Xunit;
using FluentAssertions;
using ETH.Http;
using System.ServiceModel.Channels;
using System.IO;
using System.Xml.Serialization;

namespace ETH.Tests.Soap
{
	public class SoapDecoderTest
	{
		[Fact]
		public void FromMessage()
		{
			var injector = new MockInjector();
			SoapDecoder decoder = injector.Create<SoapDecoder>();
			var contentTypeProvider = injector.GetMock<IContentTypeProvider>();
			var messageProvider = injector.GetMock<IMessageProvider>();
			var endpointProvider = injector.GetMock<IEndpointProvider>();
			var securityHeaderFactory = injector.GetMock<ISecurityHeaderFactory>();

			var response = injector.GetMock<IHttpListenerResponse>();
			var responseMessage = Message.CreateMessage(MessageVersion.Default, "moo");
			var requestMessage = Message.CreateMessage(MessageVersion.Default, "moo");
			var request = injector.GetMock<IHttpListenerRequest>();

			var stream = new MemoryStream();
			messageProvider.Setup(m => m.GetMessage(stream)).Returns(requestMessage);
			request.Setup(r => r.InputStream).Returns(stream);
			request.Setup(r => r.ContentType).Returns("text/xml");
			endpointProvider.Setup(e => e.SkipAuthentication).Returns(true);
			response.Setup(r => r.OutputStream).Returns(stream);
			contentTypeProvider.Setup(p => p.ValidContentTypes).Returns(new[] { "text/xml" });

			var securityHeader = MessageHeader.CreateHeader("security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd", "secure");
			securityHeaderFactory.Setup(s => s.CreateWithoutUserToken(requestMessage)).Returns(securityHeader);
			decoder.FromMessage(response.Object, responseMessage, request.Object);

			responseMessage.Headers.Should().HaveCount(2)
				.And.Contain(securityHeader);
		}
	}
}
