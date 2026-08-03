# API — KYC AI Platform

## OpenAPI / Swagger

| Recurso | URL (homologação) |
|---------|-------------------|
| Swagger UI | `/swagger` |
| Especificação OpenAPI 3 | `/swagger/v1/swagger.json` |
| Metadados | `GET /api/openapi/info` (anónimo) |

Activar em produção: `OpenApi__Enable=true` no `.env` (recomendado restringir por rede/firewall).

## Endpoints principais

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| GET | `/health` | Anónimo | Health check |
| POST | `/api/identity/webhook` | HMAC | Callback verificação identidade |
| POST | `/api/cases/{id}/documents` | Analyst+ | Upload multipart |
| GET | `/api/cases/{id}/report.pdf` | Analyst+ | PDF relatório KYC |
| GET | `/api/admin/aml-reports/{id}/export` | Admin | RPB JSON ou `?format=bdp` XML |
| GET | `/api/admin/compliance/metrics` | Admin/Auditor | Métricas FP/FN e biometria |

## Autenticação

- **Produção:** Microsoft Entra ID (JWT Bearer nas chamadas API machine-to-machine).
- **Desenvolvimento:** cookie ASP.NET Identity após login `/Identity/Account/Login`.

Documentação completa: [../DOCUMENTACAO_APLICACAO.md](../DOCUMENTACAO_APLICACAO.md) §8.
