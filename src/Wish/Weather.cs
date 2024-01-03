using Savvyio.Domain;

namespace Wish
{
    public record Weather : ValueObject
    {
        public Weather()
        {
        }

        public double Temperature { get; set; }

        public double TemperatureApparent { get; set; }

        public string Condition { get; set; }

        public int ConditionCode { get; set; }

        public double WindSpeed { get; set; }

        public double WindGust { get; set; }

        public int Humidity { get; set; }
    }
}
