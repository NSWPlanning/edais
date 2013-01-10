using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.ServiceModel.Channels;
using System.Text;
using ETH.CommandLine;
using ETH.Http;
using Moq;
using Xunit;
using FluentAssertions;

namespace ETH.Tests.Http
{
	public class ServerTest
	{
		public class Construction
		{
			[Fact]
			public void Prefixes()
			{
				var listener = CreateListener();
				var baseUrlProvider = new Mock<IEndpointProvider>();
				baseUrlProvider.Setup(b => b.ServerBaseUrl).Returns("moo");

				var server = new Server(listener.Object, baseUrlProvider.Object);

				listener.Object.Prefixes.Should().BeEquivalentTo(new[] {"moo"});
			}
		}

		public class Receive
		{
			[Fact]
			public void OnceDefault()
			{
				var listener = CreateListener();
				var server = new Server(listener.Object, new Mock<IEndpointProvider>().Object);
				var request = new Mock<IHttpListenerRequest>();

				listener.Setup(l => l.IsListening).Returns(false);
				listener.Setup(l => l.GetContextAsync().Result.Request).Returns(request.Object);

				server.Receive()
				      .Should().Be(request.Object);

				listener.Verify(l => l.Start(), Times.Once());
			}

			[Fact]
			public void OnceWhenAlreadyListening()
			{
				var listener = CreateListener();
				var server = new Server(listener.Object, new Mock<IEndpointProvider>().Object);
				var request = new Mock<IHttpListenerRequest>();

				listener.Setup(l => l.IsListening).Returns(true);
				listener.Setup(l => l.GetContextAsync().Result.Request).Returns(request.Object);

				server.Receive()
					  .Should().Be(request.Object);

				listener.Verify(l => l.Start(), Times.Never());
			}

			[Fact]
			public void TwiceWhenPreviousRequestNotRespondedTo()
			{
				var listener = CreateListener();
				var server = new Server(listener.Object, new Mock<IEndpointProvider>().Object);
				var request = new Mock<IHttpListenerRequest>();

				listener.Setup(l => l.GetContextAsync().Result.Request).Returns(request.Object);

				server.Receive();

				server.Invoking(s => s.Receive())
				      .ShouldThrow<InvalidOperationException>();
			}

			[Fact]
			public void TwiceWhenPreviousResponseAborted()
			{
				var listener = CreateListener();
				var server = new Server(listener.Object, new Mock<IEndpointProvider>().Object);
				var request1 = new Mock<IHttpListenerRequest>();
				var response1 = new Mock<IHttpListenerResponse>();
				var request2 = new Mock<IHttpListenerRequest>();

				listener.SetupSequence(l => l.GetContextAsync().Result.Request)
				        .Returns(request1.Object)
				        .Returns(request2.Object);

				listener.Setup(l => l.GetContextAsync().Result.Response)
				        .Returns(response1.Object);

				server.Receive();
				server.AbortResponse();
				server.Receive()
				      .Should().Be(request2.Object);
			}
		}

		public class Respond
		{
			[Fact]
			public void Abort()
			{
				var listener = CreateListener();
				var server = new Server(listener.Object, new Mock<IEndpointProvider>().Object);
				var request = new Mock<IHttpListenerRequest>();
				var response = new Mock<IHttpListenerResponse>();

				listener.Setup(l => l.GetContextAsync().Result.Request).Returns(request.Object);
				listener.Setup(l => l.GetContextAsync().Result.Response).Returns(response.Object);

				server.Receive();
				server.AbortResponse();

				response.Verify(r => r.Abort(), Times.Once());
			}

			[Fact]
			public void RespondWithCustomAction()
			{
				var listener = CreateListener();
				var server = new Server(listener.Object, new Mock<IEndpointProvider>().Object);
				var request = new Mock<IHttpListenerRequest>();
				var response = new Mock<IHttpListenerResponse>();

				listener.Setup(l => l.GetContextAsync().Result.Request).Returns(request.Object);
				listener.Setup(l => l.GetContextAsync().Result.Response).Returns(response.Object);

				bool called = false;
				server.Receive();
				server.Respond(r =>
				{
					r.Should().Be(response.Object);
					called = true;
				});

				called.Should().BeTrue();
			}

