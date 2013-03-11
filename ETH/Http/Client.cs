using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.ServiceModel.Channels;
using ETH.CommandLine;
using ETH.Soap;
using ImpromptuInterface;
using Utility.Logging;
using ServiceStack.Text;
using System.Linq;

namespace ETH.Http
{
	public interface IClient
	{
		IHttpWebResponse Send(Message message);
		IHttpWebResponse Send(string action, object data);
		IHttpWebResponse Send(string xml);
		IHttpWebResponse Send(Action<IHttpWebRequest> modifyRequest);
	}

	public class Client : IClient
	{
		const string ContentType = "application/soap+xml";

		readonly IEndpointProvider endpointProvider;
		readonly ISoapDecoder soapDecoder;
		readonly IWebRequestFactory webRequestFactory;
		readonly ISecurityHeaderFactory securityHeaderFactory;
		readonly ILogger logger;

		public Client(
			IEndpointProvider endpointProvider, 
			ISoapDecoder soapDecoder,
			IWebRequestFactory webRequestFactory,
			ISecurityHeaderFactory securityHeaderFactory,
			ILogger logger)
		{
			this.endpointProvider = endpointProvider;
			this.soapDecoder = soapDecoder;
			this.webRequestFactory = webRequestFactory;
			this.securityHeaderFactory = securityHeaderFactory;
			this.logger = logger;
		}

		public IHttpWebResponse Send(Message message)
		{
			return Send(r =>
			{
				var securityHeader = securityHeaderFactory.Create();
				message.Headers.Add(securityHeader);

				r.ContentType = ContentType;
				soapDecoder.FromMessage(r, message);
			});
		}

		public IHttpWebResponse Send(string action, object data)
		{
			return Send(soapDecoder.ToMessage(action, data));
		}

		public IHttpWebResponse Send(string xml)
		{
			throw new NotImplementedException();
		}

		public IHttpWebResponse Send(Action<IHttpWebRequest> modifyRequest)
		{
			IHttpWebResponse response;
			try
			{
				var requestUriString = endpointProvider.ClientEndpoint.Dequeue();
				var request =
					new HttpWebRequestWrapper(webRequestFactory.CreateHttp(requestUriString));
				modifyRequest(request);
				logger.Info("Request: {0}", request.ToString());
				response = request.GetResponse();
				logger.Info("Response: {0}", response.ToString());
			}
			catch (WebException ex)
			{
				logger.Error(ex, "Response error.");
				response = new HttpWebResponseWrapper(ex.Response.ActLike<IHttpWebResponse>());
				logger.Info("Response: {0}", response.ToString());
			}
			return response;
		}

		class HttpWebRequestWrapper : IHttpWebRequest
		{
			readonly IHttpWebRequest request;
			readonly MemoryStream stream = new MemoryStream();

			public HttpWebRequestWrapper(IHttpWebRequest request)
			{
				this.request = request;
			}

			public IHttpWebResponse GetResponse()
			{
				return new HttpWebResponseWrapper(request.GetResponse());
			}

			public NameValueCollection Headers
			{
				get { return request.Headers; }
			}

			public string ContentType
			{
				get { return request.ContentType; }
				set { request.ContentType = value; }
			}

			public string Method
			{
				get { return request.Method; }
				set { request.Method = value; }
			}

			public Stream GetRequestStream()
			{
				return stream;
			}

			public void Send()
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.CopyTo(request.GetRequestStream());
			}

			public override string ToString()
			{
				stream.Seek(0, SeekOrigin.Begin);
				return new
				{
					Headers = request.Headers.AllKeys
						.Select(k =>
							new
							{
								Name = k,
								Values = request.Headers.GetValues(k)
							}),
					Contents = stream.ReadFully().FromUtf8Bytes()
				}.Dump();
			}
		}

		class HttpWebResponseWrapper : IHttpWebResponse
		{
			readonly IHttpWebResponse response;
			MemoryStream stream;

			public HttpWebResponseWrapper(IHttpWebResponse response)
			{
				this.response = response;
			}

			public Stream GetResponseStream()
			{
				if (stream == null)
				{
					stream = new MemoryStream();
					response.GetResponseStream().CopyTo(stream);
				}
				stream.Seek(0, SeekOrigin.Begin);
				return stream;
			}

			public override string ToString()
			{
				return GetResponseStream().ReadFully().FromUtf8Bytes().Dump();
			}
		}
	}
}