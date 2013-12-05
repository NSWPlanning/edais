using ServiceStack.Text;
using System.IO;
using System.Xml;

namespace ETH.Soap
{
	public interface IXmlDocumentProvider
	{
		XmlDocument GetRequestDocument(Stream stream);
	}

	public class XmlDocumentProvider : IXmlDocumentProvider
	{
		public XmlDocument GetRequestDocument(Stream stream)
		{
			var requestXml = stream.ReadFully().FromUtf8Bytes();
			var requestDocument = new XmlDocument();
			requestDocument.LoadXml(requestXml);
			return requestDocument;
		}
	}
}
