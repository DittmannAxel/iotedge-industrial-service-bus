using System.Collections.Generic;
using Newtonsoft.Json;

namespace Industrial.Service.Bus
{
    public sealed class Content
    {
        [JsonProperty("HwId")]
        public string HwId { get; set; }

        [JsonProperty("Data")]
        public IEnumerable<DataContent> Data { get; set; }
    }
}

