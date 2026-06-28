using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ProcessingFunctionApp
{
    public class MessageModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;
        public DateTimeOffset ReceivedAt { get; set; }
        public DateTimeOffset? ProcessedAt { get; set; }
        public string Data { get; set; } = string.Empty;
    }
}
