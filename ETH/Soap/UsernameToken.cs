using System;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using FluentAssertions;

namespace ETH.Soap
{
	[XmlType(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd")]
	public class UsernameToken
	{		
		public string Username { get; set; }
		public Password Password { get; set; }
		public Nonce Nonce { get; set; }
		[XmlElement(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
		public string Created { get; set; }

		public void SetPassword(string password, DateTime created, byte[] nonce)
		{
			if (Password == null)
			{
				Password = new Password();
			}

			if (Nonce == null)
			{
				Nonce = new Nonce();
			}

			Created = created.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
			Nonce.Value = Convert.ToBase64String(nonce);
			Password.Value = ComputePasswordDigest(password, nonce, Created);
		}

		static string ComputePasswordDigest(string secret, byte[] nonceInBytes, string created)
		{
			var createdBytes = Encoding.UTF8.GetBytes(created);
			var secretBytes = Encoding.UTF8.GetBytes(secret);
			var outputBytes = new byte[nonceInBytes.Length + createdBytes.Length + secretBytes.Length];
			Array.Copy(nonceInBytes, outputBytes, nonceInBytes.Length);
			Array.Copy(createdBytes, 0, outputBytes, nonceInBytes.Length, createdBytes.Length);
			Array.Copy(secretBytes, 0, outputBytes, (nonceInBytes.Length + createdBytes.Length), secretBytes.Length);
			return Convert.ToBase64String(SHA1.Create().ComputeHash(outputBytes));
		}

		public void Verify(string username, string password)
		{
			Username.Should().NotBeNullOrEmpty();
			Password.Should().NotBeNull();
			Password.Value.Should().NotBeNullOrEmpty();
			Nonce.Should().NotBeNull();
			Nonce.Value.Should().NotBeNullOrEmpty();
			Created.Should().NotBeNullOrEmpty();

			var digest = ComputePasswordDigest(
				password,
				Convert.FromBase64String(Nonce.Value),
				Created);

			Username.Should().Be(username);
			Password.Value.Should().Be(digest);
		}
	}
}