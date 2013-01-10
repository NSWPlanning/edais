using System;
using System.Net;
using System.ServiceModel.Channels;
using ETH.CommandLine;
using ETH.Soap;

namespace ETH.Http
{
	public interface IClient
	{		
		HttpWebResponse Send(Message message);
		HttpWebResponse Send(string action, object data);
		HttpWebResponse Send(string xml);
		HttpWebResponse Send(Action<HttpWebRequest> modifyRequest);
	}

	public class Client : IClient
	{
		const string contentType = "application/soap+xml";

		readonly IEndpointProvider endpointProvider;

		public Client(IEndpointProvider endpointProvider)
		{
			this.endpointProvider = endpointProvider;
		}

		public HttpWebResponse Send(Message message)
		{
			return Send(r => r.FromMessage(message));
		}

		public HttpWebResponse Send(string action, object data)
		{
			return Send(r => r.FromData(action, data));
		}

		public HttpWebResponse Send(string xml)
		{
			throw new NotImplementedException();
		}

		public HttpWebResponse Send(Action<HttpWebRequest> modifyRequest)
		{
			var request = WebRequest.CreateHttp(endpointProvider.ClientEndpoint);
			modifyRequest(request);
			return (HttpWebResponse) request.GetResponse();
		}
	}
}