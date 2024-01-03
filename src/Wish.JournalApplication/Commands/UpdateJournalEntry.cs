using Savvyio.Commands;

namespace Wish.JournalApplication.Commands
{
    public record UpdateJournalEntry : Command
    {
        public UpdateJournalEntry(OwnerId ownerId, JournalId journalId, JournalEntryId entryId)
        {
            OwnerId = ownerId;
            JournalId = journalId;
            EntryId = entryId;
        }

        public OwnerId OwnerId { get; }

        public JournalId JournalId { get; }

        public JournalEntryId EntryId { get; }

        public Notes Notes { get; set; }
    }
}
