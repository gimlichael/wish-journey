using System;
using Azure;
using Azure.Data.Tables;
using Wish.JournalApplication.Projections;

namespace Wish.JournalAzureTableStorage
{
    public class StatusEntity : ITableEntity
    {
        public StatusEntity()
        {
        }

        public StatusEntity(Guid ownerId, Guid correlationId)
        {
            PartitionKey = ownerId.ToString("N");
            RowKey = correlationId.ToString("N");
        }

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        public string Message { get; set; }

        public StatusResult Result { get; set; }

        public StatusAction Action { get; set; }

        public string Endpoint { get; set; }

        public string EndpointRouteValue { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Modified { get; set; }
    }
}
