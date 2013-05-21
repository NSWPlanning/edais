using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Text;

namespace ETH.Http
{
	public class HttpWebRequestProxy : IHttpWebRequest
	{
		readonly IHttpWebRequest request;
		readonly MemoryStream stream = new MemoryStream();

		public HttpWebRequestProxy(IHttpWebRequest request)
		{
			this.request = request;
		}

		public IHttpWebResponse GetResponse()
		{
			return new HttpWebResponseProxy(request.GetResponse());
		}

		public NameValueCollection Headers
		{
			get { return request.Headers; }
		}

		public string ContentType
		{
			get { return request.ContentType; }
			set { request.ContentType = value; }
		}

		public string Method
		{
			get { return request.Method; }
			set { request.Method = value; }
		}

		public Stream GetRequestStream()
		{
			return stream;
		}

		public void Send()
		{
			stream.Seek(0, SeekOrigin.Begin);
			stream.CopyTo(request.GetRequestStream());
		}

		public override string ToString()
		{
			stream.Seek(0, SeekOrigin.Begin);
			return new
			{
				Headers = request.Headers.AllKeys
					.Select(k =>
						new
						{
							Name = k,
							Values = request.Headers.GetValues(k)
						}),
				Contents = stream.ReadFully().FromUtf8Bytes()
			}.Dump();
		}
	}
}
