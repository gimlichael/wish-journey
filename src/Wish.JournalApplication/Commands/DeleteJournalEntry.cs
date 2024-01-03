using Savvyio.Commands;

namespace Wish.JournalApplication.Commands
{
    public record DeleteJournalEntry : Command
    {
        public DeleteJournalEntry(OwnerId ownerId, JournalId journalId, JournalEntryId entryId)
        {
            OwnerId = ownerId;
            JournalId = journalId;
            EntryId = entryId;
        }

        public OwnerId OwnerId { get; }

        public JournalId JournalId { get; }

        public JournalEntryId EntryId { get; }
    }
}
