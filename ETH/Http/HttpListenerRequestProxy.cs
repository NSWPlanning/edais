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
	public class HttpListenerRequestProxy : IHttpListenerRequest
	{
		readonly IHttpListenerRequest request;
		MemoryStream stream;

		public HttpListenerRequestProxy(IHttpListenerRequest request)
		{
			this.request = request;
		}

		public string ContentType
		{
			get { return request.ContentType; }
		}

		public Stream InputStream
		{
			get
			{
				if (stream == null)
				{
					stream = new MemoryStream();
					request.InputStream.CopyTo(stream);
				}
				stream.Seek(0, SeekOrigin.Begin);
				return stream;
			}
		}

		public NameValueCollection Headers
		{
			get { return request.Headers; }
		}

		public override string ToString()
		{
			return new
			{
				Headers = request.Headers.AllKeys
					.Select(k =>
						new
						{
							Name = k,
							Values = request.Headers.GetValues(k)
						}),
				Content = InputStream.ReadFully().FromUtf8Bytes()
			}.Dump();
		}
	}
}
