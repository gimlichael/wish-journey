using System;

namespace Wish.JournalApplication.Views
{
    public record JournalEntryCollectionViewModel
    {
        public string Id { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string Condition { get; set; }

        public int ConditionCode { get; set; }

        public string Query { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset? Modified { get; set; }
    }
}
