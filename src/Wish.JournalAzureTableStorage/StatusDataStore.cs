using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Cuemon.Extensions;
using Cuemon.Threading;
using Microsoft.Extensions.Configuration;
using Wish.JournalApplication;
using Wish.JournalApplication.Projections;

namespace Wish.JournalAzureTableStorage
{
    public class StatusDataStore : TableClient, IStatusDataStore
    {
        public StatusDataStore(IConfiguration configuration) : base(configuration.GetConnectionString("JournalTable"), "JournalStatus")
        {
        }

        public Task CreateAsync(StatusProjection projection, Action<AsyncOptions> setup = null)
        {
            var options = setup.Configure();
            var entity = new StatusEntity(projection.OwnerId, projection.CorrelationId)
            {
                Created = projection.Created,
                Modified = projection.Modified,
                Message = projection.Message,
                Result = projection.Result,
                Action = projection.Action,
                Endpoint = projection.Endpoint,
                EndpointRouteValue = projection.EndpointRouteValue
            };
            return AddEntityAsync(entity, options.CancellationToken);
        }

        public async Task UpdateAsync(StatusProjection projection, Action<AsyncOptions> setup = null)
        {
            var options = setup.Configure();
            var entity = await GetEntityAsync<StatusEntity>(projection.OwnerId.ToString("N"), projection.CorrelationId.ToString("N")).ConfigureAwait(false);
            if (entity.HasValue)
            {
                entity.Value.Message = projection.Message;
                entity.Value.Result = projection.Result;
                entity.Value.Action = projection.Action;
                entity.Value.Modified = projection.Modified;
                entity.Value.Endpoint = projection.Endpoint;
                entity.Value.EndpointRouteValue = projection.EndpointRouteValue;
                await UpdateEntityAsync(entity.Value, entity.Value.ETag, TableUpdateMode.Merge, options.CancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<StatusProjection>> FindAllAsync(Action<QueryOptions<StatusProjection>> setup = null)
        {
            var pageSize = 1000;
            var options = setup.Configure();
            var continueQuerying = true;
            string continuationToken = null;
            var result = new List<StatusProjection>();
            while (continueQuerying)
            {
                var pages = QueryAsync<StatusEntity>(maxPerPage: pageSize, cancellationToken: options.CancellationToken)
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
                        var dto = new StatusProjection(Guid.Parse(entity.PartitionKey), Guid.Parse(entity.RowKey))
                        {
                            Created = entity.Created,
                            Modified = entity.Modified,
                            Message = entity.Message,
                            Result = entity.Result,
                            Action = entity.Action,
                            Endpoint = entity.Endpoint?.ToLowerInvariant(),
                            EndpointRouteValue = entity.EndpointRouteValue?.ToLowerInvariant()
                        };
                        if (options.Filter(dto)) { result.Add(dto); }
                    }
                    continueQuerying = hasEntities && result.Count <= options.MaxInclusiveResultCount && continuationToken != null;
                }
            }
            return result.OrderByDescending(dto => dto.Created).Take(options.MaxInclusiveResultCount);
        }

        public Task DeleteAsync(StatusProjection dto, Action<AsyncOptions> setup = null)
        {
            var options = setup.Configure();
            return DeleteEntityAsync(dto.OwnerId.ToString("N"), dto.CorrelationId.ToString("N"), ETag.All, options.CancellationToken);
        }
    }
}
