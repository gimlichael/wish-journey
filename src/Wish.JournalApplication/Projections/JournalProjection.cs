using System;

namespace Wish.JournalApplication.Projections
{
    public class JournalProjection
    {
        public JournalProjection(Guid id, Guid ownerId)
        {
            Id = id;
            OwnerId = ownerId;
        }

        public Guid Id { get; }

        public Guid OwnerId { get; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Modified { get; set; }
    }
}
