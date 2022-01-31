using System.Collections;
using Newtonsoft.Json;

namespace Jenkins.Models
{
    public class JobsList
    {
        [JsonProperty("jobs")]
        public List<Job> Jobs { get; set; } = new List<Job>();
    }

    public class Job
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("builds")]
        public List<Build> Builds { get; set; } = new List<Build>();
    }
}
