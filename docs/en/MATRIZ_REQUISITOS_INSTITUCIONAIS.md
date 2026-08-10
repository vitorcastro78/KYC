# Institutional requirements matrix — KYC AI Platform

> **Last verification:** May 2026 · Commit after governance implementation  
> **Legend:** ✅ Implemented / documented · 🟡 Partial (institutional action pending) · 🔴 Pending · 🌐 External

---

## 2.1 Architecture and business (baseline)

| # | Requirement | Status | Evidence / notes |
|---|-------------|--------|------------------|
| 1.1 | Overview, scope, and high-level diagrams | ✅ | [DOCUMENTACAO_APLICACAO.md](DOCUMENTACAO_APLICACAO.md) §1–2; Mermaid flows in the application documentation |
| 1.2 | API technical documentation (Swagger / OpenAPI) | ✅ | `/swagger`, `/swagger/v1/swagger.json`, `OpenApi:Enable`; [api/README.md](api/README.md) |
| 1.3 | User manuals and troubleshooting | ✅ | [../help-online/en/](../help-online/en/), [OPERACOES_E_HOMOLOGACAO.md](OPERACOES_E_HOMOLOGACAO.md) §8 |

---

## 2.2 Information security and cybersecurity

| # | Requirement | Status | Evidence / notes |
|---|-------------|--------|------------------|
| 2.1 | Written and approved information-security policy | 🟡 | Template: [governanca/POLITICA_SEGURANCA_INFORMACAO.md](governanca/POLITICA_SEGURANCA_INFORMACAO.md) — **requires DPO/CISO signature** |
| 2.2 | External pen-test report | 🟡 | Checklist: [OPERACOES_E_HOMOLOGACAO.md](OPERACOES_E_HOMOLOGACAO.md) §6; report template: [governanca/RELATORIO_PEN_TEST_MODELO.md](governanca/RELATORIO_PEN_TEST_MODELO.md) — **run OWASP ZAP + auditor** |
| 2.3 | Encryption policy (in transit and at rest) | ✅ | [governanca/POLITICA_CRIPTOGRAFIA.md](governanca/POLITICA_CRIPTOGRAFIA.md); TLS, HSTS, cookies, PostgreSQL |
| 2.4 | Strong authentication (MFA/SCA) for operators | ✅ | Entra ID + Conditional Access (MFA); dev Identity with password policy — see policy §4 |

---

## 2.3 Resilience and business continuity

| # | Requirement | Status | Evidence / notes |
|---|-------------|--------|------------------|
| 3.1 | Structured BCP | 🟡 | [governanca/PCN_PLANO_CONTINUIDADE_NEGOCIO.md](governanca/PCN_PLANO_CONTINUIDADE_NEGOCIO.md) — **COMEX approval** |
| 3.2 | Tested DRP | 🟡 | [governanca/PRD_PLANO_RECUPERACAO_DESASTRES.md](governanca/PRD_PLANO_RECUPERACAO_DESASTRES.md) — **annual simulation pending** |
| 3.3 | RTO/RPO validated in simulation | 🔴 | [governanca/RTO_RPO_METRICAS.md](governanca/RTO_RPO_METRICAS.md) — target table + empty simulation record |
| 3.4 | EU multi-region infrastructure, 99.9% SLA | 🟡 | [governanca/INFRAESTRUTURA_MULTI_REGION_UE.md](governanca/INFRAESTRUTURA_MULTI_REGION_UE.md) — design; **cloud procurement pending** |

---

## 2.4 Risk management and subcontracting

| # | Requirement | Status | Evidence / notes |
|---|-------------|--------|------------------|
| 4.1 | Updated IT risk matrix | 🟡 | [governanca/MATRIZ_RISCOS_TI.md](governanca/MATRIZ_RISCOS_TI.md) — **quarterly COMEX review** |
| 4.2 | Cloud certifications (ISO 27001 / SOC 2) | 🌐 | Provider responsibility (Azure/AWS); attach certificates to the dossier |
| 4.3 | BdP audit clause in contract template | ✅ | Law 83/2017 reference + immutable audit trail; template clause in MATRIZ_RISCOS §6 |

---

## 2.5 AML/CFT and biometrics compliance (KYC)

| # | Requirement | Status | Evidence / notes |
|---|-------------|--------|------------------|
| 5.1 | Engine false-positive/negative metrics | ✅ | `GET /api/admin/compliance/metrics` → `ScreeningMetricsDto`; annual RPB |
| 5.2 | Sanction and PEP-list integration | ✅ | OFAC/EU workers, pipeline, UI signals |
| 5.3 | Liveness proof ISO/IEC 30107-3 | 🟡 | Provider + `LivenessScore` in `CaseParty`; [governanca/LIVENESS_ISO_30107.md](governanca/LIVENESS_ISO_30107.md) — **provider certificate** |
| 5.4 | Biometric FAR/FRR report | 🟡 | Metrics API + [governanca/METRICAS_BIOMETRIA_FAR_FRR.md](governanca/METRICAS_BIOMETRIA_FAR_FRR.md); FAR=0 until provider lab |
| 5.5 | Manual contingency (APIs unavailable) | ✅ | BdP asset freeze, UIF SAR, identity, signals, legal name at start — [OPERACOES_E_HOMOLOGACAO.md](OPERACOES_E_HOMOLOGACAO.md) §4 (scenarios 6–9), [../help-online/en/07-resolucao-problemas.md](../help-online/en/07-resolucao-problemas.md) |

---

## 2.6 Traceability and audit

| # | Requirement | Status | Evidence / notes |
|---|-------------|--------|------------------|
| 6.1 | Consolidated KYC report PDF/JSON | ✅ | HTML/PDF report, JSON/XML RPB export |
| 6.2 | Tamper-proof timestamp (audit trail) | ✅ | `audit_entries` + `tr_audit_entries_immutable` trigger |
| 6.3 | 5–7-year retention (GDPR) | 🟡 | Configurable `DataRetentionHostedService`; [governanca/RETENCAO_DADOS_RGPD.md](governanca/RETENCAO_DADOS_RGPD.md) — enable `EnableHostedService` in prod |

---

## Priority institutional actions

1. Sign the **Security Policy** and **BCP/DRP** (COMEX + DPO).
2. Run the **pen test** and complete the report → `docs/dossier/10-seguranca/`.
3. Simulate the **DRP** and record RTO/RPO in `RTO_RPO_METRICAS.md`.
4. Obtain the **ISO 30107-3 certificate** from the identity provider (DigitalSign/CMD).
5. Enable **DataRetention** in production and validate the daily job.

---

## References

- [docs/README.md](README.md)
- [CATALOGO_FUNCIONALIDADES.md](CATALOGO_FUNCIONALIDADES.md)
- [OPERACOES_E_HOMOLOGACAO.md](OPERACOES_E_HOMOLOGACAO.md)
