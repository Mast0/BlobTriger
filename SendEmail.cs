using System;
using System.IO;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace BlobTriger
{
    public class SendEmail
    {
        [FunctionName("SendEmail")]
        public void Run([BlobTrigger("reenbit-docs/{name}", Connection = "BlobConnection")]Stream myBlob, string name, ILogger log)
        {
            // get configuration file
            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("local.settings.json", true).AddEnvironmentVariables().Build();
            BlobClient blobClientSAS = BlobService.GetBlobSasClient(config.GetValue<string>("BlobConnection"), "reenbit-docs", name);

            // receiving the email to which need to send the letter
            string emailTo = BlobService.GetEmailFromMetadata(blobClientSAS);

            // sending the letter
            BlobService.SendUrlEmail(emailTo, name, blobClientSAS);
            
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        }

        
    }
}
