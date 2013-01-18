using eDAIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace ETH.Scenarios
{
	public static class ReceiptAcknowledgementSegment
	{
		public static void RunCommonTests(this ReceiptAcknowledgement receiptAcknoledgement)
		{
			receiptAcknoledgement.Should().NotBeNull();
			
		}
	}
}
