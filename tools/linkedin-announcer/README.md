# LinkedIn announcer

Posts KYC release notes to LinkedIn (personal or organization URN).

## Local dry-run

```powershell
$env:RELEASE_TAG = "v0.0.1-beta"
$env:RELEASE_BODY = "First public beta."
$env:REPO_URL = "https://github.com/vitorcastro78/KYC"
$env:DRY_RUN = "true"
$env:LINKEDIN_PERSON_URN = "urn:li:person:…"
dotnet run --project tools/linkedin-announcer -- post
```

## OAuth (get refresh token)

```powershell
$env:LINKEDIN_CLIENT_ID = "…"
$env:LINKEDIN_CLIENT_SECRET = "…"
dotnet run --project tools/linkedin-announcer -- get-token
```

Store the printed values as GitHub Actions secrets used by `.github/workflows/announce-release.yml`.
