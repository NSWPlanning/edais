using System.IO;

namespace ETH.Http
{
	public interface IHttpWebResponse
	{
		Stream GetResponseStream();
	}
}