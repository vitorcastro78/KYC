using System.Text;
using System.Text.Json;
using KYC.Infrastructure.ExternalSources.At;

var dataRoot = args.Length > 0
    ? args[0]
    : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "src", "KYC.Workers", "Data", "AT", "Devedores");

dataRoot = Path.GetFullPath(dataRoot);
Console.WriteLine($"Data root: {dataRoot}");

var jsonOptions = AtDebtorsJson.SerializerOptions;
var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var manifestTiers = new List<AtDebtorsManifestTier>();
var syncedAt = DateTimeOffset.UtcNow;

foreach (var tier in AtDebtorsTierCatalog.All)
{
    var tierDir = Path.Combine(dataRoot, tier.FolderName);
    var pdfPath = Path.Combine(tierDir, "_pdf", tier.PdfFileName);
    var jsonPath = Path.Combine(tierDir, $"{tier.Code}.json");

    if (!File.Exists(pdfPath))
    {
        Console.WriteLine($"SKIP {tier.Code}: PDF em falta {pdfPath}");
        continue;
    }

    var parsed = AtDebtorsPdfParser.ParseFile(pdfPath);
    var document = new AtDebtorsTierDocument
    {
        SourceUrl = $"https://static.portaldasfinancas.gov.pt/app/devedores_static/{tier.PdfFileName}",
        PdfFileName = tier.PdfFileName,
        TierCode = tier.Code,
        TaxpayerType = tier.TaxpayerTypeLabel,
        DebtRangeLabel = AtDebtorsTextNormalizer.Normalize(tier.DebtRangeLabel),
        SourceUpdatedAt = parsed.SourceUpdatedAt?.ToString("yyyy-MM-dd"),
        DownloadedAt = syncedAt,
        EntryCount = parsed.Entries.Count,
        Entries = parsed.Entries
            .Select(e => new AtDebtorsTierEntry { Nif = e.Nif, Name = e.Name })
            .ToList()
    };

    await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(document, jsonOptions), utf8);

    manifestTiers.Add(new AtDebtorsManifestTier
    {
        TierCode = tier.Code,
        TaxpayerType = tier.TaxpayerTypeLabel,
        DebtRangeLabel = document.DebtRangeLabel,
        PdfRelativePath = $"{tier.FolderName}/_pdf/{tier.PdfFileName}".Replace('\\', '/'),
        JsonRelativePath = $"{tier.FolderName}/{tier.Code}.json".Replace('\\', '/'),
        SourceUpdatedAt = document.SourceUpdatedAt,
        EntryCount = document.EntryCount
    });

    Console.WriteLine($"OK {tier.Code}: {document.EntryCount} entradas -> {jsonPath}");
}

var manifest = new AtDebtorsManifest
{
    SyncedAt = syncedAt,
    SourceIndexUrl = "https://static.portaldasfinancas.gov.pt/app/devedores_static/de-devedores.html",
    TierCount = manifestTiers.Count,
    TotalEntries = manifestTiers.Sum(t => t.EntryCount),
    Tiers = manifestTiers
};

var manifestPath = Path.Combine(dataRoot, "manifest.json");
await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest, jsonOptions), utf8);
Console.WriteLine($"Manifest: {manifest.TotalEntries} entradas totais -> {manifestPath}");
