using System;
using System.Net;
using System.ServiceModel.Channels;
using ETH.CommandLine;
using ETH.Soap;
using ImpromptuInterface;

namespace ETH.Http
{
	public interface IServer
	{
		IHttpListenerRequest Receive();
		void Respond(Message message);
		void Respond(string action, object data);
		void Respond(string xml);
		void Respond(Action<IHttpListenerResponse> modifyResponse);
		void AbortResponse();
	}

	class Server : IServer, IDisposable
	{
		// TODO: implement auth
		readonly IHttpListener listener;
		IHttpListenerContext currentContext;
		bool isDisposed;

		public Server(IHttpListener listener, IEndpointProvider endpointProvider)
		{
			this.listener = listener;	
			listener.Prefixes.Add(endpointProvider.ServerBaseUrl);
		}

		~Server()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (!isDisposed)
			{
				try
				{
					listener.Stop();
				}
				catch (InvalidOperationException)
				{
					// mono throws here - ImpromptuInterface bug?
				}
			}

			isDisposed = true;
		}

		public IHttpListenerRequest Receive()
		{
			if (currentContext != null)
			{
				throw new InvalidOperationException("The previous request hasn't been responded to.");
			}

			if (!listener.IsListening)
			{
				listener.Start();
			}

			currentContext = listener.GetContextAsync().Result;
			return currentContext.Request;
		}

		public void Respond(Message message)
		{
			Respond(r => r.FromMessage(message));
		}

		public void Respond(string action, object data)
		{
			Respond(r => r.FromData(action, data));
		}

		public void Respond(string xml)
		{
			Respond(r => r.FromXml(xml));
		}

		public void Respond(Action<IHttpListenerResponse> modifyResponse)
		{
			if (currentContext == null)
			{
				throw new InvalidOperationException("There's no request to respond to.");
			}

			modifyResponse(currentContext.Response);
			currentContext = null;
		}

		public void AbortResponse()
		{
			Respond(r => r.Abort());
		}
	}
}
