using System.Diagnostics;

namespace ETH.Tests
{
	public class EthProcess : Process
	{
		public EthProcess()
		{
			var location = TestFiles.BinPath;
			var startInfo = new ProcessStartInfo
			{
				CreateNoWindow = false,
				WindowStyle = ProcessWindowStyle.Hidden,
				FileName = location,
				RedirectStandardOutput = true,
				UseShellExecute = false
			};
			StartInfo = startInfo;
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (!HasExited)
				{
					Kill();
					WaitForExit(500);
				}
			}
			catch
			{
			}
			base.Dispose(disposing);
		}
	}
}