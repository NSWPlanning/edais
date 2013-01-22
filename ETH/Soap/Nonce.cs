using System.Xml.Serialization;

namespace ETH.Soap
{
	[XmlType(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd")]
	public class Nonce
	{
		public Nonce()
		{
			EncodingType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary";
		}

		[XmlAttribute]
		public string EncodingType { get; set; }
		[XmlText]
		public string Value { get; set; }
	}
}