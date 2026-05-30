# Quick start — Analista AML (KYC Platform)

## 1. Acesso

- URL homologação/prod conforme deploy (`docs/DEPLOY_ONPREM.md`)
- Roles: `KYC.Analyst` (casos), `KYC.Supervisor` (escalação + 4-eyes), `KYC.Admin` (RPB, PAC)

## 2. Novo caso

1. **Casos → Novo** — NIF, montante, tipo de relação (ocasional/continuada), CAE se aplicável.
2. Aguardar triagem automática (barra de progresso no detalhe do caso).
3. Revisar sinais e confirmar/descartar correspondências.

## 3. Conformidade (secção amarela)

- **Identidade** — Verificar UBO/administradores (Aviso BdP 1/2022) antes de aprovar.
- **EDD** — Preencher origem dos fundos; segundo aprovador obrigatório.
- **SAR** — Se banner amarelo: comunicar à UIF (≥200 caracteres) ou marcar não aplicável.
- **RCBE** — Reportar discrepância se detectada.

## 4. Aprovar ou rejeitar

- Botão **Aprovar** só activo quando `CanApprove` não indicar bloqueio.
- Casos com sanção confirmada → congelamento automático + estado «Em revisão».

## 5. Alertas em tempo real

- SignalR: progresso de triagem, relatório pronto, alertas compliance (SAR, identidade, congelamento).
- Supervisores recebem alertas SAR no grupo `supervisors`.

## 6. Referência

- E2E completo: `docs/E2E_HOMOLOGACAO.md`
- Checklist BdP: `docs/CHECKLIST_HOMOLOGACAO_BDP.md`
