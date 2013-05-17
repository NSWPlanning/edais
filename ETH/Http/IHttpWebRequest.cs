using System.Collections.Specialized;
using System.IO;

namespace ETH.Http
{
	public interface IHttpWebRequest
	{
		IHttpWebResponse GetResponse();
		NameValueCollection Headers { get; }
		string ContentType { get; set; }
		string Method { get; set; }
		void Send();
		Stream GetRequestStream();
	}
}