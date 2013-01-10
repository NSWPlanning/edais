namespace ETH.CommandLine
{
	public interface IEndpointProvider
	{
		string ServerBaseUrl { get; set; }
		string ClientEndpoint { get; set; }
	}

	class EndpointProvider : IEndpointProvider
	{
		public string ServerBaseUrl { get; set; }
		public string ClientEndpoint { get; set; }
	}
}