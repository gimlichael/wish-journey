using System;
using Azure;
using Azure.Data.Tables;

namespace Wish.JournalAzureTableStorage
{
    public class JournalEntity : ITableEntity
    {
        public JournalEntity()
        {
        }

        public JournalEntity(Guid ownerId, Guid journalId)
        {
            PartitionKey = ownerId.ToString("N");
            RowKey = journalId.ToString("N");
        }

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Modified { get; set; }
    }
}
