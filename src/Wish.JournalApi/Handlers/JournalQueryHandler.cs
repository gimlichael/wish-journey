using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cuemon.AspNetCore.Http;
using Savvyio.Handlers;
using Savvyio.Queries;
using Wish.JournalApplication;
using Wish.JournalApplication.Queries;
using Wish.JournalApplication.Views;

namespace Wish.JournalApi.Handlers
{
    public class JournalQueryHandler : QueryHandler
    {
        private readonly IJournalDataStore _journalDataStore;
        private readonly IJournalEntryDataStore _journalEntryDataStore;

        public JournalQueryHandler(IJournalDataStore journalDataStore, IJournalEntryDataStore journalEntryDataStore)
        {
            _journalDataStore = journalDataStore;
            _journalEntryDataStore = journalEntryDataStore;
        }

        protected override void RegisterDelegates(IRequestReplyRegistry<IQuery> handlers)
        {
            handlers.RegisterAsync<ListJournal, IEnumerable<JournalCollectionViewModel>>(ListJournalAsync);
            handlers.RegisterAsync<GetJournal, JournalViewModel>(GetJournalAsync);
            handlers.RegisterAsync<ListJournalEntries, IEnumerable<JournalEntryCollectionViewModel>>(ListJournalEntriesAsync);
            handlers.RegisterAsync<GetJournalEntry, JournalEntryViewModel>(GetJournalEntryAsync);
        }

        private async Task<JournalEntryViewModel> GetJournalEntryAsync(GetJournalEntry query)
        {
			var projection = await _journalEntryDataStore.GetByIdAsync(new[] { query.JournalId, query.EntryId }).ConfigureAwait(false);
            if (projection.OwnerId != query.OwnerId) { throw new NotFoundException(); } // safeguard; throws 404 if owner and journal is mismatch
            return new JournalEntryViewModel()
            {
                Created = projection.Created,
                Modified = projection.Modified,
                Notes = projection.Notes,
                Coordinates = new Coordinates(projection.CoordinatesLatitude, projection.CoordinatesLongitude),
                Weather = new Weather
                {
                    Condition = projection.WeatherCondition,
                    ConditionCode = projection.WeatherConditionCode,
                    WindSpeed = projection.WeatherWindSpeed,
                    Humidity = projection.WeatherHumidity,
                    Temperature = projection.WeatherTemperature,
                    TemperatureApparent = projection.WeatherTemperatureApparent,
                    WindGust = projection.WeatherWindGust
                },
                Location = new Location
                {
                    City = projection.LocationCity,
                    Country = projection.LocationCountry,
                    Query = projection.LocationQuery
                }
            };
        }

        private async Task<IEnumerable<JournalEntryCollectionViewModel>> ListJournalEntriesAsync(ListJournalEntries query)
        {
            var projections = await _journalEntryDataStore.FindAllAsync(o =>
            {
                o.MaxInclusiveResultCount = query.MaxInclusiveResultCount;
                o.Filter = projection => projection.OwnerId == query.OwnerId && projection.JournalId == query.JournalId;
            }).ConfigureAwait(false);

            return projections.Select(projection => new JournalEntryCollectionViewModel()
            {
                Created = projection.Created,
                Modified = projection.Modified,
                Condition = projection.WeatherCondition,
                ConditionCode = projection.WeatherConditionCode,
                Id = projection.Id.ToString("N"),
                Latitude = projection.CoordinatesLatitude,
                Longitude = projection.CoordinatesLongitude,
                Query = projection.LocationQuery
            });
        }

        private async Task<JournalViewModel> GetJournalAsync(GetJournal query)
        {
            var projection = await _journalDataStore.GetByIdAsync(new[] { query.OwnerId, query.JournalId }).ConfigureAwait(false);
            return new JournalViewModel()
            {
                Created = projection.Created,
                Modified = projection.Modified,
                Description = projection.Description,
                Title = projection.Title
            };
        }

        private async Task<IEnumerable<JournalCollectionViewModel>> ListJournalAsync(ListJournal query)
        {
            var projections = await _journalDataStore.FindAllAsync(o =>
            {
                o.MaxInclusiveResultCount = query.MaxInclusiveResultCount;
                o.Filter = projection => projection.OwnerId == query.OwnerId;
            }).ConfigureAwait(false);

            return projections.Select(projection => new JournalCollectionViewModel()
            {
                Title = projection.Title,
                Id = projection.Id.ToString("N")
            });
        }
    }
}
