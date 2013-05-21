using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace ETH.Util
{
	public class FailException : Exception
	{
		public FailException(string message, Exception wrappedException) 
			: base(message)
		{
			WrappedException = wrappedException;
		}

		public Exception WrappedException { get; private set; }
	}
}
