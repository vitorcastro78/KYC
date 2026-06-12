# Checklist de Homologação BdP — KYC AI Platform

## Lei 83/2017 — AML/CFT
- [x] PAC versionada activa (`customer_acceptance_policies`) — validação no `StartKycCase`
- [x] DDC Simplificada/Standard/Reforçada calculada por caso
- [x] EDD: origem de fundos obrigatória antes de aprovação
- [x] Revisão periódica (`NextReviewDue`) após aprovação
- [x] SAR/UIF com audit trail (`SarSubmitted`, referência UIF)
- [x] SAR pendente + registo manual UIF (`SarApiFailedPendingManual`, `SarManualRegistered`)
- [x] Denominação social manual se RCBE/GLEIF em falha (`LegalCompanyName` no arranque)
- [x] Sinais de risco manuais + confirmação analista (`AddManualRiskSignalCommand`, `OverrideSignal`)

## Aviso BdP 1/2022
- [x] Verificação identidade (webhook + polling + UI métodos)
- [x] Verificação manual de contingência (`RecordManualIdentityVerificationCommand`)
- [x] Bloqueio aprovação se UBO/admin não verificado
- [x] 4-eyes em EDD (`SecondApproverId`)

## Lei 97/2017 — Congelamento
- [x] Notificação automática ao confirmar sanção
- [x] `AssetFreezeNotified` registado
- [x] Registo manual ref. BdP se API falhar (`RegisterManualAssetFreezeReferenceCommand`)

## Instrução BdP 8/2024 — RPB
- [x] Geração anual `AmlComplianceReport`
- [x] Export JSON interno + XML BdP (`?format=bdp`)

## RGPD
- [x] DPIA activa registada (Admin criar versão)
- [x] Audit trail imutável (trigger `tr_audit_entries_immutable` na migration BdP)
- [x] Auto-approve apenas Low risk (score ≤30, sem High/Critical/sanções)
- [x] Secção explainability no relatório (Art. 22)

## Execução homologação (evidências)

- [x] E2E cenários 1–10 executados (testes auto + UI 2–5) — ver [E2E_HOMOLOGACAO.md](E2E_HOMOLOGACAO.md) §Registo (2026-05-31)
- [x] Dossier preenchido em `docs/dossier/` (parcial: falta `10-seguranca/` pen test)
- [x] Prints PAC em `docs/dossier/01-pac/` — [REGISTO_UI_PAC_20260531-181205.md](dossier/09-e2e/REGISTO_UI_PAC_20260531-181205.md)
- [ ] Pen test — [SECURITY_PEN_TEST_CHECKLIST.md](SECURITY_PEN_TEST_CHECKLIST.md)

_Data homologação:_ 2026-05-31 _Responsável:_ homologação técnica (auto + Playwright UI)

## Operacional
- [x] Health check `/health`
- [x] Secrets fora do repositório (`.env` no `.gitignore`, usar `.env.example`)
- [x] Deploy on-prem documentado (`docker-compose.prod.yml`, `docs/DEPLOY_ONPREM.md`)
- [x] CI pipeline (`/.github/workflows/ci.yml` — build, migrate, test)
