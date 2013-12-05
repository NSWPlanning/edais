namespace ETH.Scenarios
{
	public class PrivateCertifier_EndToEnd : EndToEndScenario
	{
		// pretend to be the private certifier talking to Council
		public void ForCouncil_Accept()
		{
			ProposeCreateApplication.Send();
			AcceptCreateApplication.Receive();
			DeclareDetermination.Send();
		}

		// pretend to be the private certifier talking to Council
		public void ForCouncil_Reject()
		{
			ProposeCreateApplication.Send();
			RejectCreateApplication.Receive();
		}

		// pretend to be the private certifier talking to EHC
		public void ForEhc_Accept()
		{
			ProposeCreateApplication.Receive();
			AcceptCreateApplication.Send();
			DeclareDetermination.Send();
		}

		// pretend to be the private certifier talking to EHC
		public void ForEhc_Reject()
		{
			ProposeCreateApplication.Receive();
			RejectCreateApplication.Send();
		}
	}
}