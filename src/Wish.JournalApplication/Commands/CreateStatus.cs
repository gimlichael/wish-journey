using System;
using Savvyio.Commands;
using Wish.JournalApplication.Projections;

namespace Wish.JournalApplication.Commands
{
    public record CreateStatus : Command
    {
        public CreateStatus(OwnerId ownerId, string correlationId)
        {
            OwnerId = ownerId;
            CorrelationId = Guid.Parse(correlationId);
        }

        public Guid OwnerId { get; }

        public Guid CorrelationId { get; }

        public string Endpoint { get; set; }

        public string Message { get; set; }

        public StatusAction Action { get; set; }

        public DateTime Created { get; set; }
    }
}
