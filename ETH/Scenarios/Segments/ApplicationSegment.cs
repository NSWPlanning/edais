using eDAIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace ETH.Scenarios
{
	public static class ApplicationSegment
	{

		public static void RunCommonTests(this Application application)
		{
			application.ApplicationNumber.Value.Should().NotBeNullOrEmpty();
			application.ApplicationNumber.Value.Should().StartWith("APP-");
			application.ApplicationNumber.Value.Length.Should().Be(14);
		}

		public static void RunStatusTests(this Application application)
		{
			//  Neither Infor nor Civica provide a <status> in AcceptCreateApplicationTransaction, so status checks split out from RunCommonTests
			application.Status.Should().NotBeNull();
			application.Status.Value.Should().NotBeNullOrEmpty();
			application.Status.Value.ToUpperInvariant().Should().BeOneOf(new[] { "LODGED", "APPROVED", "PENDING" });
		}

		public static void RunProposeCreateTests(this Application application)
		{
			application.RunCommonTests();
			application.RunStatusTests();
			application.Status.Value.ToUpperInvariant().Should().BeOneOf(new[] { "LODGED", "APPROVED" });

			application.SubjectLand.Should().NotBeNull();
			application.SubjectLand.Length.Should().Be(1);
			var subjectLand = application.SubjectLand[0];
			subjectLand.Address.Should().NotBeNull();
			subjectLand.Address.RunCommonTests();

			application.Comments.Should().NotBeNull();
			application.CreatedDate.Should().NotBeNull();
			application.LodgementDate.Should().NotBeNull();

			subjectLand.Parcel.Should().NotBeNull();
			subjectLand.Parcel.Classification.Should().NotBeNull();
			subjectLand.Parcel.Name.Should().NotBeNull();
			subjectLand.Parcel.OID.Should().NotBeNull();
			subjectLand.Parcel.OID.Value.ToUpperInvariant().Should().BeOneOf(CouncilList);

			subjectLand.Document.Should().NotBeNull();
			subjectLand.Document.DocumentNo.Should().NotBeNull();
			subjectLand.Document.DocumentNo.Value.Should().NotBeNullOrEmpty();
			subjectLand.Document.Jurisdiction.Should().NotBeNull();
			subjectLand.Document.Jurisdiction.Value.Should().NotBeNullOrEmpty();
			subjectLand.Document.SurveyDocument.Should().NotBeNull();
			subjectLand.Document.SurveyDocument.Survey.Should().NotBeNull();
			subjectLand.Document.SurveyDocument.Survey.Length.Should().Be(1);
			subjectLand.Document.SurveyDocument.Survey[0].CoordinateSystem.Should().NotBeNull();
			subjectLand.Document.SurveyDocument.Survey[0].CoordinateSystem.HorizontalDatumName.Should().NotBeNull();

			application.PartyRole.Should().NotBeNull();
			application.PartyRole.Length.Should().BeGreaterThan(0);
			foreach (var role in application.PartyRole)
			{
				role.Party.Should().NotBeNull();
				role.Party.Length.Should().Be(1);
				var party = role.Party[0];
				party.Identification.Should().NotBeNull();
				party.Name.Should().NotBeNull();
				party.Type.Should().NotBeNull();
				party.Type.Value.Should().NotBeNullOrEmpty();

				var person = party.Item as Person;
				person.Should().NotBeNull("Person segment missing or not serializable");
				person.Identification.Should().NotBeNull();
				person.Internet.Should().NotBeNull();
				person.Internet.Length.Should().BeGreaterOrEqualTo(1);
				person.Internet[0].Type.Should().NotBeNull();
				person.Internet[0].URI.Should().NotBeNullOrEmpty();
				person.PersonName.Should().NotBeNull();
				person.PersonName.Length.Should().Be(1);
				var personName = person.PersonName[0];
				personName.Classification.Should().NotBeNull();
				personName.FamilyName.Should().NotBeNull();
				personName.GivenName.Should().NotBeNull();
				personName.MiddleName.Should().NotBeNull();
				personName.Name.Should().NotBeNull();
				personName.Title.Should().NotBeNull();
				person.PostalAddress.Should().NotBeNull();
				person.PostalAddress.Length.Should().Be(1);
				var address = person.PostalAddress[0];
				address.BuildingNumber.Should().NotBeNull();
				address.CityName.Should().NotBeNull();
				address.CountryName.Should().NotBeNull();
				address.CountrySubDivision.Should().NotBeNull();
				address.Identification.Should().NotBeNull();
				address.LineOne.Should().NotBeNull();
				address.LineTwo.Should().NotBeNull();
				address.LineThree.Should().NotBeNull();
				address.LineFour.Should().NotBeNull();
				address.StreetName.Should().NotBeNull();
				address.Postcode.Should().NotBeNull();
				address.StreetSuffix.Should().NotBeNull();
				address.StreetType.Should().NotBeNull();
				person.Telephone.Should().NotBeNull();
				person.Telephone.Length.Should().BeGreaterOrEqualTo(1);
				var telephone = person.Telephone[0];
				telephone.AreaCode.Should().NotBeNull();
				telephone.CountryCode.Should().NotBeNull();
				telephone.Number.Should().NotBeNull();
				telephone.Type.Should().NotBeNull();
			}
		}

		static IEnumerable<string> CouncilList
		{
			get
			{
				return new[] {
					"BANKSTOWN CITY COUNCIL",
					"BLACKTOWN CITY COUNCIL",
					"LAKE MACQUARIE CITY COUNCIL",
					"PORT MACQUARIE-HASTINGS COUNCIL",
					"ROCKDALE CITY COUNCIL",
					"SHELLHARBOUR CITY COUNCIL",
					"SUTHERLAND SHIRE COUNCIL",
					"TAMWORTH REGIONAL COUNCIL",
					"THE HILLS SHIRE COUNCIL",
					"TWEED SHIRE COUNCIL"
				};
			}
		}
	}
}
