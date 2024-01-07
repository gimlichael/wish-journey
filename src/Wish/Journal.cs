using System;
using System.Collections.Generic;
using System.Linq;
using Cuemon;
using Savvyio.Domain;
using Wish.Events;

namespace Wish
{
    public class Journal : AggregateRoot<Guid>
    {
        Journal()
        {
        }

        public Journal(JournalId id) : base(id)
        {
        }

        public Journal(OwnerId ownerId, Title title, Description description = null)
        {
            Id = new JournalId();
            OwnerId = ownerId;
            Title = title;
            Description = description;
            Created = DateTime.UtcNow;
            AddEvent(new JournalInitiated(this));
        }

        public Guid OwnerId { get; }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public IEnumerable<JournalEntry> Entries { get; }

        public DateTime Created { get; }

        public DateTime? Modified { get; private set; }

        public void ChangeOwner(OwnerId ownerId, EmailAddress emailAddress)
        {
            Validator.ThrowIfNull(ownerId);
            Validator.ThrowIfNull(emailAddress);
            var owner = new Owner(ownerId);
            if (owner.Id != OwnerId) { throw new InvalidOperationException($"The specified {nameof(Owner)} is not related to the current {nameof(Journal)}."); }
            if (owner.EmailAddress.Equals(emailAddress)) { return; }
            owner.EmailAddress = emailAddress;
            owner.Modified = DateTime.UtcNow;
            AddEvent(new JournalOwnerChanged(this, owner));
        }

        public JournalEntry AddEntry(Coordinates coordinates, Location location, Weather weather, Timestamp timestamp, Notes notes)
        {
            Validator.ThrowIfNull(coordinates);
            Validator.ThrowIfNull(location);
            Validator.ThrowIfNull(weather);
            Validator.ThrowIfNull(timestamp);
            var entry = new JournalEntry(new JournalId(Id), coordinates, location, weather, timestamp, notes);
            AddEvent(new JournalEntryAdded(entry));
            return entry;
        }

        public JournalEntry ChangeEntry(JournalEntryId entryId, Notes notes, Func<string, Timestamp> timestampFactory)
        {
            Validator.ThrowIfNull(entryId);
            Validator.ThrowIfNull(notes);
            var entry = Entries.SingleOrDefault(entry => entry.Id == entryId);
            if (entry == null) { throw new InvalidOperationException($"The specified {nameof(JournalEntry)} is either not related to the current {nameof(Journal)} or it has been deleted."); }
            if (entry.Notes != null && entry.Notes.Equals(notes)) { return entry; }
            entry.Notes = notes;
            entry.Modified = timestampFactory(entry.TimeZone);
            AddEvent(new JournalEntryChanged(entry));
            return entry;
        }

        public JournalEntry RemoveEntry(JournalEntryId entryId)
        {
            Validator.ThrowIfNull(entryId);
            var entry = Entries.SingleOrDefault(entry => entry.Id == entryId);
            if (entry == null) { throw new InvalidOperationException($"The specified {nameof(JournalEntry)} is either not related to the current {nameof(Journal)} or it has been deleted."); }
            AddEvent(new JournalEntryRemoved(entry));
            return entry;
        }

        public void ChangeTitle(Title title)
        {
            Validator.ThrowIfNull(title);
            if (((string)title).Equals(Title)) { return; }
            Title = title;
            Modified = DateTime.UtcNow;
            AddEvent(new JournalTitleChanged(this));
        }

        public void ChangeDescription(Description description)
        {
            Validator.ThrowIfNull(description);
            if (((string)description).Equals(Description)) { return; }
            Description = description;
            Modified = DateTime.UtcNow;
            AddEvent(new JournalDescriptionChanged(this));
        }
    }
}
