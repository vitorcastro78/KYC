# Matriz de requisitos institucionais — KYC AI Platform

> **Última verificação:** Maio 2026 · Commit após implementação governança  
> **Legenda:** ✅ Implementado / documentado · 🟡 Parcial (acção institucional pendente) · 🔴 Pendente · 🌐 Externo

---

## 2.1 Arquitectura e negócio (base)

| # | Requisito | Estado | Evidência / notas |
|---|-----------|--------|-------------------|
| 1.1 | Visão geral, escopo e diagramas de alto nível | ✅ | [DOCUMENTACAO_APLICACAO.md](DOCUMENTACAO_APLICACAO.md) §1–2; fluxos Mermaid no blueprint |
| 1.2 | Documentação técnica APIs (Swagger / OpenAPI) | ✅ | `/swagger`, `/swagger/v1/swagger.json`, `OpenApi:Enable`; [api/README.md](api/README.md) |
| 1.3 | Manuais utilizador e troubleshooting | ✅ | [MANUAL_UTILIZADOR_TROUBLESHOOTING.md](MANUAL_UTILIZADOR_TROUBLESHOOTING.md), [ANALISTA_QUICK_START.md](ANALISTA_QUICK_START.md) |

---

## 2.2 Segurança da informação e cibersegurança

| # | Requisito | Estado | Evidência / notas |
|---|-----------|--------|-------------------|
| 2.1 | Política de segurança da informação escrita e aprovada | 🟡 | Modelo: [governanca/POLITICA_SEGURANCA_INFORMACAO.md](governanca/POLITICA_SEGURANCA_INFORMACAO.md) — **requer assinatura DPO/CISO** |
| 2.2 | Relatório pen test externo | 🟡 | Checklist: [SECURITY_PEN_TEST_CHECKLIST.md](SECURITY_PEN_TEST_CHECKLIST.md); modelo relatório: [governanca/RELATORIO_PEN_TEST_MODELO.md](governanca/RELATORIO_PEN_TEST_MODELO.md) — **executar OWASP ZAP + auditor** |
| 2.3 | Política de criptografia (trânsito e repouso) | ✅ | [governanca/POLITICA_CRIPTOGRAFIA.md](governanca/POLITICA_CRIPTOGRAFIA.md); TLS, HSTS, cookies, PostgreSQL |
| 2.4 | Autenticação forte (MFA/SCA) para operadores | ✅ | Entra ID + Conditional Access (MFA); dev Identity com password policy — ver política §4 |

---

## 2.3 Resiliência e continuidade de negócio

| # | Requisito | Estado | Evidência / notas |
|---|-----------|--------|-------------------|
| 3.1 | PCN estruturado | 🟡 | [governanca/PCN_PLANO_CONTINUIDADE_NEGOCIO.md](governanca/PCN_PLANO_CONTINUIDADE_NEGOCIO.md) — **aprovação COMEX** |
| 3.2 | PRD testado | 🟡 | [governanca/PRD_PLANO_RECUPERACAO_DESASTRES.md](governanca/PRD_PLANO_RECUPERACAO_DESASTRES.md) — **simulação anual pendente** |
| 3.3 | RTO/RPO validados em simulação | 🔴 | [governanca/RTO_RPO_METRICAS.md](governanca/RTO_RPO_METRICAS.md) — tabela alvo + registo simulação vazio |
| 3.4 | Infra multi-region UE SLA 99.9% | 🟡 | [governanca/INFRAESTRUTURA_MULTI_REGION_UE.md](governanca/INFRAESTRUTURA_MULTI_REGION_UE.md) — desenho; **contratação cloud pendente** |

---

## 2.4 Gestão de riscos e subcontratação

| # | Requisito | Estado | Evidência / notas |
|---|-----------|--------|-------------------|
| 4.1 | Matriz riscos TI actualizada | 🟡 | [governanca/MATRIZ_RISCOS_TI.md](governanca/MATRIZ_RISCOS_TI.md) — **revisão trimestral COMEX** |
| 4.2 | Certificações cloud (ISO 27001 / SOC 2) | 🌐 | Responsabilidade fornecedor (Azure/AWS); anexar certificados ao dossier |
| 4.3 | Cláusula auditoria BdP na minuta contratual | ✅ | Referência Lei 83/2017 + audit trail imutável; cláusula tipo em MATRIZ_RISCOS §6 |

---

## 2.5 Conformidade AML/CFT e biometria (KYC)

| # | Requisito | Estado | Evidência / notas |
|---|-----------|--------|-------------------|
| 5.1 | Métricas falsos positivos/negativos motor | ✅ | `GET /api/admin/compliance/metrics` → `ScreeningMetricsDto`; RPB anual |
| 5.2 | Integração listas sanções e PEP | ✅ | OFAC/EU workers, pipeline, UI sinais |
| 5.3 | Prova de vida (Liveness) ISO/IEC 30107-3 | 🟡 | Prestador + `LivenessScore` em `CaseParty`; [governanca/LIVENESS_ISO_30107.md](governanca/LIVENESS_ISO_30107.md) — **certificado prestador** |
| 5.4 | Relatório FAR/FRR biométrico | 🟡 | API métricas + [governanca/METRICAS_BIOMETRIA_FAR_FRR.md](governanca/METRICAS_BIOMETRIA_FAR_FRR.md); FAR=0 até lab prestador |

---

## 2.6 Rastreabilidade e auditoria

| # | Requisito | Estado | Evidência / notas |
|---|-----------|--------|-------------------|
| 6.1 | Relatório KYC consolidado PDF/JSON | ✅ | Relatório HTML/PDF, export RPB JSON/XML |
| 6.2 | Timestamp inviolável (audit trail) | ✅ | `audit_entries` + trigger `tr_audit_entries_immutable` |
| 6.3 | Retenção 5–7 anos (RGPD) | 🟡 | `DataRetentionHostedService` configurável; [governanca/RETENCAO_DADOS_RGPD.md](governanca/RETENCAO_DADOS_RGPD.md) — activar `EnableHostedService` em prod |

---

## Acções institucionais prioritárias

1. Assinar **Política de Segurança** e **PCN/PRD** (COMEX + DPO).
2. Executar **pen test** e preencher relatório → `docs/dossier/10-seguranca/`.
3. Simular **PRD** e registar RTO/RPO em `RTO_RPO_METRICAS.md`.
4. Obter **certificado ISO 30107-3** do prestador de identidade (DigitalSign/CMD).
5. Activar **DataRetention** em produção e validar job diário.

---

## Referências

- [docs/README.md](README.md)
- [CATALOGO_FUNCIONALIDADES.md](CATALOGO_FUNCIONALIDADES.md)
- [OPERACOES_E_HOMOLOGACAO.md](OPERACOES_E_HOMOLOGACAO.md)
