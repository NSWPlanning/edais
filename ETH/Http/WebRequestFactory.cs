using System.Net;
using ImpromptuInterface;

namespace ETH.Http
{
	public interface IWebRequestFactory
	{
		IHttpWebRequest CreateHttp(string requestUriString);
	}

	public class WebRequestFactory : IWebRequestFactory
	{
		public IHttpWebRequest CreateHttp(string requestUriString)
		{
			var request = new HttpWebRequestWrapper(WebRequest.CreateHttp(requestUriString).ActLike<IHttpWebRequest>());
			return request;
		}
	}
}