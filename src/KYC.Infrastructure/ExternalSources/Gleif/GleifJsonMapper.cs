using System.Text;
using System.Text.Json;
using KYC.Application.Models;

namespace KYC.Infrastructure.ExternalSources.Gleif;

internal static class GleifJsonMapper
{
    public static IReadOnlyList<GleifCompanySnapshot> MapLeiRecordList(JsonElement dataArray)
    {
        if (dataArray.ValueKind != JsonValueKind.Array)
            return [];

        var list = new List<GleifCompanySnapshot>();
        foreach (var item in dataArray.EnumerateArray())
        {
            var mapped = MapLeiRecordDataElement(item);
            if (mapped is not null)
                list.Add(mapped);
        }

        return list;
    }

    public static GleifCompanySnapshot? MapLeiRecordDataElement(JsonElement dataItem)
    {
        if (!dataItem.TryGetProperty("attributes", out var attrs))
            return null;

        var lei = ReadString(attrs, "lei");
        if (string.IsNullOrEmpty(lei) || !attrs.TryGetProperty("entity", out var entity))
            return null;

        var legalName = ReadNestedName(entity, "legalName");
        if (string.IsNullOrWhiteSpace(legalName))
            return null;

        var previousNames = new List<string>();
        if (entity.TryGetProperty("otherNames", out var otherNames) && otherNames.ValueKind == JsonValueKind.Array)
        {
            foreach (var on in otherNames.EnumerateArray())
            {
                var n = ReadNestedName(on, "name") ?? ReadString(on, "name");
                if (!string.IsNullOrWhiteSpace(n) &&
                    string.Equals(ReadString(on, "type"), "PREVIOUS_LEGAL_NAME", StringComparison.OrdinalIgnoreCase))
                    previousNames.Add(n);
            }
        }

        string? registrationStatus = null;
        DateTime? initialReg = null, lastUpdate = null;
        if (attrs.TryGetProperty("registration", out var reg))
        {
            registrationStatus = ReadString(reg, "status");
            initialReg = ReadDate(reg, "initialRegistrationDate");
            lastUpdate = ReadDate(reg, "lastUpdateDate");
        }

        return new GleifCompanySnapshot(
            lei,
            legalName,
            ReadString(entity, "jurisdiction"),
            ReadNestedCountry(entity, "legalAddress"),
            ReadString(entity, "registeredAs"),
            ReadRegisteredAtId(entity),
            ReadNestedId(entity, "legalForm"),
            ReadString(entity, "status"),
            registrationStatus,
            FormatAddress(entity, "legalAddress"),
            FormatAddress(entity, "headquartersAddress"),
            ReadString(attrs, "ocid"),
            ReadString(attrs, "bic"),
            ReadString(attrs, "conformityFlag"),
            ReadDate(entity, "creationDate"),
            initialReg,
            lastUpdate,
            previousNames);
    }

    public static async Task<JsonDocument?> GetJsonAsync(HttpClient http, string relativeUrl, CancellationToken ct)
    {
        using var response = await http.GetAsync(relativeUrl, ct).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            return null;

        await using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        return await JsonDocument.ParseAsync(stream, cancellationToken: ct).ConfigureAwait(false);
    }

    private static string? ReadString(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static string? ReadNestedName(JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var p))
            return null;
        if (p.ValueKind == JsonValueKind.String)
            return p.GetString();
        return ReadString(p, "name");
    }

    private static string? ReadNestedCountry(JsonElement entity, string addressProp)
    {
        if (!entity.TryGetProperty(addressProp, out var addr))
            return null;
        return ReadString(addr, "country");
    }

    private static string? ReadNestedId(JsonElement entity, string prop)
    {
        if (!entity.TryGetProperty(prop, out var lf))
            return null;
        return ReadString(lf, "id");
    }

    private static string? ReadRegisteredAtId(JsonElement entity)
    {
        if (!entity.TryGetProperty("registeredAt", out var ra))
            return null;
        return ReadString(ra, "id");
    }

    private static DateTime? ReadDate(JsonElement el, string prop)
    {
        var s = ReadString(el, prop);
        return DateTime.TryParse(s, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt)
            ? dt
            : null;
    }

    private static string? FormatAddress(JsonElement entity, string prop)
    {
        if (!entity.TryGetProperty(prop, out var addr))
            return null;

        var sb = new StringBuilder();
        if (addr.TryGetProperty("addressLines", out var lines) && lines.ValueKind == JsonValueKind.Array)
        {
            foreach (var line in lines.EnumerateArray())
            {
                if (line.ValueKind == JsonValueKind.String)
                {
                    if (sb.Length > 0) sb.Append(", ");
                    sb.Append(line.GetString());
                }
            }
        }

        var city = ReadString(addr, "city");
        var postal = ReadString(addr, "postalCode");
        var country = ReadString(addr, "country");
        if (!string.IsNullOrWhiteSpace(city))
        {
            if (sb.Length > 0) sb.Append(", ");
            sb.Append(city);
        }

        if (!string.IsNullOrWhiteSpace(postal))
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(postal);
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            if (sb.Length > 0) sb.Append(", ");
            sb.Append(country);
        }

        return sb.Length == 0 ? null : sb.ToString();
    }
}
