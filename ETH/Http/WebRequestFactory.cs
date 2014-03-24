using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using ImpromptuInterface;

namespace ETH.Http
{
	public interface IWebRequestFactory
	{
		IHttpWebRequest CreateHttp(string requestUriString);
		IHttpListenerRequest WithLoggingProxy(IHttpListenerRequest request);
		IHttpListenerResponse WithLoggingProxy(IHttpListenerResponse response);
		IHttpWebRequest WithLoggingProxy(IHttpWebRequest request);
		IHttpWebResponse WithLoggingProxy(IHttpWebResponse response);
	}

	public class WebRequestFactory : IWebRequestFactory
	{
		public IHttpWebRequest CreateHttp(string requestUriString)
		{
			var httpWebRequest = WebRequest.CreateHttp(requestUriString);
			httpWebRequest.ServerCertificateValidationCallback += ServerCertificateValidationCallback;
			return WithLoggingProxy(httpWebRequest.ActLike<IHttpWebRequest>());
		}

		bool ServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		public IHttpListenerRequest WithLoggingProxy(IHttpListenerRequest request)
		{
			return new HttpListenerRequestProxy(request);
		}

		public IHttpListenerResponse WithLoggingProxy(IHttpListenerResponse response)
		{
			return new HttpListenerResponseProxy(response);
		}

		public IHttpWebRequest WithLoggingProxy(IHttpWebRequest request)
		{
			return new HttpWebRequestProxy(request);
		}

		public IHttpWebResponse WithLoggingProxy(IHttpWebResponse response)
		{
			return new HttpWebResponseProxy(response);
		}
	}
}