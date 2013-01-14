using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel.Channels;
using System.Text;
using ETH.CommandLine;
using ETH.Http;
using FluentAssertions;
using Moq;
using Xunit;

namespace ETH.Tests.Http
{
	public class ServerTest
	{
		public class Construction
		{
			[Fact]
			public void Prefixes()
			{
				var injector = new MockInjector();
				var listener = injector.SetupAndGetListener();
				var baseUrlProvider = injector.GetMock<IEndpointProvider>();
				baseUrlProvider.Setup(b => b.ServerBaseUrl).Returns("moo");

				var server = injector.Create<Server>();

				listener.Object.Prefixes.Should().BeEquivalentTo(new[] {"moo"});
			}
		}

		public class Dispose
		{
			[Fact]
			public void StopsListener()
			{
				var injector = new MockInjector();
				var listener = injector.SetupAndGetListener();
				var server = injector.Create<Server>();

				server.Dispose();

				listener.Verify(l => l.Stop(), Times.Once());
			}

			[Fact]
			public void OnlyDisposesOnce()
			{
				var injector = new MockInjector();
				var listener = injector.SetupAndGetListener();
				var server = injector.Create<Server>();

				server.Dispose();
				server.Dispose();

				listener.Verify(l => l.Stop(), Times.Once());
			}

			[Fact]
			public void IgnoresInvalidOperationFromStoppingListener()
			{
				var injector = new MockInjector();
				var listener = injector.SetupAndGetListener();
				var server = injector.Create<Server>();

				listener.Setup(l => l.Stop()).Throws<InvalidOperationException>();

				server.Dispose();
			}

			[Fact]
			public void AllowsOtherExceptionsThroughFromStoppingListener()
			{
				var injector = new MockInjector();
				var listener = injector.SetupAndGetListener();
				var server = injector.Create<Server>();

				listener.Setup(l => l.Stop()).Throws<ArgumentException>();

				server.Invoking(s => s.Dispose()).ShouldThrow<ArgumentException>();
			}
		}

		public class Receive
		{
			[Fact]
			public void OnceDefault()
			{
				var injector = new MockInjector();
				var listener = injector.SetupAndGetListener();
				var server = injector.Create<Server>();
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
				var injector = new MockInjector();
				var listener = injector.SetupAndGetListener();
				var server = injector.Create<Server>();
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
				var injector = new MockInjector();
				var listener = injector.SetupAndGetListener();
				var server = injector.Create<Server>();
				var request = new Mock<IHttpListenerRequest>();

				listener.Setup(l => l.GetContextAsync().Result.Request).Returns(request.Object);

				server.Receive();

				server.Invoking(s => s.Receive())
				      .ShouldThrow<InvalidOperationException>();
			}

			[Fact]
			public void TwiceWhenPreviousResponseAborted()
			{
				var injector = new MockInjector();
				var listener = injector.SetupAndGetListener();
				var server = injector.Create<Server>();
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
				var injector = new MockInjector();
				var listener = injector.SetupAndGetListener();
				var server = injector.Create<Server>();
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
				var injector = new MockInjector();
				var listener = injector.SetupAndGetListener();
				var server = injector.Create<Server>();
				var request = new Mock<IHttpListenerRequest>();
				var response = new Mock<IHttpListenerResponse>();

				listener.Setup(l => l.GetContextAsync().Result.Request).Returns(request.Object);
				listener.Setup(l => l.GetContextAsync().Result.Response).Returns(response.Object);

				var called = false;
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
				var injector = new MockInjector();
				var listener = injector.SetupAndGetListener();
				var server = injector.Create<Server>();
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
				var injector = new MockInjector();
				var listener = injector.SetupAndGetListener();
				var server = injector.Create<Server>();
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
				var injector = new MockInjector();
				var listener = injector.SetupAndGetListener();
				var server = injector.Create<Server>();
				var request = new Mock<IHttpListenerRequest>();

				listener.Setup(l => l.GetContextAsync().Result.Request).Returns(request.Object);

				server.Receive();
				server.Invoking(s => s.Respond("xml")).ShouldThrow<NotImplementedException>();
			}

			[Fact]
			public void RespondWhenNoRequest()
			{
				var injector = new MockInjector();
				var listener = injector.SetupAndGetListener();
				var server = injector.Create<Server>();

				server.Invoking(s => s.Respond(r => r.Abort()))
				      .ShouldThrow<InvalidOperationException>()
				      .WithMessage("no request", ComparisonMode.Substring);
			}

			[Fact]
			public void RespondOnlyWorksOnce()
			{
				var injector = new MockInjector();
				var listener = injector.SetupAndGetListener();
				var server = injector.Create<Server>();
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
	}

	public static class ServerTestExtensions
	{
		public static Mock<IHttpListener> SetupAndGetListener(this MockInjector injector)
		{
			var result = injector.GetMock<IHttpListener>();
			result.Setup(l => l.Prefixes).Returns(new List<string>());
			return result;
		}
	}
}