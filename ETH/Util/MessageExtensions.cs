using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace ETH.Util
{
	public interface IContentTypeProvider
	{
		string ContentType(Message message);
		string[] ValidContentTypes { get; }
	}
	public class ContentTypeProvider : IContentTypeProvider
	{
		public string ContentType(Message message)
		{
			return message.Version == MessageVersion.Soap11WSAddressing10 ? "text/xml" : SoapContentType;
		}

		public string[] ValidContentTypes
		{
			get
			{
				return new[]
				{
					SoapContentType,
					"text/xml"
				};
			}
		}
		const string SoapContentType = "application/soap+xml";
	}
}
