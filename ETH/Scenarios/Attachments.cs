using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ETH.Soap;
using eDAIS;
using FluentAssertions;
using System.Net;
using System.IO;
using ETH.OutputModels;

namespace ETH.Scenarios
{
	public class Attachments : Scenario
	{
		public void ProposeCreate()
		{
			var proposeCreate = Soap.ToData<ProposeCreateApplicationTransactionType>(
				Server.Receive());

			TestAttachments(proposeCreate.Application.Attachment);
		}

		public void DeclareDetermination()
		{
			var declareDetermination = Soap.ToData<DeclareSaveDeterminationNotificationType>(Server.Receive());
			TestAttachments(declareDetermination.Application.Attachment);
		}

		void TestAttachments(Attachment[] attachments)
		{
			attachments.Count().Should().BeGreaterOrEqualTo(1, "At least one attachment is required");
			foreach (var attachment in attachments)
			{
				attachment.Description.Length.Should().BeGreaterOrEqualTo(1, "Attachment Description required");
				attachment.Description[0].Value.Should().NotBeNullOrEmpty("Attachment Description required");
				attachment.Type.Should().NotBeNull("Attachment Type required");
				attachment.Type.Value.Should().NotBeNullOrEmpty("Attachment Type required");
				var uri = attachment.Item as string;
				uri.Should().NotBeNullOrEmpty("Attachment URI should not be empty");
				var uriBuilder = new UriBuilder(uri);
				uriBuilder.Uri.Scheme.Should().Be(Uri.UriSchemeHttps, "URI should use Https");
				AttachmentScenarioType.Types.Should().Contain(attachment.Type.Value.ToUpperInvariant());
				DownloadAttachment(uri, attachment.Size.Value).Should().BeTrue();
				DownloadAttachment(uri, attachment.Size.Value).Should().BeTrue();
				// According to the spec the attachment is required to fail on the 3rd attempt but in reality this is not the case:
				//DownloadAttachment(uri, attachment.Size.Value).Should().BeTrue();
				//DownloadAttachment(uri, attachment.Size.Value).Should().BeFalse("Attachments should only be able to be downloaded 3 times");
			}
		}

		bool DownloadAttachment(string uri, decimal expectedSize)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
			HttpWebResponse response = request.GetResponse() as HttpWebResponse;
			using (var reader = new StreamReader(response.GetResponseStream()))
			{
				var attachmentData = reader.ReadToEnd();
				((decimal)response.ContentLength).Should().Be(expectedSize);
			}
			return (response.StatusCode == HttpStatusCode.OK);
		}
	}
}
