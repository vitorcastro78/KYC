namespace KYC.Infrastructure.ExternalSources.At;

public sealed class AtDebtorsTierDocument
{
    public string SourceUrl { get; init; } = "";
    public string PdfFileName { get; init; } = "";
    public string TierCode { get; init; } = "";
    public string TaxpayerType { get; init; } = "";
    public string DebtRangeLabel { get; init; } = "";
    public string? SourceUpdatedAt { get; init; }
    public DateTimeOffset DownloadedAt { get; init; }
    public int EntryCount { get; init; }
    public List<AtDebtorsTierEntry> Entries { get; init; } = [];
}

public sealed class AtDebtorsTierEntry
{
    public string Nif { get; init; } = "";
    public string Name { get; init; } = "";
}

public sealed class AtDebtorsManifest
{
    public DateTimeOffset SyncedAt { get; init; }
    public string SourceIndexUrl { get; init; } = "";
    public int TierCount { get; init; }
    public int TotalEntries { get; init; }
    public List<AtDebtorsManifestTier> Tiers { get; init; } = [];
}

public sealed class AtDebtorsManifestTier
{
    public string TierCode { get; init; } = "";
    public string TaxpayerType { get; init; } = "";
    public string DebtRangeLabel { get; init; } = "";
    public string PdfRelativePath { get; init; } = "";
    public string JsonRelativePath { get; init; } = "";
    public string? SourceUpdatedAt { get; init; }
    public int EntryCount { get; init; }
}

public sealed record AtDebtorsMatch(
    string Nif,
    string Name,
    string TierCode,
    string TaxpayerType,
    string DebtRangeLabel,
    string? SourceUpdatedAt);
