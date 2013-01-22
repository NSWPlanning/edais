using System.Xml.Serialization;

namespace ETH.Soap
{
	[XmlType(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd")]
	public class Password
	{
		public Password()
		{
			//Type = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordDigest";
			Type = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText";
		}

		[XmlAttribute]
		public string Type { get; set; }
		[XmlText]
		public string Value { get; set; }
	}
}