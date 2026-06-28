using System.Text.Json.Serialization;

namespace AzureTestApp
{
    public class MessageModel
    {
        public string Id { get; set; } = null!;
        public DateTimeOffset ReceivedAt { get; set; }
        public DateTimeOffset? ProcessedAt { get; set; }
        public string Data { get; set; } = string.Empty;
    }
}
