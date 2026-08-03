# API — KYC AI Platform

## OpenAPI / Swagger

| Resource | URL (homologation) |
|----------|---------------------|
| Swagger UI | `/swagger` |
| OpenAPI 3 specification | `/swagger/v1/swagger.json` |
| Metadata | `GET /api/openapi/info` (anonymous) |

Enable in production: `OpenApi__Enable=true` in `.env` (network/firewall restriction recommended).

## Main endpoints

| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/health` | Anonymous | Health check |
| POST | `/api/identity/webhook` | HMAC | Identity-verification callback |
| POST | `/api/cases/{id}/documents` | Analyst+ | Multipart upload |
| GET | `/api/cases/{id}/report.pdf` | Analyst+ | KYC report PDF |
| GET | `/api/admin/aml-reports/{id}/export` | Admin | RPB JSON or `?format=bdp` XML |
| GET | `/api/admin/compliance/metrics` | Admin/Auditor | FP/FN and biometric metrics |

## Authentication

- **Production:** Microsoft Entra ID (JWT Bearer for machine-to-machine API calls).
- **Development:** ASP.NET Identity cookie after login at `/Identity/Account/Login`.

Complete documentation: [../DOCUMENTACAO_APLICACAO.md](../DOCUMENTACAO_APLICACAO.md) §8.
