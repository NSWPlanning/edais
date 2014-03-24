using System.IO;
using System.Net;

namespace ETH.Http
{
	public interface IHttpWebResponse
	{
		HttpStatusCode StatusCode { get; }
		string StatusDescription { get; }
		Stream GetResponseStream();
	}
}