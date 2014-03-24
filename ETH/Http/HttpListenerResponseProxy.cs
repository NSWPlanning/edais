using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Text;

namespace ETH.Http
{
	public class HttpListenerResponseProxy : IHttpListenerResponse
	{
		readonly IHttpListenerResponse response;
		bool isAborted;
		readonly MemoryStream stream = new MemoryStream();

		public HttpListenerResponseProxy(IHttpListenerResponse response)
		{
			this.response = response;
		}

		public void Abort()
		{
			isAborted = true;
			response.Abort();
		}

		public string ContentType
		{
			get { return response.ContentType; }
			set { response.ContentType = value; }
		}

		public Stream OutputStream
		{
			get { return stream; }
		}

		public void Send()
		{
			if (!isAborted)
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.CopyTo(response.OutputStream);
				response.OutputStream.Close();
			}
		}

		public override string ToString()
		{			
			stream.Seek(0, SeekOrigin.Begin);
			return new
			{
				IsAborted = isAborted,
				ContentType,
				Response = stream.ReadFully().FromUtf8Bytes()
			}.Dump();
		}
	}
}
