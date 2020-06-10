using Newtonsoft.Json;

namespace Industrial.Service.Bus
{
    public sealed class Message
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("specversion")]
        public string SpecVersion { get; set; }

        [JsonProperty("datacontenttype")]
        public string DataContentType { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }
    }
}