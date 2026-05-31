# E2E UI cenários 2–5 — 20260531-024439

App: http://localhost:5299
Utilizador: admin@kyc.local

| Cenário | Resultado | Evidência |
|---|---------|-----------|
| 2 Identidade | PARCIAL | 06-identidade/*-20260531-024439.png |
| 3 SAR | FALHA | 05-sar-uif/*-20260531-024439.png |
| 4 EDD 4-eyes | OK | 08-audit/*-20260531-024439.png |
| 5 RPB | OK | 04-rpb/*-20260531-024439.png |

## Log
[2026-05-31T01:44:42.064Z] Login OK → http://localhost:5299/dashboard
[2026-05-31T01:44:42.068Z] === Cenário 2 — Identidade ===
[2026-05-31T01:45:16.719Z] ERRO s2: locator.click: Timeout 30000ms exceeded.
Call log:
  - waiting for locator('.party-identity-panel').filter({ hasText: 'UBO Verificado UI' }).getByRole('button', { name: 'Verificar identidade' })

[2026-05-31T01:45:16.898Z] Screenshot: docs\dossier\09-e2e\ui-error-s2-20260531-024439.png
[2026-05-31T01:45:16.899Z] === Cenário 3 — SAR ===
[2026-05-31T01:45:22.403Z] Screenshot: docs\dossier\05-sar-uif\03-sar-modal-narrativa-20260531-024439.png
[2026-05-31T01:45:26.601Z] Screenshot: docs\dossier\05-sar-uif\03-sar-submetido-20260531-024439.png
[2026-05-31T01:45:59.655Z] ERRO s3: locator.click: Timeout 30000ms exceeded.
Call log:
  - waiting for getByRole('button', { name: 'SAR não aplicável' })

[2026-05-31T01:45:59.801Z] Screenshot: docs\dossier\09-e2e\ui-error-s3-20260531-024439.png
[2026-05-31T01:45:59.802Z] === Cenário 4 — EDD 4-eyes ===
[2026-05-31T01:45:59.826Z] Caso EDD: 18714d51-5b15-4a80-8b04-2d0b8be8703c
[2026-05-31T01:46:08.637Z] Screenshot: docs\dossier\08-audit\04-edd-origem-fundos-20260531-024439.png
[2026-05-31T01:46:08.786Z] Screenshot: docs\dossier\08-audit\04-edd-verificacao-presencial-20260531-024439.png
[2026-05-31T01:46:09.863Z] Screenshot: docs\dossier\08-audit\04-edd-segundo-aprovador-20260531-024439.png
[2026-05-31T01:46:14.053Z] Screenshot: docs\dossier\08-audit\04-edd-aprovado-20260531-024439.png
[2026-05-31T01:46:14.055Z] === Cenário 5 — RPB ===
[2026-05-31T01:46:16.695Z] Screenshot: docs\dossier\04-rpb\05-rpb-antes-20260531-024439.png
[2026-05-31T01:46:21.923Z] Screenshot: docs\dossier\04-rpb\05-rpb-gerado-20260531-024439.png
[2026-05-31T01:46:22.026Z] XML BdP exportado (200)
[2026-05-31T01:46:25.223Z] Screenshot: docs\dossier\04-rpb\05-rpb-submetido-20260531-024439.png

## Resultados JSON
```json
{
  "s2": {
    "ok": false,
    "error": "locator.click: Timeout 30000ms exceeded.\nCall log:\n  - waiting for locator('.party-identity-panel').filter({ hasText: 'UBO Verificado UI' }).getByRole('button', { name: 'Verificar identidade' })\n"
  },
  "s3": {
    "ok": false,
    "error": "locator.click: Timeout 30000ms exceeded.\nCall log:\n  - waiting for getByRole('button', { name: 'SAR não aplicável' })\n"
  },
  "s4": {
    "ok": true,
    "caseId": "18714d51-5b15-4a80-8b04-2d0b8be8703c"
  },
  "s5": {
    "ok": true
  }
}
```