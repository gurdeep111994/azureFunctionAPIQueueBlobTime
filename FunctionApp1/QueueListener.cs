using System;
using System.Threading.Tasks;
using AZureFunctionAPI;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FunctionApp1
{
    public static class QueueListener
    {
        [FunctionName("Function1")]
        public static async Task Run([QueueTrigger("todos", Connection = "")]Todo todo, 
            [Blob("todos", Connection ="AzureWebJobsStorage")] CloudBlobContainer container,
            ILogger log)
        {
            await container.CreateIfNotExistsAsync();
            var blob = container.GetBlockBlobReference($"{todo.Id}.txt");
            await blob.UploadTextAsync($"Created a new task: {todo.TaskDescription}");
            log.LogInformation($"C# Queue trigger function processed: {todo.TaskDescription}");
        }
    }
}
