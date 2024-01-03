namespace Wish.JournalApplication.Inputs
{
    public record JournalInputModel
    {
        public JournalInputModel()
        {
        }

        public string Title { get; set; }

        public string Description { get; set; }
    }
}
