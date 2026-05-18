using System.Text.Json;
using System.Text.Json.Serialization;

namespace AvaloniaClaudePet.Models;

public class HookPayload
{
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = string.Empty;

    [JsonPropertyName("hook_event_name")]
    public string HookEventName { get; set; } = string.Empty;

    [JsonPropertyName("input")]
    public JsonElement? Input { get; set; }
}

public class HealthResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "healthy";

    [JsonPropertyName("port")]
    public int Port { get; set; }
}
