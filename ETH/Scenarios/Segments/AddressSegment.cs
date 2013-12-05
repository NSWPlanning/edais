using eDAIS;
using FluentAssertions;

namespace ETH.Scenarios
{
	public static class AddressSegment
	{
		public static void RunCommonTests(this Address address)
		{
			address.CountryName.Value.Should().Be("AUS");
			address.CountrySubDivision.Value.Should().Be("NSW");
			address.LineThree.Should().NotBeNull();
			address.LineThree.Value.Should().NotBeNullOrEmpty();
			address.LineOne.Should().NotBeNull();
			address.LineOne.Value.Should().NotBeNullOrEmpty();
			address.CityName.Should().NotBeNull();
			address.CityName.Value.Should().NotBeNullOrEmpty();
			address.Postcode.Should().NotBeNull();
			address.Postcode.Value.Should().NotBeNullOrEmpty();
			address.Identification.Should().NotBeNull();
			address.Identification.Value.Should().NotBeNullOrEmpty();
		}
	}
}
