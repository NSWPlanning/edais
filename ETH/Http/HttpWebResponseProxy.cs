using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Text;

namespace ETH.Http
{
	public class HttpWebResponseProxy : IHttpWebResponse
	{
		readonly IHttpWebResponse response;
		MemoryStream stream;

		public HttpWebResponseProxy(IHttpWebResponse response)
		{
			this.response = response;
		}

		public HttpStatusCode StatusCode { get { return response.StatusCode; }  }

		public string StatusDescription { get { return response.StatusDescription; } }

		public Stream GetResponseStream()
		{
			if (stream == null)
			{
				stream = new MemoryStream();
				response.GetResponseStream().CopyTo(stream);
			}
			stream.Seek(0, SeekOrigin.Begin);
			return stream;
		}

		public override string ToString()
		{
			return new
			{
				Content = GetResponseStream().ReadFully().FromUtf8Bytes(),
				StatusCode = StatusCode,
				StatusDescription = StatusDescription
			}.Dump();
		}
	}
}
