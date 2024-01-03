using System;
using Azure;
using Azure.Data.Tables;

namespace Wish.JournalAzureTableStorage
{
    public class JournalEntryEntity : ITableEntity
    {
        public JournalEntryEntity()
        {
        }
        public JournalEntryEntity(Guid journalId, Guid entryId, Guid ownerId)
        {
            PartitionKey = journalId.ToString("N");
            RowKey = entryId.ToString("N");
            OwnerId = ownerId.ToString("N");
        }

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public string OwnerId { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        public string TimeZone { get; set; }

        public double CoordinatesLatitude { get; set; }

        public double CoordinatesLongitude { get; set; }

        public string LocationQuery { get; set; }

        public string LocationCity { get; set; }

        public string LocationCountry { get; set; }

        public double WeatherTemperature { get; set; }

        public double WeatherTemperatureApparent { get; set; }

        public string WeatherCondition { get; set; }

        public int WeatherConditionCode { get; set; }

        public double WeatherWindSpeed { get; set; }

        public double WeatherWindGust { get; set; }

        public int WeatherHumidity { get; set; }

        public string Notes { get; set; }

        public string Created { get; set; }

        public string Modified { get; set; }
    }
}
