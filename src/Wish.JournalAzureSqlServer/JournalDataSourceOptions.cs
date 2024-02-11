using Cuemon;
using Cuemon.Configuration;

namespace Wish.JournalAzureSqlServer
{
    public class JournalDataSourceOptions : IValidatableParameterObject
    {
        public JournalDataSourceOptions()
        {
        }

        public string ConnectionString { get; set; }

        public void ValidateOptions()
        {
            Validator.ThrowIfInvalidState(string.IsNullOrWhiteSpace(ConnectionString));
        }
    }
}
