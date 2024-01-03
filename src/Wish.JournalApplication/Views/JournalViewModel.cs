using System;

namespace Wish.JournalApplication.Views
{
    public record JournalViewModel
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Modified { get; set; }
    }
}
