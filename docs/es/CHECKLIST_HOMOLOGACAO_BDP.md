# Lista de verificación de homologación BdP — Plataforma de IA KYC

## Ley 83/2017 — AML/CFT
- [x] PAC versionada activa (`customer_acceptance_policies`) — validación en `StartKycCase`
- [x] DDC Simplificada/Estándar/Reforzada calculada por caso
- [x] EDD: origen de fondos obligatorio antes de la aprobación
- [x] Revisión periódica (`NextReviewDue`) tras la aprobación
- [x] SAR/UIF con audit trail (`SarSubmitted`, referencia UIF)
- [x] SAR pendiente + registro manual UIF (`SarApiFailedPendingManual`, `SarManualRegistered`)
- [x] Denominación social manual si RCBE/GLEIF falla (`LegalCompanyName` al inicio)
- [x] Señales de riesgo manuales + confirmación del analista (`AddManualRiskSignalCommand`, `OverrideSignal`)

## Aviso BdP 1/2022
- [x] Verificación de identidad (webhook + polling + UI de métodos)
- [x] Verificación manual de contingencia (`RecordManualIdentityVerificationCommand`)
- [x] Bloqueo de aprobación si UBO/administrador no está verificado
- [x] 4 ojos en EDD (`SecondApproverId`)

## Ley 97/2017 — Congelación de activos
- [x] Notificación automática al confirmar una sanción
- [x] `AssetFreezeNotified` registrado
- [x] Registro manual de referencia BdP si falla la API (`RegisterManualAssetFreezeReferenceCommand`)

## Instrucción BdP 8/2024 — RPB
- [x] Generación anual de `AmlComplianceReport`
- [x] Exportación JSON interna + XML BdP (`?format=bdp`)

## RGPD
- [x] DPIA activa registrada (Admin crea versión)
- [x] Audit trail inmutable (trigger `tr_audit_entries_immutable` en la migration BdP)
- [x] Auto-approve solo para riesgo Low (score ≤30, sin High/Critical/sanciones)
- [x] Sección de explainability en el informe (Art. 22)

## Ejecución de homologación (evidencias)

- [x] Escenarios E2E 1–10 ejecutados (pruebas auto + UI 2–5) — consulte [E2E_HOMOLOGACAO.md](E2E_HOMOLOGACAO.md) §Registro (2026-05-31)
- [x] Dossier completado en `docs/dossier/` (parcial: falta pen test en `10-seguranca/`)
- [x] Capturas PAC en `docs/dossier/01-pac/` — [REGISTO_UI_PAC_20260531-181205.md](dossier/09-e2e/REGISTO_UI_PAC_20260531-181205.md)
- [ ] Pen test — [SECURITY_PEN_TEST_CHECKLIST.md](SECURITY_PEN_TEST_CHECKLIST.md)

_Fecha de homologación:_ 2026-05-31 _Responsable:_ homologación técnica (auto + Playwright UI)

## Operacional
- [x] Health check `/health`
- [x] Secrets fuera del repositorio (`.env` en `.gitignore`, usar `.env.example`)
- [x] Despliegue on-prem documentado (`docker-compose.prod.yml`, `docs/DEPLOY_ONPREM.md`)
- [x] Pipeline CI (`/.github/workflows/ci.yml` — build, migrate, test)
