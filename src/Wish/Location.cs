using Savvyio.Domain;

namespace Wish
{
    public record Location : ValueObject
    {
        public Location()
        {
        }

        public string Query { get; set; }

        public string City { get; set; }

        public string Country { get; set; }
    }
}
