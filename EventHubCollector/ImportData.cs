using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Hadoop.Avro.Container;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace EventHubCollector
{
    public static class ImportData
    {
        private static string StorageContainerName = Environment.GetEnvironmentVariable("StorageContainerName");
        private static string StorageAccountName = Environment.GetEnvironmentVariable("StorageAccountName");
        private static string KeyVaultName = Environment.GetEnvironmentVariable("KeyvaultName");
        private static string OutputType = Environment.GetEnvironmentVariable("OutputType");
        [FunctionName("ImportData")]
        public async static void Run([TimerTrigger("%TimerInterval%")]TimerInfo myTimer, [CosmosDB(
                databaseName: "%ComsosDatabase%",
                collectionName: "%CosmosCollection%",
                ConnectionStringSetting = "CosmosDBConnection")]
                IAsyncCollector<string> steventsOut, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            var container = await GetBlobContainer();
            var EventHubData = await GetBlobItems(container, log);

            foreach (EventData ehEvent in EventHubData)
            {
                if (ehEvent != null)
                {
                    if (OutputType == "body")
                    {
            
                        string content = Newtonsoft.Json.JsonConvert.SerializeObject(ehEvent.Body);
                        await steventsOut.AddAsync(content);
                    }
                    if(OutputType == "all")
                    {

                   
                        string content = Newtonsoft.Json.JsonConvert.SerializeObject(ehEvent);
                        await steventsOut.AddAsync(content);
                    }
                   
                }
            }

 
                  RemoveBlob(container, log);

        }


        public static async Task<string> getSecretAsync(string secretname, string vaultname)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyvaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            var secretValue = await keyvaultClient.GetSecretAsync($"https://{vaultname}.vault.azure.net/", secretname);
            return secretValue.Value;
        }

        public static async Task<CloudBlobContainer> GetBlobContainer()
        {
            var StorageAccountKeySecretName = Environment.GetEnvironmentVariable("StorageAccountKeySecretName");
            var StorageAccountKey = await getSecretAsync(StorageAccountKeySecretName, KeyVaultName);
            CloudStorageAccount storageAccount = new CloudStorageAccount(new StorageCredentials(StorageAccountName, StorageAccountKey), true);
            // blob client
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            // container
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(StorageContainerName);
            return blobContainer;
        }

        public static async Task<List<EventData>> GetBlobItems(CloudBlobContainer container, ILogger log)
        {
 
            BlobContinuationToken blobContinuationToken = null;
            List<EventData> eventDataList= new List<EventData>();
            var TimeWindow=Environment.GetEnvironmentVariable("FunctionIntervalInMinutes");
            do
            {
                var results = await container.ListBlobsSegmentedAsync(null, true, BlobListingDetails.Metadata, null, blobContinuationToken, null, null);
                // Get the value of the continuation token returned by the listing call.
                blobContinuationToken = results.ContinuationToken;
                log.LogInformation("total results:" + results.Results.Count());
                foreach (var blob in results.Results.OfType<CloudBlockBlob>().OrderByDescending(x => x.Properties.Created.Value))
                {
      
                    if (blob.Properties.Created.Value.ToUniversalTime() > DateTime.Now.AddMinutes(int.Parse(TimeWindow)*-1).ToUniversalTime())
                    {
                

        
                        if (blob.Properties.Length != 508)
                        {
                            using (var stream = await blob.OpenReadAsync())
                            {

                                var reader = AvroContainer.CreateGenericReader(stream);

            
                                while (reader.MoveNext())
                                {
                                    foreach (dynamic record in reader.Current.Objects)
                                    {
                                        var eventData = new EventData(record);
                                        eventDataList.Add(eventData);


                                    }
                                }
                               // await blob.DeleteAsync();
                 

                            }
                        }
                
                    }
                    else
                    {
                   
                        blobContinuationToken = null;
                    }

                  
                }

                //foreach (var blob in results.Results.OfType<CloudBlockBlob>().OrderByDescending(x => x.Properties.Created.Value))
                //{
                //    await blob.DeleteAsync();
                //}
                } while (blobContinuationToken != null); // Loop while the continuation token is not null. 

            return eventDataList;
        }

        public static async void  RemoveBlob(CloudBlobContainer container, ILogger log)
        {
            BlobContinuationToken blobContinuationToken = null;

            do
            {
                var results = await container.ListBlobsSegmentedAsync(null, true, BlobListingDetails.Metadata, null, blobContinuationToken, null, null);
                // Get the value of the continuation token returned by the listing call.
                blobContinuationToken = results.ContinuationToken;
                log.LogInformation("total results:" + results.Results.Count());
                foreach (var blob in results.Results.OfType<CloudBlockBlob>())
                {
                    await blob.DeleteIfExistsAsync();
                }
            } while (blobContinuationToken != null); // Loop while the continuation token is not null. 
        }


            //private static async Task<List<EventData>> Dump(CloudBlockBlob blob, ILogger log)
            //{
            //    // Check for blob with no event data, size is always 508
            //    if (blob.Properties.Length != 508)
            //    {
            //        using (var stream = await blob.OpenReadAsync())
            //        {

            //            var reader = AvroContainer.CreateGenericReader(stream);

            //            List<EventData> eventDataList = new List<EventData>();
            //            while (reader.MoveNext())
            //            {
            //                foreach (dynamic record in reader.Current.Objects)
            //                {
            //                    var eventData = new EventData(record);
            //                    eventDataList.Add(eventData);


            //                }
            //            }
            //            await blob.DeleteAsync();
            //            return eventDataList;

            //        }
            //    }

            //    return null;

            //}
        }
    }
