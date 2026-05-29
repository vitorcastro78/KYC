# Checklist de Homologação BdP — KYC AI Platform

## Lei 83/2017 — AML/CFT
- [ ] PAC versionada activa (`customer_acceptance_policies`)
- [ ] DDC Simplificada/Standard/Reforçada calculada por caso
- [ ] EDD: origem de fundos obrigatória antes de aprovação
- [ ] Revisão periódica (`NextReviewDue`) após aprovação
- [ ] SAR/UIF com audit trail (`SarSubmitted`, referência UIF)

## Aviso BdP 1/2022
- [ ] Verificação identidade remota (API prestador)
- [ ] Bloqueio aprovação se UBO/admin não verificado
- [ ] 4-eyes em EDD (`SecondApproverId`)

## Lei 97/2017 — Congelamento
- [ ] Notificação automática ao confirmar sanção
- [ ] `AssetFreezeNotified` registado

## Instrução BdP 8/2024 — RPB
- [ ] Geração anual `AmlComplianceReport`
- [ ] Export JSON formatado

## RGPD
- [ ] DPIA activa registada
- [ ] Audit trail imutável (trigger PostgreSQL)
- [ ] Auto-approve apenas Low risk
- [ ] Secção explainability no relatório

## Operacional
- [ ] Health check `/health`
- [ ] Secrets fora do repositório
- [ ] Deploy on-prem (`docker-compose.prod.yml`)
- [ ] CI pipeline verde
