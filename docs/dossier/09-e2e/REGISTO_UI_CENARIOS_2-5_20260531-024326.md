# E2E UI cenários 2–5 — 20260531-024326

App: http://localhost:5299
Utilizador: admin@kyc.local

| Cenário | Resultado | Evidência |
|---|---------|-----------|
| 2 Identidade | OK | 06-identidade/*-20260531-024326.png |
| 3 SAR | OK | 05-sar-uif/*-20260531-024326.png |
| 4 EDD 4-eyes | FALHA | 08-audit/*-20260531-024326.png |
| 5 RPB | OK | 04-rpb/*-20260531-024326.png |

## Log
[2026-05-31T01:43:29.575Z] Login OK → http://localhost:5299/dashboard
[2026-05-31T01:43:29.579Z] === Cenário 2 — Identidade ===
[2026-05-31T01:43:39.011Z] Screenshot: docs\dossier\06-identidade\02-verificar-identidade-modal-20260531-024326.png
[2026-05-31T01:43:39.017Z] PartyId webhook: 94c8bda9-c3da-4fef-8929-a92cb0ceae2d, session: local-94
[2026-05-31T01:43:39.849Z] Webhook: 200
[2026-05-31T01:43:43.574Z] Screenshot: docs\dossier\06-identidade\02-badge-verificado-20260531-024326.png
[2026-05-31T01:43:43.581Z] Botão Aprovar desactivado: true
[2026-05-31T01:43:43.724Z] Screenshot: docs\dossier\06-identidade\02-aprovar-bloqueado-ubo-pendente-20260531-024326.png
[2026-05-31T01:43:43.725Z] === Cenário 3 — SAR ===
[2026-05-31T01:43:49.371Z] Screenshot: docs\dossier\05-sar-uif\03-sar-modal-narrativa-20260531-024326.png
[2026-05-31T01:43:53.542Z] Screenshot: docs\dossier\05-sar-uif\03-sar-submetido-20260531-024326.png
[2026-05-31T01:44:01.011Z] Screenshot: docs\dossier\05-sar-uif\03-sar-nao-aplicavel-20260531-024326.png
[2026-05-31T01:44:03.832Z] Screenshot: docs\dossier\05-sar-uif\03-lista-badges-sar-ddc-20260531-024326.png
[2026-05-31T01:44:03.833Z] === Cenário 4 — EDD 4-eyes ===
[2026-05-31T01:44:03.862Z] Caso EDD: 18714d51-5b15-4a80-8b04-2d0b8be8703c
[2026-05-31T01:44:12.662Z] Screenshot: docs\dossier\08-audit\04-edd-origem-fundos-20260531-024326.png
[2026-05-31T01:44:15.833Z] Screenshot: docs\dossier\08-audit\04-edd-verificacao-presencial-20260531-024326.png
[2026-05-31T01:44:16.696Z] ERRO s4: locator.selectOption: options[0].label: expected string, got object
[2026-05-31T01:44:16.837Z] Screenshot: docs\dossier\09-e2e\ui-error-s4-20260531-024326.png
[2026-05-31T01:44:16.838Z] === Cenário 5 — RPB ===
[2026-05-31T01:44:19.464Z] Screenshot: docs\dossier\04-rpb\05-rpb-antes-20260531-024326.png
[2026-05-31T01:44:24.686Z] Screenshot: docs\dossier\04-rpb\05-rpb-gerado-20260531-024326.png
[2026-05-31T01:44:24.775Z] XML BdP exportado (200)
[2026-05-31T01:44:27.976Z] Screenshot: docs\dossier\04-rpb\05-rpb-submetido-20260531-024326.png

## Resultados JSON
```json
{
  "s2": {
    "ok": true,
    "disabled": true
  },
  "s3": {
    "ok": true
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