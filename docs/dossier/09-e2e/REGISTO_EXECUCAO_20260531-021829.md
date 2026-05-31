# Registo E2E automatizado â€” 20260531-021829

| Item | Ficheiro |
|------|----------|
| Resultados testes | docs/dossier/09-e2e/test-results-20260531-021829.trx |
| Export audit/casos | audit-export-20260531-011908.json |
| Audit trail | docs/dossier/08-audit/audit-trail-e2e-*.json |
| HTTP health/OpenAPI | docs/dossier/09-e2e/http-*-20260531-021829.txt |
| Webhook identidade | docs/dossier/06-identidade/webhook-*.txt |
| Testes aplicacao | docs/dossier/09-e2e/application-tests-20260531-021829.log |

Executado: 2026-05-31T02:19:32.8180684+01:00
BD: Host=195.179.193.136;Port=5433;Database=azureopsagent;Username=kycdb;Password=***
App: http://localhost:5299
Exit code testes: 0
| # | Cenario | Resultado | Evidencia |
|---|---------|-----------|-----------|
| 1 | PAC CAE 92000 | OK (teste) | test-results-20260531-021829.trx |
| 6 | Nome legal manual | OK (teste) | audit-export / trx |
| 7 | SAR manual | OK (teste) | 05-sar-uif / trx |
| 8 | Congelamento manual | OK (teste) | 07-congelamento / trx |
| 9 | Identidade manual | OK (teste) | 06-identidade / trx |
| 10 | Sinais manuais | OK (teste) | trx |
| 2-5 | UI/API parcial | HTTP + webhook se app OK | 09-e2e, 06-identidade |
