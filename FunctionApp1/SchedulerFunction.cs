using System;
using System.Threading.Tasks;
using AZureFunctionAPI;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace FunctionApp1
{
    public static class SchedulerFunction
    {
        [FunctionName("SchedulerFunction")]
        public static async Task Run([TimerTrigger("0 */2 * * * *")]TimerInfo myTimer,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable, 
            ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            var query = new TableQuery<TodoTableEntity>();

            var segment = await todoTable.ExecuteQuerySegmentedAsync(query, null) ;
            var deleted = 0;
            foreach (var todo in segment)
            {
                if (todo.IsCompleted)
                {
                    await todoTable.ExecuteAsync(TableOperation.Delete(todo));
                    deleted++;
                }
            }
            log.LogInformation($"Deleted {deleted} items at {DateTime.Now}");
        }
    }
}
