# Política de Criptografia — KYC AI Platform

> **Versão:** 1.0 · Alinhada com controlos técnicos implementados

## 1. Criptografia em trânsito

| Canal | Algoritmo / protocolo | Configuração |
|-------|----------------------|--------------|
| Browser ↔ aplicação | TLS 1.2+ | HTTPS obrigatório homologação/prod; HSTS |
| API externas (UIF, BdP, identidade) | TLS 1.2+ | HttpClient .NET com validação certificado |
| PostgreSQL | TLS opcional | `KYC_DB_CONNECTION` com `SSL Mode` conforme infra |
| Ollama | TLS recomendado | Rede interna ou TLS no reverse proxy |

## 2. Criptografia em repouso

| Dado | Método | Responsável |
|------|--------|-------------|
| PostgreSQL | TDE / volume encrypt (infra) | Equipa infra / cloud provider |
| Ficheiros documentos `Data/cases/` | Encriptação disco servidor | SO / volume LUKS ou storage encrypt |
| Secrets | Azure Key Vault ou variáveis ambiente | DevOps |
| Backups BD | Encriptados (AES-256) | Procedimento backup PRD |

## 3. Gestão de chaves

- Rotação API keys UIF/identidade: anual ou após incidente
- `IdentityVerification:WebhookSecret`: rotação com janela dual nos prestadores
- Certificados TLS: renovação automática (Let's Encrypt / cert manager)

## 4. Algoritmos aprovados

- Simétrico: AES-256-GCM
- Hash: SHA-256 (HMAC webhook, integridade)
- Assimétrico: RSA-2048+ ou ECDSA P-256+ (TLS)

## 5. Proibições

- Armazenar passwords em texto claro (excepto seed dev documentado)
- Algoritmos obsoletos (MD5, SHA-1 para segurança, SSLv3)

## 6. Evidência

- Configuração: `Program.cs` (cookies Secure), `_Host`, nginx TLS
- Pen test: validar TLS e headers — `docs/SECURITY_PEN_TEST_CHECKLIST.md`
