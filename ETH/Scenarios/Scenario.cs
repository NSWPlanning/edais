using eDAIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETH.Scenarios
{
	public abstract class Scenario : ScenarioType
	{
		protected ReceiptAcknowledgementSignalType ReceiptAcknowledgementSignalType
		{
			get
			{
				var result = Data.FromXml<ReceiptAcknowledgementSignalType>(
					"ReceiptAcknowledgementSignal.xml");
				result.ReceiptAcknowledgement.Timestamp.Value = DateTime.Now;
				return result;
			}
		}

		public string ReceiverParty { get; set; }

		protected void SetReceiverParty(StandardBusinessMessageHeader standardBusinessMessageHeader)
		{
			if (!string.IsNullOrWhiteSpace(ReceiverParty))
			{
				standardBusinessMessageHeader.ReceiverPartner.Authority.Value = ReceiverParty;
			}
		}
	}
}
