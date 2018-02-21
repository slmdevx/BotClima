using Newtonsoft.Json;

namespace ClimaBot
{
    public partial class Deserialize
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("data")]
        public Datum[] Data { get; set; }
    }

    public partial class Datum
    {
        [JsonProperty("date")]
        public System.DateTime Date { get; set; }

        [JsonProperty("date_br")]
        public string DateBr { get; set; }

        [JsonProperty("humidity")]
        public Humidity Humidity { get; set; }

        [JsonProperty("rain")]
        public Rain Rain { get; set; }

        [JsonProperty("wind")]
        public Wind Wind { get; set; }

        [JsonProperty("uv")]
        public Uv Uv { get; set; }

        [JsonProperty("thermal_sensation")]
        public ThermalSensation ThermalSensation { get; set; }

        [JsonProperty("text_icon")]
        public TextIcon TextIcon { get; set; }

        [JsonProperty("temperature")]
        public Temperature Temperature { get; set; }
    }

    public partial class Humidity
    {
        [JsonProperty("min")]
        public long Min { get; set; }

        [JsonProperty("max")]
        public long Max { get; set; }
    }

    public partial class Rain
    {
        [JsonProperty("probability")]
        public long Probability { get; set; }

        [JsonProperty("precipitation")]
        public long Precipitation { get; set; }
    }

    public partial class Temperature
    {
        [JsonProperty("min")]
        public long Min { get; set; }

        [JsonProperty("max")]
        public long Max { get; set; }

        [JsonProperty("morning")]
        public Humidity Morning { get; set; }

        [JsonProperty("afternoon")]
        public Humidity Afternoon { get; set; }

        [JsonProperty("night")]
        public Humidity Night { get; set; }
    }

    public partial class TextIcon
    {
        [JsonProperty("icon")]
        public Icon Icon { get; set; }

        [JsonProperty("text")]
        public Text Text { get; set; }
    }

    public partial class Icon
    {
        [JsonProperty("dawn")]
        public string Dawn { get; set; }

        [JsonProperty("morning")]
        public string Morning { get; set; }

        [JsonProperty("afternoon")]
        public string Afternoon { get; set; }

        [JsonProperty("night")]
        public string Night { get; set; }

        [JsonProperty("day")]
        public string Day { get; set; }

        [JsonProperty("reduced")]
        public string Reduced { get; set; }
    }

    public partial class Text
    {
        [JsonProperty("pt")]
        public string Pt { get; set; }

        [JsonProperty("en")]
        public string En { get; set; }

        [JsonProperty("es")]
        public string Es { get; set; }

        [JsonProperty("phrase")]
        public Icon Phrase { get; set; }
    }

    public partial class ThermalSensation
    {
        [JsonProperty("max")]
        public double Max { get; set; }

        [JsonProperty("min")]
        public double Min { get; set; }
    }

    public partial class Uv
    {
        [JsonProperty("max")]
        public long Max { get; set; }
    }

    public partial class Wind
    {
        [JsonProperty("velocity_min")]
        public long VelocityMin { get; set; }

        [JsonProperty("velocity_max")]
        public long VelocityMax { get; set; }

        [JsonProperty("velocity_avg")]
        public long VelocityAvg { get; set; }

        [JsonProperty("gust_max")]
        public double? GustMax { get; set; }

        [JsonProperty("direction_degrees")]
        public long DirectionDegrees { get; set; }

        [JsonProperty("direction")]
        public string Direction { get; set; }
    }

    public partial class Deserialize
    {
        public static Deserialize FromJson(string json) => JsonConvert.DeserializeObject<Deserialize>(json, ClimaBot.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Deserialize self) => JsonConvert.SerializeObject(self, ClimaBot.Converter.Settings);
    }

    public class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
        };
    }
}