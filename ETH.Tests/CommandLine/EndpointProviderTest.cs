using System.ServiceModel.Channels;
using ETH.CommandLine;
using Xunit;
using FluentAssertions;

namespace ETH.Tests.CommandLine
{
	public class EndpointProviderTest
	{
		public class Constructor
		{
			[Fact]
			public void DefaultMessageVersion()
			{
				var provider = new EndpointProvider();
				provider.MessageVersion.Should().Be(MessageVersion.Soap12WSAddressingAugust2004);
			}
		}
	}
}