# Catálogo de Funcionalidades — KYC AI Platform

> **Uso:** base para manuais de utilizador, RFP, homologação funcional e roadmaps.  
> **Legenda estado:** ✅ Implementado · 🟡 Parcial / depende de integração · 🔴 Planeado / não implementado · 🌐 Dependência externa

---

## Módulo 1 — Gestão de casos KYC

| ID | Funcionalidade | Descrição | UI / API | Estado | Base legal / nota |
|----|----------------|-----------|----------|--------|-------------------|
| KYC-01 | Abertura de caso | NIF, montante, relação ocasional/continuada, CAE | `/cases/new` | ✅ | Lei 83/2017 |
| KYC-02 | Validação PAC no arranque | Rejeita CAE/jurisdição proibida antes de gravar | Command handler | ✅ | Art. 24.º |
| KYC-03 | Lista de casos | Score, DDC, badge SAR, estado, data | `/cases` | ✅ | |
| KYC-04 | Detalhe do caso | Hero, progresso triagem, acções, partes | `/cases/{id}` | ✅ | |
| KYC-05 | Aprovar caso | Bloqueio por `CanApproveMessage` | CaseDetail + Supervisor | ✅ | Aviso 1/2022 |
| KYC-06 | Rejeitar caso | Motivo obrigatório | CaseDetail | ✅ | |
| KYC-07 | Pedir revisão manual | Estado UnderReview | CaseDetail | ✅ | |
| KYC-08 | Auto-approve Low risk | Score ≤30, sem sinais graves | Pipeline | ✅ | RGPD / política |
| KYC-09 | Atribuição analista | `AssignedAnalystId` | Domínio | ✅ | |
| KYC-10 | Revisão periódica | `NextReviewDue` após aprovação | Domínio + compliance | ✅ | Art. 35.º |

---

## Módulo 2 — Resolução de entidades e UBO

| ID | Funcionalidade | Descrição | UI / API | Estado | Base legal / nota |
|----|----------------|-----------|----------|--------|-------------------|
| ENT-01 | Resolução GLEIF | Snapshot empresa + partes relacionadas | GleifCompanyCard | ✅ | |
| ENT-02 | RCBE | Validação registo beneficiários | Infraestrutura | 🟡 | Lei 89/2017 |
| ENT-03 | Grafo UBO (backend) | `BuildUboGraphAsync` recursivo | Query | ✅ | |
| ENT-04 | Grafo UBO (UI rico) | Layout hierárquico, zoom, inspector, tabela, flags PEP | `/cases/{id}/ubo`, embed CaseDetail | ✅ | Maio 2026 |
| ENT-05 | Merge grafo + caso | Partes do caso + GLEIF, `CasePartyId` | `UboGraphViewBuilder` | ✅ | |
| ENT-06 | Adicionar parte manual | UBO, accionista, órgão social, procurador | CaseDetail modal | ✅ | |
| ENT-07 | Detalhe da parte | Triagem, identidade, sinais | `/cases/{id}/parties/{id}` | ✅ | |
| ENT-08 | Reportar discrepância RCBE | Botão + audit | PartyIdentityPanel | ✅ | IRN |

---

## Módulo 3 — Triagem e sinais de risco

| ID | Funcionalidade | Descrição | UI / API | Estado | Base legal / nota |
|----|----------------|-----------|----------|--------|-------------------|
| SCR-01 | Pipeline automático | Sanções, media, AT, judicial, ICIJ, scoring | Workers + MediatR | ✅ | |
| SCR-02 | Progresso em tempo real | SignalR + barra de progresso | CaseDetail | ✅ | |
| SCR-03 | Refazer triagem | Todas as partes, regen relatório | CaseDetail | ✅ | |
| SCR-04 | Triagem por parte | Screen individual | EntityCard / PartyDetail | ✅ | |
| SCR-05 | Listas OFAC / EU | Download e índice local | Workers | ✅ | |
| SCR-06 | Confirmação de sinais | Analista confirma correspondência | SignalCard | ✅ | |
| SCR-07 | Congelamento por sanção | Notificação BdP + UnderReview | Pipeline | ✅ | Lei 97/2017 |
| SCR-08 | Scoring Ollama | 0–100 + nível | RiskScoreBadge | ✅ | Sem Claude |
| SCR-09 | DDC automática | Simplified / Standard / Enhanced | Compliance section | ✅ | Aviso 1/2022 |
| SCR-10 | Adverse media janela | 2 anos / 5 anos EDD | Pipeline | ✅ | |

