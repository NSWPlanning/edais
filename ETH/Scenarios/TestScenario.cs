using ETH.Soap;
using FluentAssertions;
using eDAIS;

namespace ETH.Scenarios
{
	public class TestScenario : Scenario
	{
		public void Valid1()
		{
			var request = Server.Receive();
			var proposeCreate = Soap.ToData<ProposeCreateApplicationTransactionType>(request);

			proposeCreate.Application.ApplicationNumber.Value.Should().Be("APP-0000134647");

			var response = new ReceiptAcknowledgementSignalType
			{
				Specialisation = new[]
				{
					new Specialisation
					{
						@ref = "hurr"
					}
				}
			};

			Server.Respond("Moo", response, request);
		}

		public void Fail()
		{
			1.Should().Be(2);
		}

		public void Pass()
		{
			1.Should().Be(1);
		}
	}
}
