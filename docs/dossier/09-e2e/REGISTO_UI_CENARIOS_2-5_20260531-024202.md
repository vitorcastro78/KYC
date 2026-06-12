# E2E UI cenários 2–5 — 20260531-024202

App: http://localhost:5299
Utilizador: admin@kyc.local

| Cenário | Resultado | Evidência |
|---|---------|-----------|
| 2 Identidade | PARCIAL | 06-identidade/*-20260531-024202.png |
| 3 SAR | FALHA | 05-sar-uif/*-20260531-024202.png |
| 4 EDD 4-eyes | FALHA | 08-audit/*-20260531-024202.png |
| 5 RPB | OK | 04-rpb/*-20260531-024202.png |

## Log
[2026-05-31T01:42:04.934Z] Login OK → http://localhost:5299/dashboard
[2026-05-31T01:42:04.938Z] === Cenário 2 — Identidade ===
[2026-05-31T01:42:10.129Z] ERRO s2: locator.selectOption: options[0].label: expected string, got object
[2026-05-31T01:42:10.301Z] Screenshot: docs\dossier\09-e2e\ui-error-s2-20260531-024202.png
[2026-05-31T01:42:10.301Z] === Cenário 3 — SAR ===
[2026-05-31T01:42:55.471Z] ERRO s3: page.waitForFunction: Timeout 30000ms exceeded.
[2026-05-31T01:42:55.620Z] Screenshot: docs\dossier\09-e2e\ui-error-s3-20260531-024202.png
[2026-05-31T01:42:55.621Z] === Cenário 4 — EDD 4-eyes ===
[2026-05-31T01:42:55.648Z] Caso EDD: 18714d51-5b15-4a80-8b04-2d0b8be8703c
[2026-05-31T01:43:04.507Z] Screenshot: docs\dossier\08-audit\04-edd-origem-fundos-20260531-024202.png
[2026-05-31T01:43:04.963Z] ERRO s4: locator.selectOption: options[0].label: expected string, got object
[2026-05-31T01:43:05.142Z] Screenshot: docs\dossier\09-e2e\ui-error-s4-20260531-024202.png
[2026-05-31T01:43:05.143Z] === Cenário 5 — RPB ===
[2026-05-31T01:43:07.783Z] Screenshot: docs\dossier\04-rpb\05-rpb-antes-20260531-024202.png
[2026-05-31T01:43:12.975Z] Screenshot: docs\dossier\04-rpb\05-rpb-gerado-20260531-024202.png
[2026-05-31T01:43:13.076Z] XML BdP exportado (200)
[2026-05-31T01:43:16.253Z] Screenshot: docs\dossier\04-rpb\05-rpb-submetido-20260531-024202.png

## Resultados JSON
```json
{
  "s2": {
    "ok": false,
    "error": "locator.selectOption: options[0].label: expected string, got object"
  },
  "s3": {
    "ok": false,
    "error": "page.waitForFunction: Timeout 30000ms exceeded."
  },
  "s4": {
    "ok": false,
    "error": "locator.selectOption: options[0].label: expected string, got object"
  },
  "s5": {
    "ok": true
  }
}
```