using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ETH.Soap
{
	[XmlType(Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd")]
	public class Timestamp
	{
		public string Id { get; set; }
		public string Created { get; set; }
		public string Expires { get; set; }
	}
}
