using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Service.WalletManager.Controllers.IsAlive
{
    public class IsAliveResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("env")]
        public string Env { get; set; }

        [JsonPropertyName("isDebug")]
        public bool IsDebug { get; set; }

        [JsonPropertyName("startedAt")]
        public DateTime StartedAt { get; set; }

        [JsonPropertyName("issueIndicators")]
        public List<IssueIndicator> IssueIndicators { get; set; }

        public class IssueIndicator
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("value")]
            public string Value { get; set; }
        }
    }
}
