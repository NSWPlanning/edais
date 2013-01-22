using System.IO;

namespace ETH.Http
{
	public interface IHttpWebRequest
	{
		IHttpWebResponse GetResponse();
		string ContentType { get; set; }
		string Method { get; set; }
		Stream GetRequestStream();
	}
}