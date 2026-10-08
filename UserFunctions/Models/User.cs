using Azure;
using Azure.Data.Tables;

namespace UserFunctions.Models
{
    public class User : ITableEntity
    {
        public string PartitionKey { get; set; } = "Users";

        public string RowKey { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; } = "";

        public string Email { get; set; } = "";

        public string Phone { get; set; } = "";

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }
    }
}