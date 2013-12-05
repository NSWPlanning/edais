using System.Net;
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
			return WithLoggingProxy(WebRequest.CreateHttp(requestUriString).ActLike<IHttpWebRequest>());
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