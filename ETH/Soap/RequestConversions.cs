using System.Net;
using System.ServiceModel.Channels;
using ETH.Http;

namespace ETH.Soap
{
	public static class RequestConversions
	{
		// TODO: refactor with SoapDecoder class

		public static string ToXml(this Message message)
		{
			return message.GetReaderAtBodyContents().ReadOuterXml();
		}

		public static string ToXml(this IHttpListenerRequest request)
		{
			return request.ToMessage().ToXml();
		}

		public static string ToXml(this HttpWebResponse response)
		{
			return response.ToMessage().ToXml();
		}
		public static T ToData<T>(this HttpWebResponse response)
		{
			return response.ToMessage().ToData<T>();
		}
		
		public static T ToData<T>(this IHttpListenerRequest request)
		{
			return request.ToMessage().ToData<T>();
		}
	}
}