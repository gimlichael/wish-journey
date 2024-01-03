using Savvyio.Commands;

namespace Wish.JournalApplication.Commands
{
    public record CreateJournalEntry : Command
    {
        public CreateJournalEntry(OwnerId ownerId, JournalId journalId)
        {
            OwnerId = ownerId;
            JournalId = journalId;
        }

        public OwnerId OwnerId { get; }

        public JournalId JournalId { get; }

        public Timestamp Timestamp { get; set; }

        public Coordinates Coordinates { get; set; }

        public Notes Notes { get; set; }
    }
}
