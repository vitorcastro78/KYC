# Matriz de requisitos institucionales — Plataforma de IA KYC

> **Última verificación:** mayo de 2026 · Commit posterior a la implementación de gobernanza  
> **Leyenda:** ✅ Implementado / documentado · 🟡 Parcial (acción institucional pendiente) · 🔴 Pendiente · 🌐 Externo

---

## 2.1 Arquitectura y negocio (base)

| # | Requisito | Estado | Evidencia / notas |
|---|-----------|--------|-------------------|
| 1.1 | Visión general, alcance y diagramas de alto nivel | ✅ | [DOCUMENTACAO_APLICACAO.md](DOCUMENTACAO_APLICACAO.md) §1–2; flujos Mermaid en la documentación de la aplicación |
| 1.2 | Documentación técnica de APIs (Swagger / OpenAPI) | ✅ | `/swagger`, `/swagger/v1/swagger.json`, `OpenApi:Enable`; [api/README.md](api/README.md) |
| 1.3 | Manuales de usuario y resolución de problemas | ✅ | [../help-online/es/](../help-online/es/), [OPERACOES_E_HOMOLOGACAO.md](OPERACOES_E_HOMOLOGACAO.md) §8 |

---

## 2.2 Seguridad de la información y ciberseguridad

| # | Requisito | Estado | Evidencia / notas |
|---|-----------|--------|-------------------|
| 2.1 | Política de seguridad de la información escrita y aprobada | 🟡 | Modelo: [governanca/POLITICA_SEGURANCA_INFORMACAO.md](governanca/POLITICA_SEGURANCA_INFORMACAO.md) — **requiere firma DPO/CISO** |
| 2.2 | Informe externo de pen test | 🟡 | Checklist: [OPERACOES_E_HOMOLOGACAO.md](OPERACOES_E_HOMOLOGACAO.md) §6; modelo de informe: [governanca/RELATORIO_PEN_TEST_MODELO.md](governanca/RELATORIO_PEN_TEST_MODELO.md) — **ejecutar OWASP ZAP + auditor** |
| 2.3 | Política de cifrado (en tránsito y en reposo) | ✅ | [governanca/POLITICA_CRIPTOGRAFIA.md](governanca/POLITICA_CRIPTOGRAFIA.md); TLS, HSTS, cookies, PostgreSQL |
| 2.4 | Autenticación fuerte (MFA/SCA) para operadores | ✅ | Entra ID + Conditional Access (MFA); dev Identity con política de contraseña — ver política §4 |

---

## 2.3 Resiliencia y continuidad de negocio

| # | Requisito | Estado | Evidencia / notas |
|---|-----------|--------|-------------------|
| 3.1 | PCN estructurado | 🟡 | [governanca/PCN_PLANO_CONTINUIDADE_NEGOCIO.md](governanca/PCN_PLANO_CONTINUIDADE_NEGOCIO.md) — **aprobación COMEX** |
| 3.2 | PRD probado | 🟡 | [governanca/PRD_PLANO_RECUPERACAO_DESASTRES.md](governanca/PRD_PLANO_RECUPERACAO_DESASTRES.md) — **simulación anual pendiente** |
| 3.3 | RTO/RPO validados en simulación | 🔴 | [governanca/RTO_RPO_METRICAS.md](governanca/RTO_RPO_METRICAS.md) — tabla objetivo + registro de simulación vacío |
| 3.4 | Infraestructura multi-región UE, SLA 99.9% | 🟡 | [governanca/INFRAESTRUTURA_MULTI_REGION_UE.md](governanca/INFRAESTRUTURA_MULTI_REGION_UE.md) — diseño; **contratación cloud pendiente** |

---

## 2.4 Gestión de riesgos y subcontratación

| # | Requisito | Estado | Evidencia / notas |
|---|-----------|--------|-------------------|
| 4.1 | Matriz de riesgos TI actualizada | 🟡 | [governanca/MATRIZ_RISCOS_TI.md](governanca/MATRIZ_RISCOS_TI.md) — **revisión trimestral COMEX** |
| 4.2 | Certificaciones cloud (ISO 27001 / SOC 2) | 🌐 | Responsabilidad del proveedor (Azure/AWS); adjuntar certificados al dossier |
| 4.3 | Cláusula de auditoría BdP en el modelo contractual | ✅ | Referencia a Ley 83/2017 + audit trail inmutable; cláusula modelo en MATRIZ_RISCOS §6 |

---

## 2.5 Conformidad AML/CFT y biometría (KYC)

| # | Requisito | Estado | Evidencia / notas |
|---|-----------|--------|-------------------|
| 5.1 | Métricas de falsos positivos/negativos del motor | ✅ | `GET /api/admin/compliance/metrics` → `ScreeningMetricsDto`; RPB anual |
| 5.2 | Integración de listas de sanciones y PEP | ✅ | Workers OFAC/UE, pipeline, señales UI |
| 5.3 | Prueba de vida (Liveness) ISO/IEC 30107-3 | 🟡 | Proveedor + `LivenessScore` en `CaseParty`; [governanca/LIVENESS_ISO_30107.md](governanca/LIVENESS_ISO_30107.md) — **certificado del proveedor** |
| 5.4 | Informe FAR/FRR biométrico | 🟡 | API de métricas + [governanca/METRICAS_BIOMETRIA_FAR_FRR.md](governanca/METRICAS_BIOMETRIA_FAR_FRR.md); FAR=0 hasta laboratorio del proveedor |
| 5.5 | Contingencia manual (APIs no disponibles) | ✅ | Congelación BdP, SAR UIF, identidad, señales y denominación al inicio — [OPERACOES_E_HOMOLOGACAO.md](OPERACOES_E_HOMOLOGACAO.md) §4 (escenarios 6–9), [../help-online/es/07-resolucao-problemas.md](../help-online/es/07-resolucao-problemas.md) |

---

## 2.6 Trazabilidad y auditoría

| # | Requisito | Estado | Evidencia / notas |
|---|-----------|--------|-------------------|
| 6.1 | Informe KYC consolidado PDF/JSON | ✅ | Informe HTML/PDF, exportación RPB JSON/XML |
| 6.2 | Timestamp inviolable (audit trail) | ✅ | `audit_entries` + trigger `tr_audit_entries_immutable` |
| 6.3 | Retención de 5–7 años (RGPD) | 🟡 | `DataRetentionHostedService` configurable; [governanca/RETENCAO_DADOS_RGPD.md](governanca/RETENCAO_DADOS_RGPD.md) — activar `EnableHostedService` en prod |

---

## Acciones institucionales prioritarias

1. Firmar la **Política de Seguridad** y el **PCN/PRD** (COMEX + DPO).
2. Ejecutar el **pen test** y completar el informe → `docs/dossier/10-seguranca/`.
3. Simular el **PRD** y registrar RTO/RPO en `RTO_RPO_METRICAS.md`.
4. Obtener el **certificado ISO 30107-3** del proveedor de identidad (DigitalSign/CMD).
5. Activar **DataRetention** en producción y validar el trabajo diario.

---

## Referencias

- [docs/README.md](README.md)
- [CATALOGO_FUNCIONALIDADES.md](CATALOGO_FUNCIONALIDADES.md)
- [OPERACOES_E_HOMOLOGACAO.md](OPERACOES_E_HOMOLOGACAO.md)
