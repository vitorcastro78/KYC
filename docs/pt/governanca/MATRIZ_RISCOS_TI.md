# Matriz de avaliação de riscos de TI — KYC AI Platform

> **Versão:** 1.0 · **Revisão:** trimestral (próxima: _[data]_)

Escala: Probabilidade (1–5) × Impacto (1–5) = Exposição (1–25)

| ID | Risco | P | I | Exp. | Controlos | Owner | Estado |
|----|-------|---|---|------|-----------|-------|--------|
| R01 | Falha BD sem backup | 2 | 5 | 10 | PRD + backups diários | TI | 🟡 |
| R02 | Breach dados pessoais | 2 | 5 | 10 | TLS, RBAC, pen test, DPO | CISO | 🟡 |
| R03 | Falso negativo sanções | 3 | 5 | 15 | Listas OFAC/EU, confirmação analista, métricas | Compliance | ✅ |
| R04 | Falso positivo excessivo | 3 | 3 | 9 | Confirmação manual, métricas FP | Compliance | ✅ |
| R05 | API UIF indisponível | 3 | 4 | 12 | Fila SAR + registo manual | Compliance | ✅ |
| R06 | Prestador identidade down | 2 | 4 | 8 | Presencial + polling | Ops | ✅ |
| R07 | LLM alucinação relatório | 2 | 4 | 8 | Revisão humana, explainability Art.22 | Compliance | ✅ |
| R08 | Insider abuse admin | 2 | 5 | 10 | Audit imutável, roles, MFA | CISO | ✅ |
| R09 | Ransomware | 2 | 5 | 10 | Backups offline, PRD | TI | 🟡 |
| R10 | Não conformidade RGPD retenção | 2 | 4 | 8 | DataRetention job 5–7a | DPO | 🟡 |

## Cláusula auditoria BdP (4.3)

Modelo para contratos de subcontratação e SLA:

> A instituição e o Banco de Portugal, nos termos da Lei n.º 83/2017 e demais diploma aplicável, reservam o direito de auditar, diretamente ou através de terceiros mandatados, os sistemas de informação e os registos de tratamento de dados relacionados com o serviço KYC, com pré-aviso razoável de 30 dias úteis, sem prejuízo de auditorias de emergência em caso de incidente grave.

**Estado:** ✅ Texto disponível para inclusão em minutas contratuais.
