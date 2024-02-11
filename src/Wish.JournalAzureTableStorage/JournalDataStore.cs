using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Cuemon;
using Cuemon.AspNetCore.Http;
using Cuemon.Extensions;
using Cuemon.Threading;
using Microsoft.AspNetCore.Http;
using Wish.JournalApplication;
using Wish.JournalApplication.Projections;

namespace Wish.JournalAzureTableStorage
{
    public class JournalDataStore : TableClient, IJournalDataStore
    {
        public JournalDataStore(JournalTableOptions options) : base(Validator.CheckParameter(() =>
        {
            Validator.ThrowIfInvalidOptions(options);
            return options.ConnectionString;
        }), "JournalProjection")
        {
        }

        public Task CreateAsync(JournalProjection projection, Action<AsyncOptions> setup = null)
        {
            var options = setup.Configure();
            var entity = new JournalEntity(projection.OwnerId, projection.Id)
            {
                Created = projection.Created,
                Description = projection.Description,
                Title = projection.Title
            };
            return AddEntityAsync(entity, options.CancellationToken);
        }

        public async Task UpdateAsync(JournalProjection projection, Action<AsyncOptions> setup = null)
        {
            var options = setup.Configure();
            var entity = await GetEntityAsync<JournalEntity>(projection.OwnerId.ToString("N"), projection.Id.ToString("N")).ConfigureAwait(false);
            if (entity.HasValue)
            {
                entity.Value.Description = projection.Description;
                entity.Value.Title = projection.Title;
                entity.Value.Modified = projection.Modified;
                await UpdateEntityAsync(entity.Value, entity.Value.ETag, TableUpdateMode.Merge, options.CancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<JournalProjection> GetByIdAsync(object id, Action<AsyncOptions> setup = null)
        {
            var options = setup.Configure();
            if (id is Guid[] compoundIds)
            {
                try
                {
                    var entity = await GetEntityAsync<JournalEntity>(compoundIds[0].ToString("N"), compoundIds[1].ToString("N"), null, options.CancellationToken).ConfigureAwait(false);
                    if (entity.HasValue)
                    {
                        return new JournalProjection(Guid.Parse(entity.Value.PartitionKey), Guid.Parse(entity.Value.RowKey))
                        {
                            Created = entity.Value.Created,
                            Modified = entity.Value.Modified,
                            Description = entity.Value.Description,
                            Title = entity.Value.Title
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

        public async Task<IEnumerable<JournalProjection>> FindAllAsync(Action<QueryOptions<JournalProjection>> setup = null)
        {
            var pageSize = 1000;
            var options = setup.Configure();
            var continueQuerying = true;
            string continuationToken = null;
            var result = new List<JournalProjection>();
            while (continueQuerying)
            {
                var pages = QueryAsync<JournalEntity>(maxPerPage: pageSize, cancellationToken: options.CancellationToken)
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
                        var dto = new JournalProjection(Guid.Parse(entity.RowKey), Guid.Parse(entity.PartitionKey))
                        {
                            Created = entity.Created,
                            Description = entity.Description,
                            Title = entity.Title,
                            Modified = entity.Modified
                        };
                        if (options.Filter(dto)) { result.Add(dto); }
                    }
                    continueQuerying = hasEntities && result.Count <= options.MaxInclusiveResultCount && continuationToken != null;
                }
            }
            return result.OrderByDescending(dto => dto.Created).Take(options.MaxInclusiveResultCount);
        }

        public Task DeleteAsync(JournalProjection projection, Action<AsyncOptions> setup = null)
        {
            var options = setup.Configure();
            return DeleteEntityAsync(projection.OwnerId.ToString("N"), projection.Id.ToString("N"), ETag.All, options.CancellationToken);
        }
    }
}
