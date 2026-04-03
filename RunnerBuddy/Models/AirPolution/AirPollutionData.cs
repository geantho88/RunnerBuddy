using System.Text.Json.Serialization;

namespace RunnerBuddy.Models.AirPolution
{
    public class AirPollutionData
    {
        public Coord Coord { get; set; }
        public List[] List { get; set; }
    }
}
