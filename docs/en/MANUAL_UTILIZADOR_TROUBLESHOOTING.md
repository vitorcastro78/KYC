# User Manual and Troubleshooting — KYC AI Platform

> Analysts, supervisors, and administrators.

## 1. Access

| Environment | URL | Authentication |
|-------------|-----|----------------|
| UAT | _[Institutional URL]_ | Entra ID (MFA) |
| Local dev | `http://localhost:8080` | `admin@kyc.local` (see `.env`) |

Roles: `KYC.Analyst`, `KYC.Supervisor`, `KYC.Admin`, `KYC.Auditor`.

## 2. Main workflows

### New case
1. **Cases → New** — NIF, amount, relationship, CAE
2. If RCBE/GLEIF cannot resolve the entity, enter the **company name (manual)** (mandatory)
3. Wait for the progress bar (automated screening)
4. Review signals → confirm or discard; use **Register manual signal** if screening APIs fail

### Compliance (yellow card)
- **Identity** — Verify UBOs/corporate bodies; portal link if pending; **Verified manually (without API)** if the provider is unavailable
- **SAR** — Narrative ≥200 characters or “not applicable” ≥50; if an urgent report fails at UIF, the status remains **Pending** with a manual reference record
- **BdP asset freeze** — After confirming a sanction; if the API fails, manually record the BdP reference in the red alert
- **EDD** — Source of funds + second approver upon approval

### Approve
- The **Approve** button is active only when there is no blocking message
- Supervisor: a second approver is mandatory for EDD

## 3. Troubleshooting

| Symptom | Likely cause | Action |
|---------|--------------|--------|
| Case does not start (PAC) | Prohibited CAE/jurisdiction | Check Settings → PAC; correct the data |
| Approve disabled | Unverified UBO / EDD funds | Compliance section |
| Screening stuck at % | Ollama unavailable | Check `OLLAMA_ENDPOINT`; restart Ollama |
| Identity webhook 401 | Incorrect HMAC | Align `IdentityVerification:WebhookSecret` |
| Report PDF error | Puppeteer/Chromium | `kyc-web` logs; reinstall Docker dependencies |
| SAR fails in production | Missing UIF URL | Configure `Uif:BaseUrl` or manually record it in the SAR section (Pending status) |
| BdP asset freeze failed | `BdpAssetFreeze:BaseUrl` | Record the reference manually after confirming the sanction |
| “Entity {NIF}” name | No RCBE/GLEIF | Correct at start-up (manual company name) or add manual parties |
| Case list empty | Database / migrations | `dotnet ef database update` |
| SignalR without updates | WebSocket proxy | nginx: `Upgrade` headers for `/hubs/` |

## 4. Logs and support

- Application logs: Docker stdout for `kyc-web`
- Audit: Admin → Audit log or `audit_entries` query
- Health: `GET /health`

## 5. APIs (technical team)

See [api/README.md](api/README.md) and Swagger `/swagger`.

## 6. Related documentation

- [ANALISTA_QUICK_START.md](ANALISTA_QUICK_START.md)
- [OPERACOES_E_HOMOLOGACAO.md](OPERACOES_E_HOMOLOGACAO.md)
- [MATRIZ_REQUISITOS_INSTITUCIONAIS.md](MATRIZ_REQUISITOS_INSTITUCIONAIS.md)
