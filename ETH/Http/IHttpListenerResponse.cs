using System.IO;

namespace ETH.Http
{
	// Interface for System.Net.HttpListenerResponse
	public interface IHttpListenerResponse
	{
		void Abort();
		string ContentType { get; set; }
		Stream OutputStream { get; }
		void Send();
	}
}