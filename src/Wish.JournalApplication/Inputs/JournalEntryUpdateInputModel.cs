namespace Wish.JournalApplication.Inputs
{
    public record JournalEntryUpdateInputModel
    {
        public JournalEntryUpdateInputModel()
        {
        }

        public string Notes { get; set; }
    }
}
