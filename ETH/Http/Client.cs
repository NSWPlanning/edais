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
			IHttpWebResponse response = null;
			try
			{
				var requestUriString = endpointProvider.ClientEndpoint.Dequeue();
				var request = webRequestFactory.CreateHttp(requestUriString);
				modifyRequest(request);
				request.Send();
				logger.Info("Request: {0}", request.ToString());
				response = request.GetResponse();
				logger.Info("Response: {0}", response.ToString());
			}
			catch (WebException ex)
			{
				logger.Error(ex, "Response error.");
				if (ex.Response != null)
				{
					response = ex.Response.ActLike<IHttpWebResponse>();
					logger.Info("Response: {0}", new HttpWebResponseWrapper(response).ToString());
				}
			}
			return response;
		}
	}
}