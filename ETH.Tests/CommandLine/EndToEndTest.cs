using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;
using System.Reflection;
using System.IO;
using System.Threading;
using NCrunch.Framework;

namespace ETH.Tests.CommandLine
{
	public class EndToEndTest
	{
		[Fact]
		[ExclusivelyUsesAttribute("OIUEWROJDOSFJOIWEU")]
		public void RunEndToEnd11()
		{
			RunEndToEnd(true);
		}

		[Fact]
		[ExclusivelyUsesAttribute("OIUEWROJDOSFJOIWEU")]
		public void RunEndToEnd12()
		{
			RunEndToEnd(false);
		}

		void RunEndToEnd(bool useSoap11)
		{
			var soapArgument = useSoap11 ? " -m" : "";
			using (endToEndProgram = new EthProcess())
			using (acceptCreateResponderProgram = new EthProcess())
			using (declareDeterminationProgram = new EthProcess())
			using (sendCreateApplicationProgram = new EthProcess())
			{
				endToEndProgram.StartInfo.Arguments = "-t Council_EndToEnd.FromEHC_Accept -u http://+:8181/ -s -c http://localhost:8182 http://localhost:8183" + soapArgument;
				endToEndProgram.Start();
				endToEndProgram.ShouldHaveJsonLine(new { status = "ready", endpoint = "http://+:8181/" });

				acceptCreateResponderProgram.StartInfo.Arguments = "-t AcceptCreateApplication.Receive -s -u http://+:8182/" + soapArgument;
				acceptCreateResponderProgram.Start();
				acceptCreateResponderProgram.StandardOutput.ShouldHaveJsonLine(new { status = "ready", endpoint = "http://+:8182/" });

				declareDeterminationProgram.StartInfo.Arguments = "-t DeclareDetermination.Receive -s -u http://+:8183/" + soapArgument;
				declareDeterminationProgram.Start();
				declareDeterminationProgram.StandardOutput.ShouldHaveJsonLine(new { status = "ready", endpoint = "http://+:8183/" });

				sendCreateApplicationProgram.StartInfo.Arguments = "-s -c http://localhost:8181/ -t ProposeCreateApplication.Send -a APP-1231231234 test@localhost.test" + soapArgument;
				sendCreateApplicationProgram.Start();
				sendCreateApplicationProgram.ShouldHaveJsonLine(new { result = "pass" });

				endToEndProgram.ShouldHaveJsonLine(new { result = "pass" });
			}
		}
		EthProcess endToEndProgram;
		EthProcess acceptCreateResponderProgram;
		EthProcess declareDeterminationProgram;
		EthProcess sendCreateApplicationProgram;
	}
}
