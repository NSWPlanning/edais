using System.Collections.Generic;
using System.ServiceModel.Channels;

namespace ETH.CommandLine
{
	public interface IEndpointProvider
	{
		string ServerBaseUrl { get; set; }
		Queue<string> ClientEndpoint { get; set; }
		MessageVersion MessageVersion { get; set; }
		string Username { get; set; }
		string Password { get; set; }
		bool SkipAuthentication { get; set; }
	}

	class EndpointProvider : IEndpointProvider
	{
		public EndpointProvider()
		{
			MessageVersion = MessageVersion.Soap12WSAddressingAugust2004;
		}

		public string ServerBaseUrl { get; set; }
		public Queue<string> ClientEndpoint { get; set; }
		public MessageVersion MessageVersion { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public bool SkipAuthentication { get; set; }
	}
}