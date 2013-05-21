using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using ETH.CommandLine;
using ETH.Http;
using ETH.Soap;
using ETH.Util;
using ImpromptuInterface;
using Moq;
using FluentAssertions;
using Xunit;
using System.IO;

namespace ETH.Tests.Http
{
	public class ClientTest
	{
		public class Send
		{
			[Fact]
			public void SendCustomAction()
			{
				var injector = new MockInjector();
				var client = injector.Create<Client>();
				var endpointProvider = injector.GetMock<IEndpointProvider>();
				var webRequestFactory = injector.GetMock<IWebRequestFactory>();
				var request = new Mock<IHttpWebRequest>();
				var response = new Mock<IHttpWebResponse>();

				endpointProvider.Setup(e => e.ClientEndpoint).Returns(new Queue<string>(new[] { "moo" }));
				webRequestFactory.Setup(f => f.CreateHttp("moo")).Returns(request.Object);
				request.Setup(r => r.GetResponse()).Returns(response.Object);

				client.Send(r => r.Should().Be(request.Object))
					  .Should().Be(response.Object);
			}

			[Fact]
			public void SendMissingClientEndpoint()
			{
				var injector = new MockInjector();
				var client = injector.Create<Client>();
				var endpointProvider = injector.GetMock<IEndpointProvider>();
				var webRequestFactory = injector.GetMock<IWebRequestFactory>();
				var request = new Mock<IHttpWebRequest>();
				var response = new Mock<IHttpWebResponse>();

				webRequestFactory.Setup(f => f.CreateHttp("moo")).Returns(request.Object);
				request.Setup(r => r.GetResponse()).Returns(response.Object);

				var ex = Assert.Throws<NotSupportedException>(() => client.Send(r => r.Should().Be(request.Object)));
				ex.Message.Should().Be("Client endpoint should be specified with -c");
			}

			[Fact]
			public void SendCustomActionExceptionStillReturnsResponse()
			{
				var injector = new MockInjector();
				var client = injector.Create<Client>();
				var endpointProvider = injector.GetMock<IEndpointProvider>();
				var webRequestFactory = injector.GetMock<IWebRequestFactory>();
				var request = new Mock<IHttpWebRequest>();
				var response = new Mock<HttpWebResponse>();

				response
					.Setup(r => r.GetResponseStream())
					.Returns(new MemoryStream());
				endpointProvider
					.Setup(e => e.ClientEndpoint)
					.Returns(new Queue<string>(new[] { "moo" }));
				webRequestFactory
					.Setup(f => f.CreateHttp("moo"))
					.Returns(request.Object);
				webRequestFactory
					.Setup(f => f.WithLoggingProxy(response.Object.ActLike<IHttpWebResponse>()))
					.Returns(response.Object.ActLike<IHttpWebResponse>());
				request.Setup(r => r.GetResponse())
					   .Throws(new WebException(
								   "moo",
								   null,
								   WebExceptionStatus.ProtocolError,
								   response.Object));

				client.Send(r => r.Should().Be(request.Object))
					  .Should().Be(response.Object);
			}

			[Fact]
			public void SendCustomActionExceptionNoResponse()
			{
				var injector = new MockInjector();
				var client = injector.Create<Client>();
				var endpointProvider = injector.GetMock<IEndpointProvider>();
				var webRequestFactory = injector.GetMock<IWebRequestFactory>();
				var request = new Mock<IHttpWebRequest>();

				endpointProvider.Setup(e => e.ClientEndpoint).Returns(new Queue<string>(new[] { "moo" }));
				webRequestFactory.Setup(f => f.CreateHttp("moo")).Returns(request.Object);
				request.Setup(r => r.GetResponse())
					   .Throws(new WebException(
								   "moo",
								   null,
								   WebExceptionStatus.ProtocolError,
								   null));

				client.Send(r => r.Should().Be(request.Object))
					.Should().BeNull();
			}

			[Fact]
			public void SendData()
			{
				var injector = new MockInjector();
				var client = injector.Create<Client>();
				var endpointProvider = injector.GetMock<IEndpointProvider>();
				var webRequestFactory = injector.GetMock<IWebRequestFactory>();
				var soapDecoder = injector.GetMock<ISoapDecoder>();
				var securityHeaderFactory = injector.GetMock<ISecurityHeaderFactory>();
				var request = new Mock<IHttpWebRequest>();
				var response = new Mock<IHttpWebResponse>();
				var data = new { };
				var message = Message.CreateMessage(MessageVersion.Default, "moo");

				endpointProvider.Setup(e => e.ClientEndpoint).Returns(new Queue<string>(new[] { "moo" }));
				securityHeaderFactory.Setup(s => s.Create()).Returns(MessageHeader.CreateHeader("Moo", "", ""));
				webRequestFactory.Setup(f => f.CreateHttp("moo")).Returns(request.Object);
				request.Setup(r => r.GetResponse()).Returns(response.Object);
				soapDecoder.Setup(s => s.ToMessage("moo", data))
						   .Returns(message);

				client.Send("moo", data)
					  .Should().Be(response.Object);

				soapDecoder.Verify(s => s.FromMessage(request.Object, message));
			}

			[Fact]
			public void SendMessage()
			{
				var injector = new MockInjector();
				var client = injector.Create<Client>();
				var endpointProvider = injector.GetMock<IEndpointProvider>();
				var webRequestFactory = injector.GetMock<IWebRequestFactory>();
				var soapDecoder = injector.GetMock<ISoapDecoder>();
				var securityHeaderFactory = injector.GetMock<ISecurityHeaderFactory>();
				var request = new Mock<IHttpWebRequest>();
				var response = new Mock<IHttpWebResponse>();
				var message = Message.CreateMessage(MessageVersion.Default, "moo");

				endpointProvider.Setup(e => e.ClientEndpoint).Returns(new Queue<string>(new[] { "moo" }));
				securityHeaderFactory.Setup(s => s.Create()).Returns(MessageHeader.CreateHeader("Moo", "", ""));
				webRequestFactory.Setup(f => f.CreateHttp("moo")).Returns(request.Object);
				request.Setup(r => r.GetResponse()).Returns(response.Object);

				client.Send(message)
					  .Should().Be(response.Object);

				soapDecoder.Verify(s => s.FromMessage(request.Object, message));
			}

			[Fact]
			public void SendMessageSecurityHeader()
			{
				var injector = new MockInjector();
				var client = injector.Create<Client>();
				var endpointProvider = injector.GetMock<IEndpointProvider>();
				var webRequestFactory = injector.GetMock<IWebRequestFactory>();
				var securityHeaderFactory = injector.GetMock<ISecurityHeaderFactory>();
				var soapDecoder = injector.GetMock<ISoapDecoder>();
				var request = new Mock<IHttpWebRequest>();
				var response = new Mock<IHttpWebResponse>();
				var message = Message.CreateMessage(MessageVersion.Default, "moo");
				var messageHeader = MessageHeader.CreateHeader("TestHeader", "", "");

				endpointProvider.Setup(e => e.ClientEndpoint).Returns(new Queue<string>(new[] { "moo" }));
				securityHeaderFactory.Setup(s => s.Create()).Returns(messageHeader);
				webRequestFactory.Setup(f => f.CreateHttp("moo")).Returns(request.Object);
				request.Setup(r => r.GetResponse()).Returns(response.Object);

				client.Send(message);

				message.Headers.Single(h => h.Name == "TestHeader")
					   .Should().Be(messageHeader);
			}
		}
	}
}