---

## Módulo 4 — Relatórios e explainability

| ID | Funcionalidade | Descrição | UI / API | Estado | Base legal / nota |
|----|----------------|-----------|----------|--------|-------------------|
| RPT-01 | Relatório narrativo 8 secções | LLM Ollama | `/cases/{id}/report` | ✅ | |
| RPT-02 | Secção Art. 22 GDPR | Explainability no prompt | Relatório | ✅ | RGPD |
| RPT-03 | Export PDF | Puppeteer | `/api/cases/{id}/report.pdf` | ✅ | |
| RPT-04 | Embeddings pgvector | Pesquisa semântica relatório | Infra | ✅ | |
| RPT-05 | Consistência documentos | Checker vs GLEIF/caso | Ingestão | ✅ | |
| RPT-06 | Claude API narrativa | Roteamento cloud | — | 🔴 | Desvio intencional |

---

## Módulo 5 — Ingestão de documentos

| ID | Funcionalidade | Descrição | UI / API | Estado | Base legal / nota |
|----|----------------|-----------|----------|--------|-------------------|
| DOC-01 | Upload documentos | PDF, DOCX, imagens | CaseDetail | ✅ | |
| DOC-02 | Pipeline assíncrono | Channel + hosted service | Background | ✅ | |
| DOC-03 | Extração PDF/DOCX | PdfPig, OpenXML | Infra | ✅ | |
| DOC-04 | Extração imagem | Qwen visão | Infra | ✅ | |
| DOC-05 | Facts e parties na BD | Tabelas estruturadas | BD | ✅ | |
| DOC-06 | Re-triagem pós-ingestão | Command | ✅ | |
| DOC-07 | Azure Blob Storage | Armazenamento cloud | — | 🔴 | Blueprint fase 2 |
| DOC-08 | Azure Document Intelligence | OCR cloud | — | 🔴 | |

---

## Módulo 6 — Conformidade BdP (UI e regras)

| ID | Funcionalidade | Descrição | UI / API | Estado | Base legal / nota |
|----|----------------|-----------|----------|--------|-------------------|
| CMP-01 | Secção conformidade | Card amarelo no caso | ComplianceCaseSection | ✅ | |
| CMP-02 | Badge SAR no hero | Estado comunicação UIF | CaseDetail | ✅ | |
| CMP-03 | Banner SAR sugerido | Pipeline `SuggestSar` | CaseDetail + compliance | ✅ | |
| CMP-04 | Modal SAR | Narrativa ≥200, urgente | SarActionModals | ✅ | Art. 52.º–57.º |
| CMP-05 | SAR não aplicável | Justificação ≥50 | SarActionModals | ✅ | |
| CMP-06 | SAR urgente síncrono | Sem fila | Handler | ✅ | |
| CMP-07 | SAR não urgente | Fila assíncrona | Handler | ✅ | |
| CMP-08 | Registo manual UIF | Quando API indisponível | Compliance section | ✅ | |
| CMP-09 | Consulta estado UIF | Por referência | Botão + query | ✅ | 🌐 API UIF |
| CMP-10 | Histórico SAR | Tabela audit | Compliance section | ✅ | |
| CMP-11 | Verificação identidade | Modal 4 métodos | PartyIdentityPanel | ✅ | Aviso 1/2022 |
| CMP-12 | Badge identidade PT | Verificado/Pendente/… | EntityCard, badges | ✅ | |
| CMP-13 | Link sessão verificação | URL prestador | Party panel | ✅ | |
| CMP-14 | Webhook identidade | HMAC POST | `/api/identity/webhook` | ✅ | |
| CMP-15 | Polling identidade | Fallback hosted service | Workers/Web | ✅ | |
| CMP-16 | Bloqueio aprovação identidade | `CanApproveMessage` | UI + domínio | ✅ | |
| CMP-17 | Origem fundos EDD | Textarea + comando | Compliance | ✅ | |
| CMP-18 | 4-eyes EDD | Dropdown supervisores Entra Graph | Approve dialog | ✅ | |
| CMP-19 | Alerta congelamento | Banner vermelho | Compliance | ✅ | Lei 97/2017 |
| CMP-20 | Integrações live prod | `RequireLiveIntegrations` | Config | ✅ | |
| CMP-21 | Submissão UIF real | HTTP + Polly | Infra | 🟡 | 🌐 credenciais |
| CMP-22 | Notificação BdP real | HTTP freeze | Infra | 🟡 | 🌐 endpoint |

