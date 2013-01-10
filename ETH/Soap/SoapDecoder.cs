using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.ServiceModel.Channels;
using System.Xml;
using System.Xml.Serialization;
using ETH.CommandLine;
using ETH.Http;
using Autofac;

namespace ETH.Soap
{
	/// <summary>
	/// Various SOAP protocol encoding/decoding helpers.
	/// </summary>
	public static class SoapDecoder
	{
		// TODO: refactor with RequestConversions class

		const string SoapContentType = "application/soap+xml";

		static readonly string[] ValidContentTypes = new[]
		{
			SoapContentType,
			"text/xml"
		};
		

		/// <summary>
		/// Converts a Nancy Request to a .NET SOAP Message.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public static Message ToMessage(this IHttpListenerRequest request)
		{
			var contentType = GetContentType(request.ContentType);
			var message = GetMessage(request.InputStream);
			SetAction(message, contentType, request.Headers);
			return message;
		}

		public static Message ToMessage(this HttpWebResponse response)
		{
			var contentType = GetContentType(response.ContentType);
			var message = GetMessage(response.GetResponseStream());
			SetAction(message, contentType, response.Headers);
			return message;
		}

		/// <summary>
		/// Extract object from Message body, using XmlSerializer.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="message"></param>
		/// <returns></returns>
		public static T ToData<T>(this Message message)
		{
			return (T) message.ToData(typeof (T));
		}

		/// <summary>
		/// Extract object from Message body, using XmlSerializer.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static object ToData(this Message message, Type type)
		{
			using (var reader = message.GetReaderAtBodyContents())
			{
				var serializer = new XmlSerializer(type);
				return serializer.Deserialize(reader);
			}
		}

		/// <summary>
		/// Respond with a SOAP message from data and an action.
		/// </summary>
		/// <param name="response"></param>
		/// <param name="action"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static void FromData(this IHttpListenerResponse response, string action, object data)
		{			
			response.FromMessage(ToMessage(action, data));
		}

		public static void FromData(this HttpWebRequest request, string action, object data)
		{
			request.FromMessage(ToMessage(action, data));
		}

		public static Message ToMessage(string action, object data)
		{
			var stream = new MemoryStream(); // Message holds onto this

			var serializer = new XmlSerializer(data.GetType());
			serializer.Serialize(stream, data);

			stream.Seek(0, SeekOrigin.Begin);

			var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas());

			return Message.CreateMessage(
				MessageVersion.Soap12WSAddressingAugust2004,
				action,
				reader);			
		}

		/// <summary>
		/// Respond with a SOAP message, from a Message.
		/// </summary>
		/// <param name="response"></param>
		/// <param name="message"></param>
		public static void FromMessage(this IHttpListenerResponse response, Message message)
		{
			response.ContentType = SoapContentType;

			using (var writer = XmlDictionaryWriter.CreateTextWriter(response.OutputStream))
			{
				message.WriteMessage(writer);
				writer.Flush();
				response.OutputStream.Close();
			}
		}

		public static void FromMessage(this HttpWebRequest request, Message message)
		{
			// TODO: refactor to share code with other FromMessage
			request.ContentType = SoapContentType;

			using (var stream = request.GetRequestStream())
			using (var writer = XmlDictionaryWriter.CreateTextWriter(stream))
			{
				message.WriteMessage(writer);
				writer.Flush();
			}
		}

		public static void FromXml(this IHttpListenerResponse response, string xml)
		{
			throw new NotImplementedException();
		}

		/// <exception cref="InvalidOperationException">Request is not a SOAP request.</exception>
		static ContentType GetContentType(string rawContentType)
		{
			if (string.IsNullOrWhiteSpace(rawContentType))
			{
				throw new InvalidOperationException("Request has no Content-type!");
			}

			var contentType = new ContentType(rawContentType);
			if (!ValidContentTypes.Contains(contentType.MediaType))
			{
				throw new InvalidOperationException(string.Format(
					"Request has an invalid content type: {0}.", 
					contentType.MediaType));
			}
			return contentType;
		}

		/// <exception cref="InvalidOperationException">SOAP action missing.</exception>
		static void SetAction(Message message, ContentType contentType, NameValueCollection headers)
		{
			if (!string.IsNullOrWhiteSpace(message.Headers.Action))
			{
				return;
			}

			if (contentType.Parameters != null && contentType.Parameters.ContainsKey("action"))
			{
				message.Headers.Action = contentType.Parameters["action"];
			}
			else if (headers.AllKeys.Contains("SOAPAction"))
			{
				message.Headers.Action = headers["SOAPAction"];
			}
			else
			{
				throw new InvalidOperationException("SOAP action missing.");
			}
		}

		static Message GetMessage(Stream stream)
		{
			var requestDocument = GetRequestDocument(stream);

			// TODO: refactor out
			var endpointProvider = Program.Container.Resolve<IEndpointProvider>();

			var message = Message.CreateMessage(
				new XmlNodeReader(requestDocument),
				int.MaxValue,
				endpointProvider.MessageVersion);

			return message;
		}

		static XmlDocument GetRequestDocument(Stream stream)
		{
			var requestXml = GetRequestXml(stream);
			var requestDocument = new XmlDocument();
			requestDocument.LoadXml(requestXml);
			return requestDocument;
		}

		static string GetRequestXml(Stream stream)
		{
			string requestXml;
			using (var reader = new StreamReader(stream))
			{
				requestXml = reader.ReadToEnd();
			}
			return requestXml;
		}
	}
}