# Checklist de Homologação BdP — KYC AI Platform

## Lei 83/2017 — AML/CFT
- [x] PAC versionada activa (`customer_acceptance_policies`) — validação no `StartKycCase`
- [x] DDC Simplificada/Standard/Reforçada calculada por caso
- [x] EDD: origem de fundos obrigatória antes de aprovação
- [x] Revisão periódica (`NextReviewDue`) após aprovação
- [x] SAR/UIF com audit trail (`SarSubmitted`, referência UIF)

## Aviso BdP 1/2022
- [x] Verificação identidade (webhook + polling + UI métodos)
- [x] Bloqueio aprovação se UBO/admin não verificado
- [x] 4-eyes em EDD (`SecondApproverId`)

## Lei 97/2017 — Congelamento
- [x] Notificação automática ao confirmar sanção
- [x] `AssetFreezeNotified` registado

## Instrução BdP 8/2024 — RPB
- [x] Geração anual `AmlComplianceReport`
- [x] Export JSON interno + XML BdP (`?format=bdp`)

## RGPD
- [x] DPIA activa registada (Admin criar versão)
- [x] Audit trail imutável (trigger `tr_audit_entries_immutable` na migration BdP)
- [x] Auto-approve apenas Low risk (score ≤30, sem High/Critical/sanções)
- [x] Secção explainability no relatório (Art. 22)

## Operacional
- [x] Health check `/health`
- [x] Secrets fora do repositório (`.env` no `.gitignore`, usar `.env.example`)
- [x] Deploy on-prem documentado (`docker-compose.prod.yml`, `docs/DEPLOY_ONPREM.md`)
- [x] CI pipeline (`/.github/workflows/ci.yml` — build, migrate, test)
