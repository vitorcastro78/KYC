# E2E UI cenários 2–5 — 20260531-022633

App: http://localhost:5299
Utilizador: admin@kyc.local

| Cenário | Resultado | Evidência |
|---|---------|-----------|
| 2 Identidade | PARCIAL | 06-identidade/*-20260531-022633.png |
| 3 SAR | FALHA | 05-sar-uif/*-20260531-022633.png |
| 4 EDD 4-eyes | FALHA | 08-audit/*-20260531-022633.png |
| 5 RPB | OK | 04-rpb/*-20260531-022633.png |

## Log
[2026-05-31T01:26:36.424Z] Login OK → http://localhost:5299/dashboard
[2026-05-31T01:26:36.427Z] === Cenário 2 — Identidade ===
[2026-05-31T01:27:10.712Z] ERRO s2: locator.fill: Timeout 30000ms exceeded.
Call log:
  - waiting for locator('.modal.show').locator('input.form-control').first()

[2026-05-31T01:27:10.895Z] Screenshot: docs\dossier\09-e2e\ui-error-s2-20260531-022633.png
[2026-05-31T01:27:10.896Z] === Cenário 3 — SAR ===
[2026-05-31T01:27:15.779Z] Screenshot: docs\dossier\05-sar-uif\03-sar-modal-narrativa-20260531-022633.png
[2026-05-31T01:27:45.799Z] ERRO s3: locator.click: Timeout 30000ms exceeded.
Call log:
  - waiting for getByRole('button', { name: 'Submeter SAR' })
    - locator resolved to <button disabled type="button" class="btn btn-danger">Submeter SAR</button>
  - attempting click action
    2 × waiting for element to be visible, enabled and stable
      - element is not enabled
    - retrying click action
    - waiting 20ms
    2 × waiting for element to be visible, enabled and stable
      - element is not enabled
    - retrying click action
      - waiting 100ms
    56 × waiting for element to be visible, enabled and stable
       - element is not enabled
     - retrying click action
       - waiting 500ms

[2026-05-31T01:27:45.994Z] Screenshot: docs\dossier\09-e2e\ui-error-s3-20260531-022633.png
[2026-05-31T01:27:45.995Z] === Cenário 4 — EDD 4-eyes ===
[2026-05-31T01:32:50.166Z] ERRO s4: page.waitForURL: Timeout 300000ms exceeded.
=========================== logs ===========================
waiting for navigation until "load"
============================================================
[2026-05-31T01:32:50.248Z] Screenshot: docs\dossier\09-e2e\ui-error-s4-20260531-022633.png
[2026-05-31T01:32:50.249Z] === Cenário 5 — RPB ===
[2026-05-31T01:32:52.908Z] Screenshot: docs\dossier\04-rpb\05-rpb-antes-20260531-022633.png
[2026-05-31T01:32:58.152Z] Screenshot: docs\dossier\04-rpb\05-rpb-gerado-20260531-022633.png
[2026-05-31T01:32:58.268Z] XML BdP exportado (200)
[2026-05-31T01:33:01.442Z] Screenshot: docs\dossier\04-rpb\05-rpb-submetido-20260531-022633.png

## Resultados JSON
```json
{
  "s2": {
    "ok": false,
    "error": "locator.fill: Timeout 30000ms exceeded.\nCall log:\n  - waiting for locator('.modal.show').locator('input.form-control').first()\n"
  },
  "s3": {
    "ok": false,
    "error": "locator.click: Timeout 30000ms exceeded.\nCall log:\n  - waiting for getByRole('button', { name: 'Submeter SAR' })\n    - locator resolved to <button disabled type=\"button\" class=\"btn btn-danger\">Submeter SAR</button>\n  - attempting click action\n    2 × waiting for element to be visible, enabled and stable\n      - element is not enabled\n    - retrying click action\n    - waiting 20ms\n    2 × waiting for element to be visible, enabled and stable\n      - element is not enabled\n    - retrying click action\n      - waiting 100ms\n    56 × waiting for element to be visible, enabled and stable\n       - element is not enabled\n     - retrying click action\n       - waiting 500ms\n"
  },
  "s4": {
    "ok": false,
    "error": "page.waitForURL: Timeout 300000ms exceeded.\n=========================== logs ===========================\nwaiting for navigation until \"load\"\n============================================================"
  },
  "s5": {
    "ok": true
  }
}
```