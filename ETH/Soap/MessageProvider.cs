using Autofac;
using ETH.CommandLine;
using System.IO;
using System.ServiceModel.Channels;
using System.Xml;
using Utility.Logging;

namespace ETH.Soap
{
	public interface IMessageProvider
	{
		Message GetMessage(Stream stream);
	}

	public class MessageProvider : IMessageProvider
	{
		readonly IXmlDocumentProvider xmlDocumentProvider;
		readonly ILogger logger;

		public MessageProvider(IXmlDocumentProvider xmlDocumentProvider,
			ILogger logger)
		{
			this.xmlDocumentProvider = xmlDocumentProvider;
			this.logger = logger;
		}

		public Message GetMessage(Stream stream)
		{
			var requestDocument = xmlDocumentProvider.GetRequestDocument(stream);

			var endpointProvider = Program.Container.Resolve<IEndpointProvider>();

			var message = Message.CreateMessage(
				new XmlNodeReader(requestDocument),
				int.MaxValue,
				endpointProvider.MessageVersion);

			logger.Info("Decoded into SOAP Message successfully.");

			return message;
		}
	}
}
