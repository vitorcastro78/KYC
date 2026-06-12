namespace KYC.Workers;

public sealed class AtDebtorsDailyDownloadOptions
{
    public const string SectionKey = "ExternalSources:AtDebtorsDailyDownload";

    /// <summary>Quando false, o worker não descarrega (reavalia a cada 5 min).</summary>
    public bool Enabled { get; set; }

    public string BaseUrl { get; set; } =
        "https://static.portaldasfinancas.gov.pt/app/devedores_static/";

    /// <summary>Raiz relativa ao ContentRoot (ex.: Data/AT/Devedores).</summary>
    public string DataRootPath { get; set; } = "Data/AT/Devedores";

    /// <summary>Intervalo entre sincronizações completas (1–168 horas).</summary>
    public int IntervalHours { get; set; } = 24;
}
