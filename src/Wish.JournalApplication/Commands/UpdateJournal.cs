using Savvyio.Commands;

namespace Wish.JournalApplication.Commands
{
    public record UpdateJournal : Command
    {
        public UpdateJournal(OwnerId ownerId, JournalId journalId)
        {
            OwnerId = ownerId;
            JournalId = journalId;
        }

        public OwnerId OwnerId { get; }

        public JournalId JournalId { get; }

        public Title Title { get; set; }

        public Description Description { get; set; }
    }
}
