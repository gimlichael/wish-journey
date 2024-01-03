using System;

namespace Wish.JournalApplication.Views
{
    public record JournalEntryViewModel
    {
        public Coordinates Coordinates { get; set; }

        public Location Location { get; set; }

        public Weather Weather { get; set; }

        public string Notes { get; set; }

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset? Modified { get; set;}
    }
}
