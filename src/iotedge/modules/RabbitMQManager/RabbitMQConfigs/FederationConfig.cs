using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace RabbitMQManager.RabbitMQConfigs
{
    public class FederationConfig
    {
        public IDictionary<string, Upstream> Upstreams { get; set; }

        public IDictionary<string, Policy> Policies { get; set; }
    }

    public class Upstream
    {
        [JsonProperty("uri")]
        public Uri Uri { get; set; }

        [JsonProperty("expires")]
        public long ExpirationTimeOut { get; set; }
    }

    public class Policy
    {
        [JsonProperty("pattern")]
        public string Pattern { get; set; }

        [JsonProperty("definition")]
        public IDictionary<string,string> Definition { get; set; }

        [JsonProperty("apply-to")]
        public string ApplyTo { get; set; }
    }
}
