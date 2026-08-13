# Set GitHub repository topics for discoverability (requires `gh auth login`).
# Usage:
#   pwsh ./scripts/set-github-topics.ps1
#   pwsh ./scripts/set-github-topics.ps1 -Repo vitorcastro78/KYC

param(
    [string]$Repo = ""
)

$ErrorActionPreference = "Stop"

$catalog = @{
    "vitorcastro78/KYC" = @(
        "kyc", "aml", "know-your-customer", "blazor", "csharp", "dotnet",
        "postgresql", "contextmemory", "sanctions", "pep",
        "self-hosted", "agpl", "open-source", "fintech", "compliance"
    )
    "vitorcastro78/CreditAI" = @(
        "credit-risk", "credit-scoring", "fintech", "csharp", "dotnet", "blazor",
        "postgresql", "contextmemory", "self-hosted", "agpl",
        "open-source", "model-risk", "open-banking", "explainable-ai", "crc"
    )
    "vitorcastro78/Fincheck" = @(
        "fintech", "sme", "cashflow", "open-banking", "csharp", "dotnet", "blazor",
        "postgresql", "contextmemory", "multi-tenant", "self-hosted",
        "agpl", "open-source", "mollie", "pme"
    )
}

function Set-Topics([string]$name, [string[]]$topics) {
    Write-Host "==== $name ===="
    $tmp = New-TemporaryFile
    try {
        @{ names = @($topics) } | ConvertTo-Json -Compress | Set-Content -Path $tmp -Encoding utf8NoBOM
        gh api -X PUT "repos/$name/topics" `
            -H "Accept: application/vnd.github+json" `
            --input $tmp
        Write-Host "OK topics: $($topics -join ', ')"
    }
    finally {
        Remove-Item $tmp -Force -ErrorAction SilentlyContinue
    }
}

gh auth status | Out-Null

if ($Repo) {
    if (-not $catalog.ContainsKey($Repo)) { throw "Unknown repo $Repo. Known: $($catalog.Keys -join ', ')" }
    Set-Topics $Repo $catalog[$Repo]
}
else {
    foreach ($key in @($catalog.Keys | Sort-Object)) {
        try { Set-Topics $key $catalog[$key] }
        catch { Write-Warning "$key : $($_.Exception.Message)" }
    }
}
