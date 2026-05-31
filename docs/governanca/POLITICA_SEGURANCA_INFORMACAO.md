# Política de Segurança da Informação — KYC AI Platform

> **Versão:** 1.0 (rascunho para aprovação)  
> **Classificação:** Interno — Confidencial  
> **Aprovação:** _[CISO / COMEX — data e assinatura]_

## 1. Objectivo

Definir princípios e controlos de segurança para a plataforma KYC (dados pessoais, dados financeiros, comunicações UIF e audit trail regulatório).

## 2. Âmbito

- Aplicação KYC.Web, Workers, PostgreSQL, Ollama, integrações UIF/BdP/identidade
- Utilizadores: analistas, supervisores, administradores, auditores
- Ambientes: desenvolvimento, homologação, produção

## 3. Princípios

1. **Menor privilégio** — roles `KYC.Analyst`, `KYC.Supervisor`, `KYC.Admin`, `KYC.Auditor`
2. **Defesa em profundidade** — rede, TLS, auth, autorização, audit imutável
3. **Segregação de ambientes** — secrets distintos por ambiente; sem dados prod em dev
4. **Responsabilização** — `ICurrentAnalystAccessor` em todas as acções de compliance

## 4. Controlos implementados na plataforma

| Controlo | Implementação |
|----------|---------------|
| Autenticação | Microsoft Entra OIDC (prod) ou Identity (dev) |
| MFA | Conditional Access Entra (obrigatório operadores prod) |
| Autorização | Políticas ASP.NET Core por role |
| Sessão | Cookies HttpOnly; expiração 14 dias (dev Identity) |
| Secrets | `.env` / Azure Key Vault — nunca no repositório |
| Webhook | HMAC SHA-256 `IdentityVerification:WebhookSecret` |
| Audit | Trigger PostgreSQL imutável |
| Integrações prod | `Compliance:RequireLiveIntegrations` |

## 5. Gestão de incidentes

1. Detecção via logs Application Insights / SIEM institucional
2. Classificação P1–P4; notificação DPO se violação dados pessoais (72h RGPD)
3. Registo em ticket + entrada audit se impacto casos KYC

## 6. Revisão

Revisão anual ou após incidente grave. Próxima revisão: _[data]_.

## 7. Aprovações

| Função | Nome | Data | Assinatura |
|--------|------|------|------------|
| CISO | | | |
| DPO | | | |
| Responsável Compliance | | | |
