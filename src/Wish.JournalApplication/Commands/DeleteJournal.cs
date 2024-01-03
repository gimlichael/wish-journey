using Savvyio.Commands;

namespace Wish.JournalApplication.Commands
{
    public record DeleteJournal : Command
    {
        public DeleteJournal(OwnerId ownerId, JournalId journalId)
        {
            OwnerId = ownerId;
            JournalId = journalId;
        }

        public OwnerId OwnerId { get; }

        public JournalId JournalId { get; }
    }
}
