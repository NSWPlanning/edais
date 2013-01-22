using System;
using System.Net;
using System.ServiceModel.Channels;
using ETH.CommandLine;
using ETH.Soap;
using ImpromptuInterface;

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

		public Client(
			IEndpointProvider endpointProvider, 
			ISoapDecoder soapDecoder,
			IWebRequestFactory webRequestFactory,
			ISecurityHeaderFactory securityHeaderFactory)
		{
			this.endpointProvider = endpointProvider;
			this.soapDecoder = soapDecoder;
			this.webRequestFactory = webRequestFactory;
			this.securityHeaderFactory = securityHeaderFactory;
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
				var request = webRequestFactory.CreateHttp(endpointProvider.ClientEndpoint);
				modifyRequest(request);
				response = request.GetResponse();
			}
			catch (WebException ex)
			{
				response = ex.Response.ActLike<IHttpWebResponse>();
			}
			return response;
		}
	}
}