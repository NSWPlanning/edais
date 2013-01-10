namespace ETH.Http
{
	// Interface for Task<System.Net.HttpListenerContext>
	public interface IHttpListenerContextTask
	{
		IHttpListenerContext Result { get; }
	}
}