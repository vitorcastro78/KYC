namespace KYC.Web.Services;

/// <summary>Etiquetas e chaves de módulo partilhadas entre páginas de triagem.</summary>
public static class ScanProgressLabels
{
    public static string ToDisplayLabel(string moduleKey) => moduleKey switch
    {
        "EntityResolution" => "Resolução de entidade",
        "Sanctions" => "Sanções e fontes",
        "LLM" => "Score e relatório",
        "Concluído" => "Concluído",
        "A iniciar" => "A iniciar",
        _ => moduleKey
    };

    /// <summary>Aproxima a fase visível quando só há percentagem da BD (fallback sem SignalR).</summary>
    public static string ModuleKeyFromDatabasePercent(int percentComplete) => percentComplete switch
    {
        >= 100 => "Concluído",
        >= 92 => "LLM",
        >= 10 => "Sanctions",
        >= 5 => "EntityResolution",
        _ => "A iniciar"
    };
}
