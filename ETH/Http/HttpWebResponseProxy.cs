using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
			return GetResponseStream().ReadFully().FromUtf8Bytes().Dump();
		}
	}
}
