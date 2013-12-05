using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETH.Tests
{
	static class TestFiles
	{
		public static string Path
		{
			get
			{
				// Dodgy hack to get the path to this file
				var st = new StackTrace(new StackFrame(true));
				var sf = st.GetFrame(0);
				var thisFilePath = System.IO.Path.GetDirectoryName(sf.GetFileName());
				return thisFilePath;
			}
		}

		public static string BinPath
		{
			get
			{
				return System.IO.Path.Combine(Path, @"..\ETH\bin\Debug\ETH.exe");
			}
		}
	}
}
