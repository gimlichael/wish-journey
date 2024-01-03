using Savvyio.Commands;

namespace Wish.JournalApplication.Commands
{
    public record CreateJournal : Command
    {
        public CreateJournal(OwnerId ownerId)
        {
            OwnerId = ownerId;
        }

        public OwnerId OwnerId { get; }

        public Title Title { get; set; }

        public Description Description { get; set; }
    }
}
