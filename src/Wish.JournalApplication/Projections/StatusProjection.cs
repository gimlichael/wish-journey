using System;

namespace Wish.JournalApplication.Projections
{
    public class StatusProjection
    {
        public StatusProjection(Guid ownerId, Guid correlationId)
        {
            OwnerId = ownerId;
            CorrelationId = correlationId;
        }

        public Guid OwnerId { get; }

        public Guid CorrelationId { get; }

        public string Message { get; set; }

        public StatusResult Result { get; set; } = StatusResult.Pending;

        public StatusAction Action { get; set; } = StatusAction.Undefined;

        public string Endpoint { get; set; }

        public string EndpointRouteValue { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Modified { get; set; }
    }

    public enum StatusResult
    {
        Pending,
        Completed,
        Failed
    }

    public enum StatusAction
    {
        Undefined,
        Create,
        Update,
        Delete
    }
}