			[Fact]
			public void RespondWithMessage()
			{
				var listener = CreateListener();
				var server = new Server(listener.Object, new Mock<IEndpointProvider>().Object);
				var request = new Mock<IHttpListenerRequest>();
				var response = new Mock<IHttpListenerResponse>();
				var responseStream = new MemoryStream();
				var message = Message.CreateMessage(MessageVersion.Default, "moo");

				listener.Setup(l => l.GetContextAsync().Result.Request).Returns(request.Object);
				listener.Setup(l => l.GetContextAsync().Result.Response).Returns(response.Object);
				response.Setup(r => r.OutputStream).Returns(responseStream);

				server.Receive();
				server.Respond(message);

				response.Verify(r => r.OutputStream);
				response.VerifySet(r => r.ContentType = "application/soap+xml");
				responseStream.CanWrite.Should().BeFalse();
				responseStream.CanRead.Should().BeFalse();

				var responseOutput = Encoding.Default.GetString(responseStream.ToArray());
				responseOutput.Should().Contain("envelope");				
			}

			[Fact]
			public void RespondWithData()
			{
				var listener = CreateListener();
				var server = new Server(listener.Object, new Mock<IEndpointProvider>().Object);
				var request = new Mock<IHttpListenerRequest>();
				var response = new Mock<IHttpListenerResponse>();
				var responseStream = new MemoryStream();
				var data = new KeyValuePair<string, string>("moo", "baa");

				listener.Setup(l => l.GetContextAsync().Result.Request).Returns(request.Object);
				listener.Setup(l => l.GetContextAsync().Result.Response).Returns(response.Object);
				response.Setup(r => r.OutputStream).Returns(responseStream);

				server.Receive();
				server.Respond("moo", data);

				response.Verify(r => r.OutputStream);
				response.VerifySet(r => r.ContentType = "application/soap+xml");
				responseStream.CanRead.Should().BeFalse();
				responseStream.CanWrite.Should().BeFalse();

				var responseOutput = Encoding.Default.GetString(responseStream.ToArray());
				responseOutput.Should()
				              .Contain("KeyValuePair")
				              .And.Contain("envelope");
			}

			[Fact]
			public void RespondWithXmlNotImplementedYet()
			{
				var listener = CreateListener();
				var server = new Server(listener.Object, new Mock<IEndpointProvider>().Object);
				var request = new Mock<IHttpListenerRequest>();

				listener.Setup(l => l.GetContextAsync().Result.Request).Returns(request.Object);

				server.Receive();
				server.Invoking(s => s.Respond("xml")).ShouldThrow<NotImplementedException>();
			}

			[Fact]
			public void RespondWhenNoRequest()
			{
				var listener = CreateListener();
				var server = new Server(listener.Object, new Mock<IEndpointProvider>().Object);

				server.Invoking(s => s.Respond(r => r.Abort()))
				      .ShouldThrow<InvalidOperationException>()
					  .WithMessage("no request", ComparisonMode.Substring);
			}

			[Fact]
			public void RespondOnlyWorksOnce()
			{
				var listener = CreateListener();
				var server = new Server(listener.Object, new Mock<IEndpointProvider>().Object);
				var request = new Mock<IHttpListenerRequest>();
				var response = new Mock<IHttpListenerResponse>();

				listener.Setup(l => l.GetContextAsync().Result.Request).Returns(request.Object);
				listener.Setup(l => l.GetContextAsync().Result.Response).Returns(response.Object);

				server.Receive();
				server.Respond(r => r.ContentType = "butts");
				server.Invoking(s => s.Respond(r => r.ContentType = "moo"))
				      .ShouldThrow<InvalidOperationException>()
				      .WithMessage("no request", ComparisonMode.Substring);
			}
		}

		public class Dispose
		{
			[Fact]
			public void StopsListener()
			{
				var listener = CreateListener();
				var server = new Server(listener.Object, new Mock<IEndpointProvider>().Object);

				server.Dispose();

				listener.Verify(l => l.Stop(), Times.Once());
			}

			[Fact]
			public void OnlyDisposesOnce()
			{
				var listener = CreateListener();
				var server = new Server(listener.Object, new Mock<IEndpointProvider>().Object);

				server.Dispose();
				server.Dispose();

				listener.Verify(l => l.Stop(), Times.Once());	
			}

			[Fact]
			public void IgnoresInvalidOperationFromStoppingListener()
			{
				var listener = CreateListener();
				var server = new Server(listener.Object, new Mock<IEndpointProvider>().Object);

				listener.Setup(l => l.Stop()).Throws<InvalidOperationException>();

				server.Dispose();
			}

			[Fact]
			public void AllowsOtherExceptionsThroughFromStoppingListener()
			{
				var listener = CreateListener();
				var server = new Server(listener.Object, new Mock<IEndpointProvider>().Object);

				listener.Setup(l => l.Stop()).Throws<ArgumentException>();

				server.Invoking(s => s.Dispose()).ShouldThrow<ArgumentException>();
			}
		}

		static Mock<IHttpListener> CreateListener()
		{
			var listener = new Mock<IHttpListener>();
			listener.Setup(l => l.Prefixes).Returns(new List<string>());
			return listener;
		}
	}
}