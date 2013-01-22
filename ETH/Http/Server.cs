using System;
using System.Net;
using System.ServiceModel.Channels;
using ETH.CommandLine;
using ETH.OutputModels;
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
		readonly IEndpointProvider endpointProvider;
		readonly IOutput output;
		readonly ISoapDecoder soapDecoder;
		IHttpListenerContext currentContext;
		bool isDisposed;

		public Server(
			IHttpListener listener,
			IEndpointProvider endpointProvider,
			IOutput output,
			ISoapDecoder soapDecoder)
		{
			this.listener = listener;
			this.endpointProvider = endpointProvider;
			this.output = output;
			this.soapDecoder = soapDecoder;
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

			var listenerContextTask = listener.GetContextAsync();

			output.Display(new StatusModel
			{

				Endpoint = endpointProvider.ServerBaseUrl
			});

			currentContext = listenerContextTask.Result;
			return currentContext.Request;
		}

		public void Respond(Message message)
		{
			Respond(r => soapDecoder.FromMessage(r, message));
		}

		public void Respond(string action, object data)
		{
			Respond(r => soapDecoder.FromData(r, action, data));
		}

		public void Respond(string xml)
		{
			Respond(r => soapDecoder.FromXml(r, xml));
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
