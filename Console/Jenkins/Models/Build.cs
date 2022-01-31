using Newtonsoft.Json;

namespace Jenkins.Models
{
    public class Artifact
    {
        [JsonProperty("displayPath")]
        public object DisplayPath { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("relativePath")]
        public string RelativePath { get; set; }
    }

    public class Build
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("number")]
        public int Number { get; set; }

        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("timestamp")]
        public object Timestamp { get; set; }

        [JsonProperty("artifacts")]
        public List<Artifact> Artifacts { get; set; }

        [JsonProperty("building")]
        public bool Building { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("estimatedDuration")]
        public int EstimatedDuration { get; set; }

        [JsonProperty("fullDisplayName")]
        public string FullDisplayName { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        public string JobName { get; set; }

        public string Version { get; set; }


        public override string? ToString()
        {
            var buildDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds((long)Timestamp / 1000);

            return $"{Number} \t {Result} \t {Version} \t {buildDate}";
        }
    }
}
