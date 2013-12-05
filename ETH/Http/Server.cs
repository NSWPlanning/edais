using System;
using System.Collections.Specialized;
using System.Dynamic;
using System.IO;
using System.Net;
using System.ServiceModel.Channels;
using ETH.CommandLine;
using ETH.OutputModels;
using ETH.Soap;
using ETH.Util;
using ImpromptuInterface;
using ServiceStack.Text;
using Utility.Logging;
using System.Linq;

namespace ETH.Http
{
	public interface IServer
	{
		IHttpListenerRequest Receive();
		void Respond(Message message, IHttpListenerRequest request);
		void Respond(string action, object data, IHttpListenerRequest request);
		void Respond(string xml);
		void Respond(Action<IHttpListenerResponse> modifyResponse);
		void AbortResponse();
	}

	class Server : IServer, IDisposable
	{
		readonly IHttpListener listener;
		readonly IEndpointProvider endpointProvider;
		readonly IOutput output;
		readonly ISoapDecoder soapDecoder;
		readonly ILogger logger;
		readonly IContentTypeProvider contentTypeProvider;
		readonly IWebRequestFactory webRequestFactory;
		IHttpListenerContext currentContext;
		bool isDisposed;

		public Server(
			IHttpListener listener,
			IEndpointProvider endpointProvider,
			IOutput output,
			ISoapDecoder soapDecoder,
			ILogger logger,
			IContentTypeProvider contentTypeProvider,
			IWebRequestFactory webRequestFactory)
		{
			this.listener = listener;
			this.endpointProvider = endpointProvider;
			this.output = output;
			this.soapDecoder = soapDecoder;
			this.logger = logger;
			this.contentTypeProvider = contentTypeProvider;
			this.webRequestFactory = webRequestFactory;
			try
			{
				listener.Prefixes.Add(endpointProvider.ServerBaseUrl);
			}
			catch (ArgumentException ex)
			{
				throw new FailException(
					string.Format(
						"While attempting to listen on URL '{1}': {0}",
						ex.Message,
						endpointProvider.ServerBaseUrl),
					ex);
			}
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
				try
				{
					listener.Start();
				}
				catch (HttpListenerException ex)
				{
					throw new FailException(
						string.Format(
							"While attempting to start listening for requests: {0}",
							ex.Message), 
						ex);
				}
			}

			var listenerContextTask = listener.GetContextAsync();

			output.Display(new StatusModel
			{

				Endpoint = endpointProvider.ServerBaseUrl
			});

			currentContext = listenerContextTask.Result;
			var request = webRequestFactory.WithLoggingProxy(currentContext.Request);			
			logger.Info("Request Headers: {0}", request.ToString());
			return request;
		}

		public void Respond(Message message, IHttpListenerRequest request)
		{
			Respond(r =>
			{
				r.ContentType = contentTypeProvider.ContentType(message);
				soapDecoder.FromMessage(r, message, request);
			});
		}

		public void Respond(string action, object data, IHttpListenerRequest request)
		{
			Respond(r => soapDecoder.FromData(r, action, data, request));
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

			var response = webRequestFactory.WithLoggingProxy(currentContext.Response);
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
