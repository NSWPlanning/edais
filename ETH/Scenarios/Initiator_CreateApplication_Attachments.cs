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
	public class Initiator_CreateApplication_Attachments : Scenario
	{
		public void DownloadAttachments()
		{
			var proposeCreate = Server.Receive()
				.ToData<ProposeCreateApplicationTransactionType>();

			proposeCreate.Application.Attachment.Count().Should().BeGreaterOrEqualTo(1);
			foreach (var attachment in proposeCreate.Application.Attachment)
			{
				attachment.Description.Length.Should().BeGreaterOrEqualTo(1);
				attachment.Description[0].Value.Should().NotBeNullOrEmpty();
				attachment.Type.Value.Should().NotBeNullOrEmpty();
				var uri = attachment.Item as string;
				uri.Should().NotBeNullOrEmpty();
				var uriBuilder = new UriBuilder(uri);
				uriBuilder.Uri.Scheme.Should().Be(Uri.UriSchemeHttps);
				DownloadAttachment(uri, attachment.Size.Value).Should().BeTrue();
				DownloadAttachment(uri, attachment.Size.Value).Should().BeTrue();
				DownloadAttachment(uri, attachment.Size.Value).Should().BeTrue();
				AttachmentScenarioType.Types.Should().Contain(attachment.Type.Value.ToUpperInvariant());
				DownloadAttachment(uri, attachment.Size.Value).Should().BeFalse("Attachments should only be able to be downloaded 3 times");
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
