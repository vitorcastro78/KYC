# API — Plataforma de IA KYC

## OpenAPI / Swagger

| Recurso | URL (homologación) |
|---------|---------------------|
| Swagger UI | `/swagger` |
| Especificación OpenAPI 3 | `/swagger/v1/swagger.json` |
| Metadatos | `GET /api/openapi/info` (anónimo) |

Activar en producción: `OpenApi__Enable=true` en `.env` (se recomienda restringir por red/firewall).

## Endpoints principales

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/health` | Anónimo | Health check |
| POST | `/api/identity/webhook` | HMAC | Callback de verificación de identidad |
| POST | `/api/cases/{id}/documents` | Analyst+ | Carga multipart |
| GET | `/api/cases/{id}/report.pdf` | Analyst+ | PDF del informe KYC |
| GET | `/api/admin/aml-reports/{id}/export` | Admin | JSON RPB o XML `?format=bdp` |
| GET | `/api/admin/compliance/metrics` | Admin/Auditor | Métricas FP/FN y biométricas |

## Autenticación

- **Producción:** Microsoft Entra ID (JWT Bearer para llamadas API máquina a máquina).
- **Desarrollo:** cookie de ASP.NET Identity tras el inicio de sesión en `/Identity/Account/Login`.

Documentación completa: [../DOCUMENTACAO_APLICACAO.md](../DOCUMENTACAO_APLICACAO.md) §8.
