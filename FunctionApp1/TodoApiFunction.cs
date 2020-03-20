using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using AZureFunctionAPI;
using System.Linq;
using Microsoft.WindowsAzure.Storage;

namespace FunctionApp1
{
    public static class TodoApi
    {
        [FunctionName("CreateTodo")]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")] IAsyncCollector<TodoTableEntity> todoTable,
            [Queue("todos", Connection = "AzureWebJobsStorage")] IAsyncCollector<Todo> todoQueue,
            ILogger log)
        {
            log.LogInformation("Creating to do list item");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<TodoCreateModel>(requestBody);
            var todo = new Todo() { TaskDescription = data.TaskDescription };
            //items.Add(todo);
            await todoTable.AddAsync(todo.ToTableEntity());
            await todoQueue.AddAsync(todo);
            return new OkObjectResult(todo);
        }

        [FunctionName("GetTodos")]
        public static async Task<IActionResult> GetTodos(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log
            )
        {
            log.LogInformation("Getting to do list items");
            var query = new TableQuery<TodoTableEntity>();
            var segment = await todoTable.ExecuteQuerySegmentedAsync(query, null);
            return new OkObjectResult(segment.Select(Mappings.ToTodo));
        }

        [FunctionName("GetTodoById")]
        public static IActionResult GetTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id}")] HttpRequest req,
            [Table("todos", "TODO", "{id}", Connection = "AzureWebJobsStorage")] TodoTableEntity todo,
            ILogger log,
            string id
            )
        {
            log.LogInformation("Getting to do list item by id");
            if (todo == null)
            {
                log.LogInformation($"Item {id} not found");
                return new NotFoundResult();
            }
            return new OkObjectResult(todo.ToTodo());
        }

        [FunctionName("UpdateTodo")]
        public static async Task<IActionResult> UpdateTodo(
           [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todo/{id}")] HttpRequest req,
           [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
           ILogger log,
           string id
           )
        {
            log.LogInformation("Getting to do list item by id");
            var findOperation = TableOperation.Retrieve<TodoTableEntity>("TODO", id);
            var findResult = await todoTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                log.LogInformation($"Item {id} not found");
                return new NotFoundResult();
            }
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<TodoUpdateModel>(requestBody);
            var existingRow = (TodoTableEntity)findResult.Result;
            existingRow.IsCompleted = data.IsCompleted;
            if (!string.IsNullOrEmpty(data.TaskDescription))
            {
                existingRow.TaskDescription = data.TaskDescription;
            }
            var replaceOperation = TableOperation.Replace(existingRow);
            await todoTable.ExecuteAsync(replaceOperation);
            return new OkObjectResult(existingRow.ToTodo());
        }

        [FunctionName("DeleteTodoById")]
        public static async Task<IActionResult> DeleteTodoById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req,
            [Table("todos", Connection = "AzureWebJobsStorage")] CloudTable todoTable,
            ILogger log,
            string id
            )
        {
            log.LogInformation("Getting to do list item by id");
            var deleteOperation = TableOperation.Delete(new TableEntity() { PartitionKey = "TODO", RowKey = id, ETag = "*" });
            try
            {
                var deleteResult = await todoTable.ExecuteAsync(deleteOperation);
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == 404)
            {
                return new NotFoundResult();
            }
            return new OkResult();
        }
    }

}
