using Cuemon;
using Cuemon.Configuration;

namespace Wish.JournalAzureTableStorage
{
    public class StatusTableOptions : IValidatableParameterObject
    {
        public StatusTableOptions()
        {
        }

        public string ConnectionString { get; set; }

        public void ValidateOptions()
        {
            Validator.ThrowIfInvalidState(string.IsNullOrWhiteSpace(ConnectionString));
        }
    }
}
