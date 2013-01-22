using System;
using System.Xml.Serialization;
using FluentAssertions;

namespace ETH.Soap
{
	[XmlRoot(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd")]
	[XmlType(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd")]
	public class Security
	{
		public Security()
		{			
		}

		public Security(string username, string password, DateTime created, byte[] nonce)
		{
			UsernameToken = new UsernameToken
			{
				Username = username,
				Password = new Password(),
				Nonce = new Nonce()
			};

			UsernameToken.SetPassword(password, created, nonce);
		}

		public UsernameToken UsernameToken { get; set; }

		public void Verify(string username, string password)
		{
			UsernameToken.Should().NotBeNull();
			UsernameToken.Verify(username, password);
		}
	}
}