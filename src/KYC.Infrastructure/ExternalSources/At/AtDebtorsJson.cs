using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KYC.Infrastructure.ExternalSources.At;

public static class AtDebtorsJson
{
    public static JsonSerializerOptions SerializerOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
}
