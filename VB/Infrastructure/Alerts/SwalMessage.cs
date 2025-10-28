using System.Text.Json;

namespace Infrastructure.Alerts;

public sealed record SwalMessage(
    string Title,
    string? Text = null,
    string Icon = "success",
    bool Toast = true,
    string Position = "top-end",
    int Timer = 3000,
    bool ShowConfirmButton = false
)
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public string ToJson() => JsonSerializer.Serialize(this, Options);
}
