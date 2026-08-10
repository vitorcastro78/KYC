# Cryptography Policy — KYC AI Platform

> **Version:** 1.0 · Aligned with implemented technical controls

## 1. Encryption in transit

| Channel | Algorithm / protocol | Configuration |
|---------|----------------------|---------------|
| Browser ↔ application | TLS 1.2+ | Mandatory HTTPS in staging/production; HSTS |
| External APIs (FIU, BdP, identity) | TLS 1.2+ | .NET HttpClient with certificate validation |
| PostgreSQL | Optional TLS | `KYC_DB_CONNECTION` with `SSL Mode` according to infrastructure |
| ContextMemory | TLS recommended | Internal network or TLS at the reverse proxy |

## 2. Encryption at rest

| Data | Method | Owner |
|------|--------|-------|
| PostgreSQL | TDE / volume encryption (infrastructure) | Infrastructure team / cloud provider |
| Document files `Data/cases/` | Server disk encryption | OS / LUKS volume or storage encryption |
| Secrets | Azure Key Vault or environment variables | DevOps |
| Database backups | Encrypted (AES-256) | PRD backup procedure |

## 3. Key management

- Rotation of FIU/identity API keys: annually or after an incident
- `IdentityVerification:WebhookSecret`: rotation with a dual-key window at providers
- TLS certificates: automatic renewal (Let's Encrypt / cert manager)

## 4. Approved algorithms

- Symmetric: AES-256-GCM
- Hash: SHA-256 (webhook HMAC, integrity)
- Asymmetric: RSA-2048+ or ECDSA P-256+ (TLS)

## 5. Prohibitions

- Store passwords in plain text (except for documented development seeds)
- Obsolete algorithms (MD5, SHA-1 for security, SSLv3)

## 6. Evidence

- Configuration: `Program.cs` (Secure cookies), `_Host`, nginx TLS
- Pen test: validate TLS and headers — `docs/en/OPERACOES_E_HOMOLOGACAO.md` §6