---

## Módulo 7 — Administração e governança

| ID | Funcionalidade | Descrição | UI / API | Estado | Base legal / nota |
|----|----------------|-----------|----------|--------|-------------------|
| ADM-01 | PAC — versões | Criar/activar, imutabilidade | `/admin/settings` | ✅ | |
| ADM-02 | Motor scoring — versões | Prompt hash, semver | Settings | ✅ | |
| ADM-03 | DPIA — versões | Upload PDF, activa | `/admin/dpia` | ✅ | RGPD |
| ADM-04 | RPB anual | Gerar draft, métricas | `/admin/aml-report` | ✅ | Instr. 8/2024 |
| ADM-05 | Export RPB XML BdP | `?format=bdp` | API Admin | 🟡 | 🌐 template oficial X1 |
| ADM-06 | Submeter RPB BdP | Referência + audit | UI Admin | ✅ | |
| ADM-07 | Audit log global | Pesquisa trail | `/admin/audit` | ✅ | |
| ADM-08 | Seed compliance | PAC/DPIA default | Hosted seed | ✅ | |

---

## Módulo 8 — Dashboard e notificações

| ID | Funcionalidade | Descrição | UI / API | Estado |
|----|----------------|-----------|----------|--------|
| DSH-01 | KPIs casos | Aprovados hoje, pendentes | `/` | ✅ |
| DSH-02 | SignalR hub | Progresso e alertas | KycHub | ✅ |
| DSH-03 | Alertas supervisores | SAR, compliance | Grupo supervisors | ✅ |

---

## Módulo 9 — Infraestrutura e operações

| ID | Funcionalidade | Descrição | Estado |
|----|----------------|-----------|--------|
| OPS-01 | Health check | `/health` | ✅ |
| OPS-02 | Docker on-prem | `docker-compose.prod.yml` | ✅ |
| OPS-03 | CI GitHub Actions | Build + migrate + test | ✅ |
| OPS-04 | Key Vault secrets | Opcional | ✅ |
| OPS-05 | Messaging abstracção | SB / Rabbit / memory | ✅ |
| OPS-06 | Data retention job | Opt-in hosted | 🟡 |
| OPS-07 | Pen test checklist | Documentado | 🔴 execução |

---

## Módulo 10 — Autenticação

| ID | Funcionalidade | Descrição | Estado |
|----|----------------|-----------|--------|
| AUTH-01 | Entra ID OIDC | Produção | ✅ |
| AUTH-02 | Identity local | Dev | ✅ |
| AUTH-03 | Roles Analyst/Supervisor/Admin | Políticas | ✅ |
| AUTH-04 | Analyst accessor HTTP | Audit actor | ✅ |

---

## Resumo por estado (Maio 2026)

| Estado | Contagem aprox. | % |
|--------|-----------------|---|
| ✅ | ~75 features | ~90% |
| 🟡 | ~8 | ~10% |
| 🔴 | ~4 | ~5% |

**Gaps prioritários para go-live:** template RPB oficial (X1), credenciais UIF/BdP/identidade (X2–X4), execução E2E + pen test + dossier.

---

## Matriz UI → funcionalidade compliance

| Ecrã | Funcionalidades |
|------|-----------------|
| CaseList | KYC-03, badge SAR |
| CaseDetail | KYC-04/05, CMP-02/03, ENT-04 embed, SCR-02, DOC-01 |
| ComplianceCaseSection | CMP-01–20 |
| UboGraph | ENT-04/05 |
| CasePartyDetail | ENT-07, CMP-11/12, SCR-04 |
| Admin | ADM-01–07 |

---

## Referências

- Documentação técnica: [DOCUMENTACAO_APLICACAO.md](DOCUMENTACAO_APLICACAO.md)
- Operações: [OPERACOES_E_HOMOLOGACAO.md](OPERACOES_E_HOMOLOGACAO.md)
- Estado blueprint: [BLUEPRINT_COMPLETION_STATUS.md](BLUEPRINT_COMPLETION_STATUS.md)
