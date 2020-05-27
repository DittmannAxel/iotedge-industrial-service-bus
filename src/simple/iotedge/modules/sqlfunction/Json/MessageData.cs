using System.Collections.Generic;
using Newtonsoft.Json;

namespace Industrial.Service.Bus
{
    public sealed class MessageData
    {
        [JsonProperty("PublishTimestamp")]
        public string PublishTimestamp { get; set; }

        [JsonProperty("Content")]
        public IEnumerable<Content> Contents { get; set; }
    }
}