using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Cuemon.AspNetCore.Http;
using Cuemon.Extensions;
using Cuemon.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Wish.JournalApplication;
using Wish.JournalApplication.Projections;

namespace Wish.JournalAzureTableStorage
{
    public class JournalEntryDataStore : TableClient, IJournalEntryDataStore
    {
        public JournalEntryDataStore(IConfiguration configuration) : base(configuration.GetConnectionString("JournalTable"), "JournalEntryProjection")
        {
        }

        public Task CreateAsync(JournalEntryProjection projection, Action<AsyncOptions> setup = null)
        {
            var options = setup.Configure();
            var entity = new JournalEntryEntity(projection.JournalId, projection.Id, projection.OwnerId)
            {
                Created = projection.Created.ToString("O"),
                TimeZone = projection.TimeZone,
                CoordinatesLatitude = projection.CoordinatesLatitude,
                CoordinatesLongitude = projection.CoordinatesLongitude,
                LocationCity = projection.LocationCity,
                LocationCountry = projection.LocationCountry,
                LocationQuery = projection.LocationQuery,
                Modified = projection.Modified?.ToString("O"),
                Notes = projection.Notes,
                WeatherCondition = projection.WeatherCondition,
                WeatherConditionCode = projection.WeatherConditionCode,
                WeatherHumidity = projection.WeatherHumidity,
                WeatherTemperature = projection.WeatherTemperature,
                WeatherTemperatureApparent = projection.WeatherTemperatureApparent,
                WeatherWindGust = projection.WeatherWindGust,
                WeatherWindSpeed = projection.WeatherWindSpeed
            };
            return AddEntityAsync(entity, options.CancellationToken);
        }

        public async Task UpdateAsync(JournalEntryProjection projection, Action<AsyncOptions> setup = null)
        {
            var options = setup.Configure();
            var entity = await GetEntityAsync<JournalEntryEntity>(projection.JournalId.ToString("N"), projection.Id.ToString("N")).ConfigureAwait(false);
            if (entity.HasValue)
            {
                entity.Value.Modified = projection.Modified?.ToString("O");
                entity.Value.Notes = projection.Notes;
                await UpdateEntityAsync(entity.Value, entity.Value.ETag, TableUpdateMode.Merge, options.CancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<JournalEntryProjection> GetByIdAsync(object id, Action<AsyncOptions> setup = null)
        {
            var options = setup.Configure();
            if (id is Guid[] compoundIds)
            {
                try
                {
                    var entity = await GetEntityAsync<JournalEntryEntity>(compoundIds[0].ToString("N"), compoundIds[1].ToString("N"), null, options.CancellationToken).ConfigureAwait(false);
                    if (entity.HasValue)
                    {
                        return new JournalEntryProjection(Guid.Parse(entity.Value.RowKey), Guid.Parse(entity.Value.PartitionKey), Guid.Parse(entity.Value.OwnerId))
                        {
                            Created = DateTimeOffset.Parse(entity.Value.Created),
                            TimeZone = entity.Value.TimeZone,
                            CoordinatesLatitude = entity.Value.CoordinatesLatitude,
                            CoordinatesLongitude = entity.Value.CoordinatesLongitude,
                            LocationCity = entity.Value.LocationCity,
                            LocationCountry = entity.Value.LocationCountry,
                            LocationQuery = entity.Value.LocationQuery,
                            Modified = entity.Value.Modified != null ? DateTimeOffset.Parse(entity.Value.Modified) : null,
                            Notes = entity.Value.Notes,
                            WeatherCondition = entity.Value.WeatherCondition,
                            WeatherConditionCode = entity.Value.WeatherConditionCode,
                            WeatherHumidity = entity.Value.WeatherHumidity,
                            WeatherTemperature = entity.Value.WeatherTemperature,
                            WeatherTemperatureApparent = entity.Value.WeatherTemperatureApparent,
                            WeatherWindGust = entity.Value.WeatherWindGust,
                            WeatherWindSpeed = entity.Value.WeatherWindSpeed
                        };
                    }
                }
                catch (RequestFailedException ex) when (ex.Status == StatusCodes.Status404NotFound)
                {
                    throw new NotFoundException();
                }
            }
            return null;
        }

        public async Task<IEnumerable<JournalEntryProjection>> FindAllAsync(Action<QueryOptions<JournalEntryProjection>> setup = null)
        {
            var pageSize = 1000;
            var options = setup.Configure();
            var continueQuerying = true;
            string continuationToken = null;
            var result = new List<JournalEntryProjection>();
            while (continueQuerying)
            {
                var pages = QueryAsync<JournalEntryEntity>(maxPerPage: pageSize, cancellationToken: options.CancellationToken)
                    .AsPages(continuationToken, pageSize)
                    .WithCancellation(options.CancellationToken)
                    .ConfigureAwait(false);
                await foreach (var page in pages)
                {
                    var hasEntities = false;
                    continuationToken = page.ContinuationToken;
                    foreach (var entity in page.Values)
                    {
                        hasEntities = true;
                        var dto = new JournalEntryProjection(Guid.Parse(entity.RowKey), Guid.Parse(entity.PartitionKey), Guid.Parse(entity.OwnerId))
                        {
                            Created = DateTimeOffset.Parse(entity.Created),
                            TimeZone = entity.TimeZone,
                            CoordinatesLatitude = entity.CoordinatesLatitude,
                            CoordinatesLongitude = entity.CoordinatesLongitude,
                            LocationCity = entity.LocationCity,
                            LocationCountry = entity.LocationCountry,
                            LocationQuery = entity.LocationQuery,
                            Modified = entity.Modified != null ? DateTimeOffset.Parse(entity.Modified) : null,
                            Notes = entity.Notes,
                            WeatherCondition = entity.WeatherCondition,
                            WeatherConditionCode = entity.WeatherConditionCode,
                            WeatherHumidity = entity.WeatherHumidity,
                            WeatherTemperature = entity.WeatherTemperature,
                            WeatherTemperatureApparent = entity.WeatherTemperatureApparent,
                            WeatherWindGust = entity.WeatherWindGust,
                            WeatherWindSpeed = entity.WeatherWindSpeed
                        };
                        if (options.Filter(dto)) { result.Add(dto); }
                    }
                    continueQuerying = hasEntities && result.Count <= options.MaxInclusiveResultCount && continuationToken != null;
                }
            }
            return result.OrderByDescending(dto => dto.Created).Take(options.MaxInclusiveResultCount);
        }

        public Task DeleteAsync(JournalEntryProjection dto, Action<AsyncOptions> setup = null)
        {
            var options = setup.Configure();
            return DeleteEntityAsync(dto.JournalId.ToString("N"), dto.Id.ToString("N"), ETag.All, options.CancellationToken);
        }
    }
}
