using System.Collections.Specialized;
using System.IO;

namespace ETH.Http
{
	// Interface for System.Net.HttpListenerRequest
	public interface IHttpListenerRequest
	{
		string ContentType { get; }
		Stream InputStream { get; }
		NameValueCollection Headers { get; }
	}
}