namespace ETH.Scenarios
{
	public class PrivateCertifier_EndToEnd : EndToEndScenario
	{
		public void ForCouncil_Accept()
		{
			ProposeCreateApplication.Send();
			AcceptCreateApplication.Receive();
			DeclareDetermination.Send();
		}

		public void ForCouncil_Reject()
		{
			ProposeCreateApplication.Send();
			RejectCreateApplication.Receive();
		}

		public void ForEhc_Accept()
		{
			ProposeCreateApplication.Receive();
			AcceptCreateApplication.Send();
			DeclareDetermination.Receive();
		}

		public void ForEhc_Reject()
		{
			ProposeCreateApplication.Receive();
			RejectCreateApplication.Send();
		}
	}
}