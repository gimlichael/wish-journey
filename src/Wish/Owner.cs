using System;
using System.Collections.Generic;
using Savvyio.Domain;

namespace Wish
{
    public class Owner : Entity<Guid>
    {
        Owner()
        {
        }

        public Owner(OwnerId id) : base(id)
        {
        }

        public Owner(EmailAddress emailAddress)
        {
            EmailAddress = emailAddress;
            Created = DateTime.UtcNow;
        }

        public IEnumerable<Journal> Journals { get; }

        public string EmailAddress { get; internal set; }

        public DateTime Created { get; }

        public DateTime? Modified { get; set; }
    }
}
