# E2E UI cenários 2–5 — 20260531-024650

App: http://localhost:5299
Utilizador: admin@kyc.local

| Cenário | Resultado | Evidência |
|---|---------|-----------|
| 2 Identidade | OK | 06-identidade/*-20260531-024650.png |
| 3 SAR | OK | 05-sar-uif/*-20260531-024650.png |
| 4 EDD 4-eyes | OK | 08-audit/*-20260531-024650.png |
| 5 RPB | OK | 04-rpb/*-20260531-024650.png |

## Log
[2026-05-31T01:46:53.569Z] Login OK → http://localhost:5299/dashboard
[2026-05-31T01:46:53.574Z] === Cenário 2 — Identidade ===
[2026-05-31T01:47:02.990Z] Screenshot: docs\dossier\06-identidade\02-verificar-identidade-modal-20260531-024650.png
[2026-05-31T01:47:02.997Z] PartyId webhook: c30f6531-1398-4935-ab2e-93a9d23fe29f, session: local-c3
[2026-05-31T01:47:03.718Z] Webhook: 200
[2026-05-31T01:47:07.436Z] Screenshot: docs\dossier\06-identidade\02-badge-verificado-20260531-024650.png
[2026-05-31T01:47:07.442Z] Botão Aprovar desactivado: true
[2026-05-31T01:47:07.588Z] Screenshot: docs\dossier\06-identidade\02-aprovar-bloqueado-ubo-pendente-20260531-024650.png
[2026-05-31T01:47:07.589Z] === Cenário 3 — SAR ===
[2026-05-31T01:47:13.064Z] Screenshot: docs\dossier\05-sar-uif\03-sar-modal-narrativa-20260531-024650.png
[2026-05-31T01:47:17.256Z] Screenshot: docs\dossier\05-sar-uif\03-sar-submetido-20260531-024650.png
[2026-05-31T01:47:24.752Z] Screenshot: docs\dossier\05-sar-uif\03-sar-nao-aplicavel-20260531-024650.png
[2026-05-31T01:47:27.604Z] Screenshot: docs\dossier\05-sar-uif\03-lista-badges-sar-ddc-20260531-024650.png
[2026-05-31T01:47:27.605Z] === Cenário 4 — EDD 4-eyes ===
[2026-05-31T01:47:27.639Z] Caso EDD: 58c21877-ec18-4b01-9351-22cefefe6ee9
[2026-05-31T01:47:36.442Z] Screenshot: docs\dossier\08-audit\04-edd-origem-fundos-20260531-024650.png
[2026-05-31T01:47:39.631Z] Screenshot: docs\dossier\08-audit\04-edd-verificacao-presencial-20260531-024650.png
[2026-05-31T01:47:40.653Z] Screenshot: docs\dossier\08-audit\04-edd-segundo-aprovador-20260531-024650.png
[2026-05-31T01:47:44.846Z] Screenshot: docs\dossier\08-audit\04-edd-aprovado-20260531-024650.png
[2026-05-31T01:47:44.849Z] === Cenário 5 — RPB ===
[2026-05-31T01:47:47.476Z] Screenshot: docs\dossier\04-rpb\05-rpb-antes-20260531-024650.png
[2026-05-31T01:47:52.675Z] Screenshot: docs\dossier\04-rpb\05-rpb-gerado-20260531-024650.png
[2026-05-31T01:47:52.756Z] XML BdP exportado (200)
[2026-05-31T01:47:55.953Z] Screenshot: docs\dossier\04-rpb\05-rpb-submetido-20260531-024650.png

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
    "ok": true,
    "caseId": "58c21877-ec18-4b01-9351-22cefefe6ee9"
  },
  "s5": {
    "ok": true
  }
}
```