using Cuemon;
using Cuemon.Configuration;

namespace Wish.JournalAzureTableStorage
{
    public class JournalTableOptions : IValidatableParameterObject
    {
        public JournalTableOptions()
        {
        }

        public string ConnectionString { get; set; }

        public void ValidateOptions()
        {
            Validator.ThrowIfInvalidState(string.IsNullOrWhiteSpace(ConnectionString));
        }
    }
}
