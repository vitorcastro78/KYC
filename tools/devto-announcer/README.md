# dev.to announcer

Publishes a short release article for KYC AI Platform.

## Local dry-run

```powershell
$env:RELEASE_TAG = "v0.0.1-beta"
$env:RELEASE_BODY = "First public beta."
$env:REPO_URL = "https://github.com/vitorcastro78/KYC"
$env:DRY_RUN = "true"
$env:DEVTO_API_KEY = "…"
dotnet run --project tools/devto-announcer -- post
```

Used by `.github/workflows/announce-release.yml` on `release: published`.
