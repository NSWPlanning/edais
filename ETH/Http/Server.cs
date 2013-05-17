using System;
using System.Collections.Specialized;
using System.Dynamic;
using System.IO;
using System.Net;
using System.ServiceModel.Channels;
using ETH.CommandLine;
using ETH.OutputModels;
using ETH.Soap;
using ImpromptuInterface;
using ServiceStack.Text;
using Utility.Logging;
using System.Linq;

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
		readonly ILogger logger;
		IHttpListenerContext currentContext;
		bool isDisposed;

		public Server(
			IHttpListener listener,
			IEndpointProvider endpointProvider,
			IOutput output,
			ISoapDecoder soapDecoder,
			ILogger logger)
		{
			this.listener = listener;
			this.endpointProvider = endpointProvider;
			this.output = output;
			this.soapDecoder = soapDecoder;
			this.logger = logger;
			listener.Prefixes.Add(endpointProvider.ServerBaseUrl);
			logger.Info("Listening on: {0}", endpointProvider.ServerBaseUrl);
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
			var request = new HttpListenerRequestProxy(currentContext.Request);			
			logger.Info("Request Headers: {0}", request.ToString());
			return request;
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

			var response = new HttpListenerResponseProxy(currentContext.Response);
			modifyResponse(response);
			logger.Info("Response: {0}", response.ToString());
			response.Send();
			currentContext = null;
		}

		public void AbortResponse()
		{
			Respond(r => r.Abort());
		}
	}
}
