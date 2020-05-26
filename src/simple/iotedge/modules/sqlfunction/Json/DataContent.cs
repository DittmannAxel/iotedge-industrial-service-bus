using System.Collections.Generic;
using Newtonsoft.Json;

namespace Industrial.Service.Bus
{
    internal sealed class DataContent
    {
        [JsonProperty("CorrelationId")]
        public string CorrelationId { get; set; }

        [JsonProperty("SourceTimestamp")]
        public string SourceTimestamp { get; set; }

        [JsonProperty("Values")]
        public IEnumerable<Measurement> Values { get; set; }
    }
}