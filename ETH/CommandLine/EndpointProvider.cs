using System.ServiceModel.Channels;

namespace ETH.CommandLine
{
	public interface IEndpointProvider
	{
		string ServerBaseUrl { get; set; }
		string ClientEndpoint { get; set; }
		MessageVersion MessageVersion { get; set; }
	}

	class EndpointProvider : IEndpointProvider
	{
		public EndpointProvider()
		{
			MessageVersion = MessageVersion.Soap12WSAddressingAugust2004;
		}

		public string ServerBaseUrl { get; set; }
		public string ClientEndpoint { get; set; }
		public MessageVersion MessageVersion { get; set; }
	}
}