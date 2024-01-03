using System;
using Savvyio.EventDriven;

namespace Wish.JournalApplication.Events
{
    public record JournalEntryCreated : IntegrationEvent
    {
        public JournalEntryCreated(Guid id, Guid journalId, Guid ownerId, double coordinatesLatitude, double coordinatesLongitude, string locationCity, string locationCountry, string locationQuery, double weatherTemperature, double weatherTemperatureApparent, string weatherCondition, int weatherConditionCode, double weatherWindGust, double weatherWindSpeed, int weatherHumidity, string notes, DateTimeOffset created)
        {
            Id = id;
            JournalId = journalId;
            OwnerId = ownerId;
            CoordinatesLatitude = coordinatesLatitude;
            CoordinatesLongitude = coordinatesLongitude;
            LocationCity = locationCity;
            LocationCountry = locationCountry;
            LocationQuery = locationQuery;
            WeatherTemperature = weatherTemperature;
            WeatherTemperatureApparent = weatherTemperatureApparent;
            WeatherCondition = weatherCondition;
            WeatherConditionCode = weatherConditionCode;
            WeatherHumidity = weatherHumidity;
            WeatherWindSpeed = weatherWindSpeed;
            WeatherWindGust = weatherWindGust;
            Notes = notes;
            Created = created;
        }

        public Guid Id { get; }

        public Guid JournalId { get; }

        public Guid OwnerId { get; }

        public double CoordinatesLatitude { get; }

        public double CoordinatesLongitude { get; }

        public string LocationQuery { get; }

        public string LocationCity { get; }

        public string LocationCountry { get; }

        public double WeatherTemperature { get; }

        public double WeatherTemperatureApparent { get; }

        public string WeatherCondition { get; }

        public int WeatherConditionCode { get; }

        public double WeatherWindSpeed { get; }

        public double WeatherWindGust { get; }

        public int WeatherHumidity { get; }

        public string Notes { get; }

        public DateTimeOffset Created { get; }
    }
}
