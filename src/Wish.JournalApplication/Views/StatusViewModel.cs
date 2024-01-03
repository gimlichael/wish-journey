using System;
using Wish.JournalApplication.Projections;

namespace Wish.JournalApplication.Views
{
    public class StatusViewModel
    {
        public string Message { get; set; }

        public StatusResult Result { get; set; }

        public StatusAction Action { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Modified { get; set; }

        public string Endpoint { get; set; }

        public string EndpointRouteValue { get; set; }
    }
}
