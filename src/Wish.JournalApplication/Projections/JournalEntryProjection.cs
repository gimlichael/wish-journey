using System;

namespace Wish.JournalApplication.Projections
{
    public class JournalEntryProjection
    {
        public JournalEntryProjection(Guid id, Guid journalId, Guid ownerId)
        {
            Id = id;
            JournalId = journalId;
            OwnerId = ownerId;
        }

        public Guid Id { get; }

        public Guid JournalId { get; }

        public Guid OwnerId { get; }

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

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset? Modified { get; set; }
    }
}
