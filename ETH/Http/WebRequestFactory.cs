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
			return WebRequest.CreateHttp(requestUriString).ActLike<IHttpWebRequest>();
		}
	}
}