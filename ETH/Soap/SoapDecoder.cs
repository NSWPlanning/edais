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
using FluentAssertions;

namespace ETH.Soap
{
	public interface ISoapDecoder
	{
		/// <summary>
		/// Converts a Nancy Request to a .NET SOAP Message.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		Message ToMessage(IHttpListenerRequest request);

		Message ToMessage(IHttpWebResponse response);

		/// <summary>
		/// Extract object from Message body, using XmlSerializer.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="message"></param>
		/// <returns></returns>
		T ToData<T>(Message message);

		/// <summary>
		/// Extract object from Message body, using XmlSerializer.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		object ToData(Message message, Type type);

		/// <summary>
		/// Respond with a SOAP message from data and an action.
		/// </summary>
		/// <param name="response"></param>
		/// <param name="action"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		void FromData(IHttpListenerResponse response, string action, object data);

		void FromData(IHttpWebRequest request, string action, object data);
		Message ToMessage(string action, object data);

		/// <summary>
		/// Respond with a SOAP message, from a Message.
		/// </summary>
		/// <param name="response"></param>
		/// <param name="message"></param>
		void FromMessage(IHttpListenerResponse response, Message message);

		void FromMessage(IHttpWebRequest request, Message message);
		void FromXml(IHttpListenerResponse response, string xml);
		string ToXml(Message message);
		string ToXml(IHttpListenerRequest request);
		string ToXml(IHttpWebResponse response);
		T ToData<T>(IHttpWebResponse response);
		T ToData<T>(IHttpListenerRequest request);
		void VerifySecurity(Message message);
	}

	/// <summary>
	/// Various SOAP protocol encoding/decoding helpers.
	/// </summary>
	public class SoapDecoder : ISoapDecoder
	{
		readonly IEndpointProvider endpointProvider;

		public SoapDecoder(IEndpointProvider endpointProvider)
		{
			this.endpointProvider = endpointProvider;
		}

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
		public Message ToMessage(IHttpListenerRequest request)
		{
			var contentType = GetContentType(request.ContentType);
			var message = GetMessage(request.InputStream);
			SetAction(message, contentType, request.Headers);
			VerifySecurity(message);
			return message;
		}

		public Message ToMessage(IHttpWebResponse response)
		{
			return GetMessage(response.GetResponseStream());
		}

		/// <summary>
		/// Extract object from Message body, using XmlSerializer.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="message"></param>
		/// <returns></returns>
		public T ToData<T>(Message message)
		{
			return (T) ToData(message, typeof (T));
		}

		/// <summary>
		/// Extract object from Message body, using XmlSerializer.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public object ToData(Message message, Type type)
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
		public void FromData(IHttpListenerResponse response, string action, object data)
		{
			FromMessage(response, ToMessage(action, data));
		}

		public void FromData(IHttpWebRequest request, string action, object data)
		{
			FromMessage(request, ToMessage(action, data));
		}

		public Message ToMessage(string action, object data)
		{
			var stream = new MemoryStream(); // Message holds onto this

			var serializer = new XmlSerializer(data.GetType());
			serializer.Serialize(stream, data);

			stream.Seek(0, SeekOrigin.Begin);

			var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas());

			return Message.CreateMessage(
				endpointProvider.MessageVersion,
				action,
				reader);
		}

		/// <summary>
		/// Respond with a SOAP message, from a Message.
		/// </summary>
		/// <param name="response"></param>
		/// <param name="message"></param>
		public void FromMessage(IHttpListenerResponse response, Message message)
		{
			response.ContentType = SoapContentType;

			using (var writer = XmlDictionaryWriter.CreateTextWriter(response.OutputStream))
			{
				message.WriteMessage(writer);
				writer.Flush();
				response.OutputStream.Close();
			}
		}

		public void FromMessage(IHttpWebRequest request, Message message)
		{
			request.ContentType = message.Version == MessageVersion.Soap11 ? "text/xml" : SoapContentType;
			request.Method = "POST";
			request.Headers.Add("SOAPAction", message.Headers.Action);
			message.Headers.Action = null;

			using (var stream = request.GetRequestStream())
			using (var writer = XmlDictionaryWriter.CreateTextWriter(stream))
			{
				message.WriteMessage(writer);
				writer.Flush();
			}
		}

		public void FromXml(IHttpListenerResponse response, string xml)
		{
			throw new NotImplementedException();
		}

		public string ToXml(Message message)
		{
			return message.GetReaderAtBodyContents().ReadOuterXml();
		}

		public string ToXml(IHttpListenerRequest request)
		{
			return ToXml(ToMessage(request));
		}

		public string ToXml(IHttpWebResponse response)
		{
			return ToXml(ToMessage(response));
		}
		public T ToData<T>(IHttpWebResponse response)
		{
			return ToData<T>(ToMessage(response));
		}

		public T ToData<T>(IHttpListenerRequest request)
		{
			return ToData<T>(ToMessage(request));
		}

		public class NamespaceIgnorantXmlTextReader : XmlTextReader
		{
			public NamespaceIgnorantXmlTextReader(TextReader reader) : base(reader) { }

			public override string NamespaceURI
			{
				get { return ""; }
			}
		}

		public void VerifySecurity(Message message)
		{
			if (endpointProvider.SkipAuthentication)
				return;

			var headerIndex = message.Headers.FindHeader(
				"Security", 
				"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
			var header = message.Headers[headerIndex];
			var headerString = header.ToString();

			var xmlSerializer = new XmlSerializer(typeof (Security));
			var security = (Security)xmlSerializer.Deserialize(new StringReader(headerString));

			security.UsernameToken.Verify(endpointProvider.Username, endpointProvider.Password);
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

			var headerAction = headers["SOAPAction"];
			if (contentType.Parameters != null && contentType.Parameters.ContainsKey("action"))
			{
				message.Headers.Action = contentType.Parameters["action"];
			}
			else if (headerAction != null)
			{
				message.Headers.Action = headerAction;
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