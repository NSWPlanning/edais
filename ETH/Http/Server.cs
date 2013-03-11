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

		class HttpListenerRequestProxy : IHttpListenerRequest
		{
			readonly IHttpListenerRequest request;
			MemoryStream stream;

			public HttpListenerRequestProxy(IHttpListenerRequest request)
			{
				this.request = request;
			}

			public string ContentType
			{
				get { return request.ContentType; }
			}

			public Stream InputStream
			{
				get
				{
					if (stream == null)
					{
						stream = new MemoryStream();
						request.InputStream.CopyTo(stream);
					}
					stream.Seek(0, SeekOrigin.Begin);
					return stream;
				}
			}

			public NameValueCollection Headers
			{
				get { return request.Headers; }
			}

			public override string ToString()
			{
				return new
				{
					Headers = request.Headers.AllKeys
						.Select(k =>
							new
							{
								Name = k,
								Values = request.Headers.GetValues(k)
							}),
					Content = InputStream.ReadFully().FromUtf8Bytes()
				}.Dump();
			}
		}

		class HttpListenerResponseProxy : IHttpListenerResponse
		{
			readonly IHttpListenerResponse response;
			bool isAborted;
			readonly MemoryStream stream = new MemoryStream();

			public HttpListenerResponseProxy(IHttpListenerResponse response)
			{
				this.response = response;
			}

			public void Abort()
			{
				isAborted = true;
				response.Abort();
			}

			public string ContentType
			{
				get { return response.ContentType; }
				set { response.ContentType = value; }
			}

			public Stream OutputStream
			{
				get { return stream; }
			}

			public void Send()
			{
				if (!isAborted)
				{
					stream.Seek(0, SeekOrigin.Begin);
					stream.CopyTo(response.OutputStream);
					response.OutputStream.Close();
				}
			}

			public override string ToString()
			{
				stream.Seek(0, SeekOrigin.Begin);
				return new
				{
					IsAborted = isAborted,
					ContentType,
					Response = stream.ReadFully().FromUtf8Bytes()
				}.Dump();
			}
		}
	}
}
