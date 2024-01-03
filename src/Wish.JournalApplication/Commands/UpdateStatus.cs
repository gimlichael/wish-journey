using System;
using Savvyio.Commands;
using Wish.JournalApplication.Projections;

namespace Wish.JournalApplication.Commands
{
    public record UpdateStatus : Command
    {
        public UpdateStatus(OwnerId ownerId, string correlationId)
        {
            OwnerId = ownerId;
            CorrelationId = Guid.Parse(correlationId);
        }

        public Guid OwnerId { get; }

        public Guid CorrelationId { get; }

        public string Message { get; set; }

        public StatusResult Result { get; set; }

        public StatusAction Action { get; set; }

        public string EndpointRouteValue { get; set; }

        public DateTime Modified { get; set; }
    }
}
