# Troubleshooting

Guide for analysts and supervisors. Infrastructure issues (server, database) must be escalated to the IT team with the “For technical support” section at the end.

## Screening and progress

| What you see | Likely cause | What to do |
|--------------|--------------|------------|
| Progress bar stopped for a long time | AI engine (Ollama) or work queue unavailable | Wait 2–3 min.; use **Rerun screening**; if it persists, contact IT |
| Progress does not update but screening runs | WebSocket connection interrupted | Refresh the page (F5); the percentage syncs through the database |
| “Screening failed” on screen | Pipeline error | Ask IT to check the application logs; try re-screening |
| No signals after screening | Entity has no matches or a partial failure | Record manual signal; check parties and NIF |

## Approval and compliance

| What you see | Likely cause | What to do |
|--------------|--------------|------------|
| **Approve** disabled | Blocking message visible | Read the message: pending UBO, EDD funds, etc. |
| “Approval blocked: UBO…” | Identity not verified | Yellow section → Verify each UBO/corporate body |
| Missing second approver | EDD case | Choose a supervisor in the approval modal |
| Name “Entity 123456789” | RCBE/GLEIF did not resolve | Enter company name manually in the new case or correct parties |

## SAR and freezing

| What you see | Likely cause | What to do |
|--------------|--------------|------------|
| UIF submission failed | Integration not configured or unavailable | Record **manual UIF reference** in the SAR section |
| SAR status Pending | Manual communication recorded | Normal in contingency — archive the official reference |
| Red freezing alert | BdP API failed | Record **manual BdP reference** after confirming the sanction |

## Reports and PDF

| What you see | Likely cause | What to do |
|--------------|--------------|------------|
| “Report unavailable” | Screening not completed | Wait for 100% or rerun screening |
| PDF does not open / error | PDF conversion service on the server | Use the HTML report; escalate to IT |
| Strange text in report | Invalid response from AI engine | Rerun screening; IT can disable LLM enrichment |

## Cases and lists

| What you see | Likely cause | What to do |
|--------------|--------------|------------|
| Empty case list | No data or database connection | Confirm with IT that the database is available |
| Case not created after form | PAC rejection | CAE/jurisdiction — Admin → Settings → PAC |
| Access denied after login | Missing role | Ask the administrator for `KYC.Analyst` |

## Documents

| What you see | Likely cause | What to do |
|--------------|--------------|------------|
| Document in “Failed” | Corrupted format or OCR unavailable | Re-send the file; prefer searchable PDF |
| Long time in “Processing” | Ingestion queue full | Wait; rerun screening after “Completed” |

## Identity and webhook

| What you see | Likely cause | What to do |
|--------------|--------------|------------|
| Empty verification link | Provider not configured | Use **Manually verified** |
| Status does not update after portal | Webhook not received | IT: check secret and URL; use manual verification |

---

## For technical support (IT)

| Symptom | Check |
|---------|-------|
| Ollama / scoring | `OLLAMA_ENDPOINT` variable, Ollama service active |
| Database | PostgreSQL, migrations applied |
| SignalR behind proxy | `Upgrade` and `Connection` headers on `/hubs/` |
| PDF | Chromium/Puppeteer in the `kyc-web` container |
| Health | `GET /health` on the instance |
| Audit | `audit_entries` table, Admin → Audit log screen |

Application logs: container stdout or the **kyc-web** service.
