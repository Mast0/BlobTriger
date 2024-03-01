using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;

namespace BlobTriger
{
    public class BlobService
    {
        public static Uri CreateSasUri(BlobClient blobClient)
        // Creating SAS URI
        {
            if (blobClient.CanGenerateSasUri)
            {
                // configure builder
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                    BlobName = blobClient.Name,
                    Resource = "b",
                };

                // setting the expiry time
                sasBuilder.ExpiresOn = DateTime.UtcNow.AddHours(1);
                sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

                // generating SAS URI
                Uri sasUri = blobClient.GenerateSasUri(sasBuilder);

                return sasUri;
            }
            else
            {
                return null;
            }
        }

        public static BlobClient GetBlobSasClient(string connection, string containerName, string blobName)
        // Return BlobClient With SAS URI
        {
            BlobContainerClient blobContainer = new(connection, containerName);
            BlobClient blob = blobContainer.GetBlobClient(blobName);
            Uri blobSasUri = CreateSasUri(blob);
            BlobClient blobClientSAS = new(blobSasUri);
            return blobClientSAS;
        }

        public static string GetEmailFromMetadata(BlobClient blobClient)
        // Return email from metadata
        {
            BlobProperties prop = blobClient.GetProperties();
            string emailTo = string.Empty;
            foreach (var metadata in prop.Metadata)
            {
                emailTo = metadata.Value;
            }
            return emailTo;
        }

        public static void SendUrlEmail(string emailTo, string blobName, BlobClient blobClient)
        // Send email with SAS URI to blob file
        {
            using (var client = new SmtpClient())
            {
                var mail = new MimeKit.MimeMessage();
                mail.Sender = MailboxAddress.Parse("bogdanremuga@gmail.com");
                mail.To.Add(MailboxAddress.Parse(emailTo));
                var builder = new BodyBuilder();
                builder.HtmlBody = $"file {blobName} seccsessfully uploaded\n" +
                    $"file uri: {blobClient.Uri.AbsoluteUri}";
                mail.Body = builder.ToMessageBody();
                mail.Subject = "Email sent test";

                client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                client.Authenticate("bogdanremuga@gmail.com", "kzmykzkdbdfneldt");
                client.Send(mail);
            }
        }
    }
}
