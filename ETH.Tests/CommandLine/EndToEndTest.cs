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
using System.Threading;

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
			using (endToEndProgram = new EthProcess())
			using (acceptCreateResponderProgram = new EthProcess())
			using (declareDeterminationProgram = new EthProcess())
			using (sendCreateApplicationProgram = new EthProcess())
			{
				endToEndProgram.StartInfo.Arguments = "-r test -t Council_EndToEnd.FromEHC_Accept -m  -u http://*:8181/ -s -c http://localhost:8182 http://localhost:8183";
				endToEndProgram.Start();

				acceptCreateResponderProgram.StartInfo.Arguments = "-r test3 -t AcceptCreateApplication.Receive -m -s -u http://localhost:8182/";
				acceptCreateResponderProgram.Start();

				declareDeterminationProgram.StartInfo.Arguments = "-r test4 -t DeclareDetermination.Receive -m -s -u http://localhost:8183/";
				declareDeterminationProgram.Start();

				sendCreateApplicationProgram.StartInfo.Arguments = "-r test2 -m -s -c http://localhost:8181/ -t ProposeCreateApplication.Send -a APP-1231231234 test@localhost.test";
				sendCreateApplicationProgram.Start();
				Thread.Sleep(5000);
				endToEndProgram.StandardOutput.ReadToEnd().Should().Be("{\"status\":\"ready\",\"endpoint\":\"http://*:8181/\"}\r\n{\"result\":\"pass\",\"message\":null}\r\n");
			}
		}
		EthProcess endToEndProgram;
		EthProcess acceptCreateResponderProgram;
		EthProcess declareDeterminationProgram;
		EthProcess sendCreateApplicationProgram;

		~EndToEndTest()
		{
			if (!endToEndProgram.HasExited) endToEndProgram.Kill();
			if (!acceptCreateResponderProgram.HasExited) acceptCreateResponderProgram.Kill();
			if (!declareDeterminationProgram.HasExited) declareDeterminationProgram.Kill();
			if (!sendCreateApplicationProgram.HasExited) sendCreateApplicationProgram.Kill();
		}
	}
}
