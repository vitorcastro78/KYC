# Releasing KYC AI Platform

Release flow mirrors [ContextMemory](https://github.com/Kortexio/ContextMemory): conventional commits → release-please → GitHub Release → Docker / SDKs / announcements.

## Pipeline

1. **Push to `main`** with conventional commits (`feat:`, `fix:`, `chore:`…).
2. **CI** runs tests; **docker-publish** pushes `ghcr.io/vitorcastro78/kyc` and `kyc-workers` (`:latest` + SHA).
3. **release-please** opens/updates a PR that bumps `version.txt` and `CHANGELOG.md`.
4. **Merge the release PR** → tag `v*` + GitHub Release.
5. On **`release: published`**:
   - Docker tags get semver (same `docker-publish` on tag push)
   - Windows appliance assets attach (`release.yml`)
   - PyPI (`sdk/python`) if `PYPI_API_TOKEN` is set
   - npm (`sdk/typescript`) if `NPM_TOKEN` is set
   - LinkedIn + dev.to if announce secrets are set

## Repo settings

Enable **Allow GitHub Actions to create and approve pull requests** (required by release-please).

## Secrets

| Secret | Used by |
| ------ | ------- |
| `GITHUB_TOKEN` | Docker GHCR (built-in `packages:write`) |
| `PYPI_API_TOKEN` | `publish-pypi` (skip if empty) |
| `NPM_TOKEN` | `publish-npm` (skip if empty) |
| `LINKEDIN_*` | `announce-release` (see `tools/linkedin-announcer`) |
| `DEVTO_API_KEY` | `announce-release` (skip if empty) |

## SDK versions

Bump `sdk/python/pyproject.toml` and `sdk/typescript/package.json` in the release PR when you want package versions to match the git tag (CI publishes whatever is in the tree).

## Dry-run announcements

```bash
gh workflow run announce-release.yml -f dry_run=true -f release_tag=v0.0.1-beta
```

## Consume a release

```bash
docker compose -f docker-compose.ghcr.yml up -d
# or
./scripts/docker-run.sh
```

Windows appliance: download `KycPlatform-*-setup.exe` from the GitHub Release.
