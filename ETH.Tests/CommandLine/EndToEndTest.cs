using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using System.Reflection;
using System.IO;

namespace ETH.Tests.CommandLine
{
	public class EthProcess : Process
	{
		public EthProcess()
		{

			var location = TestFiles.BinPath;
			var startInfo = new ProcessStartInfo();
			startInfo.CreateNoWindow = false;
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			startInfo.FileName = location;
			startInfo.RedirectStandardOutput = true;
			startInfo.UseShellExecute = false;
			this.StartInfo = startInfo;
		}
	}

	public class EndToEndTest
	{
		[Fact]
		public void RunEndToEnd()
		{
			var endToEndProgram = new EthProcess();
			endToEndProgram.StartInfo.Arguments = "-r test -t Council_EndToEnd.FromEHC_Accept -m  -u http://*:8181/ -s -c http://localhost:8182 http://localhost:8183";
			endToEndProgram.Start();

			var acceptCreateResponderProgram = new EthProcess();
			acceptCreateResponderProgram.StartInfo.Arguments = "-r test3 -t AcceptCreateApplication.Receive -m -s -u http://localhost:8182/";
			acceptCreateResponderProgram.Start();

			var declareDeterminationProgram = new EthProcess();
			declareDeterminationProgram.StartInfo.Arguments = "-r test4 -t DeclareDetermination.Receive -m -s -u http://localhost:8183/";
			declareDeterminationProgram.Start();

			var sendCreateApplicationProgram = new EthProcess();
			sendCreateApplicationProgram.StartInfo.Arguments = "-r test2 -m -s -c http://localhost:8181/ -t ProposeCreateApplication.Send -a APP-1231231234 test@localhost.test";
			sendCreateApplicationProgram.Start();
			sendCreateApplicationProgram.WaitForExit();
			endToEndProgram.WaitForExit();
			endToEndProgram.StandardOutput.ReadToEnd().Should().Be("{\"status\":\"ready\",\"endpoint\":\"http://*:8181/\"}\r\n{\"result\":\"pass\",\"message\":null}\r\n");
		}
	}
}
