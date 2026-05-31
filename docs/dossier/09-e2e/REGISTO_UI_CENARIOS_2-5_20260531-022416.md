# E2E UI cenários 2–5 — 20260531-022416

App: http://localhost:5299
Utilizador: admin@kyc.local

| Cenário | Resultado | Evidência |
|---|---------|-----------|
| 2 Identidade | PARCIAL | 06-identidade/*-20260531-022416.png |
| 3 SAR | FALHA | 05-sar-uif/*-20260531-022416.png |
| 4 EDD 4-eyes | FALHA | 08-audit/*-20260531-022416.png |
| 5 RPB | FALHA | 04-rpb/*-20260531-022416.png |

## Log
[2026-05-31T01:25:10.315Z] Login OK → http://localhost:5299/dashboard
[2026-05-31T01:25:10.319Z] === Cenário 2 — Identidade ===
[2026-05-31T01:25:43.428Z] ERRO: locator.click: Timeout 30000ms exceeded.
Call log:
  - waiting for getByRole('button', { name: 'Verificar identidade' }).first()

[2026-05-31T01:25:43.577Z] Screenshot: docs\dossier\09-e2e\ui-scenarios-error-20260531-022416.png

## Resultados JSON
```json
{
  "error": "locator.click: Timeout 30000ms exceeded.\nCall log:\n  - waiting for getByRole('button', { name: 'Verificar identidade' }).first()\n"
}
```