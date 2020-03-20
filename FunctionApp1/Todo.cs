using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace AZureFunctionAPI
{
    public class Todo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("n");
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class TodoCreateModel
    {
        public string TaskDescription { get; set; }
    }

    public class TodoUpdateModel
    {
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class TodoTableEntity: TableEntity
    {
        public DateTime CreatedTime { get; set; }
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }

    //public class TableEntity
    //{
    //    public string PartitionKey { get; set; }
    //    public string RowKey { get; set; }
    //}
    public static class Mappings
    {
        public static TodoTableEntity ToTableEntity(this Todo todo)
        {
            return new TodoTableEntity
            {
                PartitionKey = "TODO",
                RowKey = todo.Id,
                CreatedTime = todo.CreatedTime,
                IsCompleted = todo.IsCompleted,
                TaskDescription = todo.TaskDescription
                //Timestamp = DateTime.Now
            };
        }

        public static Todo ToTodo(this TodoTableEntity todo)
        {
            return new Todo
            {
                Id = todo.RowKey,
                CreatedTime = todo.CreatedTime,
                IsCompleted = todo.IsCompleted,
                TaskDescription = todo.TaskDescription
            };
        }
    }
}
