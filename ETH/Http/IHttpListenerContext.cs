namespace ETH.Http
{
	// Interface for System.Net.HttpListenerContext
	public interface IHttpListenerContext
	{
		IHttpListenerRequest Request { get; }
		IHttpListenerResponse Response { get; }
	}
}