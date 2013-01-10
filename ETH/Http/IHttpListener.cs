using System;
using System.Collections.Generic;

namespace ETH.Http
{
	// Interface for System.Net.HttpListener
	public interface IHttpListener : IDisposable
	{
		void Start();
		void Stop();
		IHttpListenerContextTask GetContextAsync();
		ICollection<string> Prefixes { get; }
		bool IsListening { get; }
	}
}