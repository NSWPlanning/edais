using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ETH.Util
{
	public static class Deserialization
	{
		public static T ConvertNode<T>(this XmlNode node) where T : class
		{
			XmlSerializer ser = new XmlSerializer(typeof(T));
			T result = (ser.Deserialize(new StringReader(node.OuterXml)) as T);
			return result;
		}
	}
}
