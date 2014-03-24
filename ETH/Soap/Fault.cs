using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ETH.Soap
{
	[Serializable]
	[XmlRoot("Fault", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
	[XmlType("Fault", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
	[DataContract(Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
	public class Fault
	{
		[DataMember(IsRequired = true, Name = "faultcode")]
		public string FaultCode { get; set; }

		[DataMember(IsRequired = true, Name = "faultstring")]
		public string FaultString { get; set; }
	}
}