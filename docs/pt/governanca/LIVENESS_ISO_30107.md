# Prova de vida (Liveness) — ISO/IEC 30107-3

## Requisito regulatório

Aviso BdP 1/2022 — verificação remota com detecção de ataque de apresentação (PAD).

## Implementação na plataforma

| Componente | Descrição |
|------------|-----------|
| Prestador | DigitalSign / API configurada em `IdentityVerification:BaseUrl` |
| Métodos | Videoconferência, CMD, presencial, assinatura qualificada |
| Campo `LivenessScore` | Persistido em `case_parties` após webhook/polling |
| Audit | Entrada `IdentityVerified` com `liveness:{score}` |

## Conformidade ISO/IEC 30107-3

| Nível | Responsável | Evidência |
|-------|-------------|-----------|
| Certificação do algoritmo PAD | **Prestador de identidade** | Certificado ou relatório de laboratório acreditado |
| Integração técnica | Plataforma KYC | Webhook + polling + armazenamento score |
| Operação | Instituição | Escolha método adequado ao risco (EDD → não simplificado) |

**Estado:** 🟡 Parcial — integração técnica ✅; certificado prestador 🌐 pendente no dossier.

## Checklist homologação

- [ ] Contrato prestador referencia ISO 30107-3 ou equivalente
- [ ] Teste videoconferência com liveness score > limiar institucional
- [ ] Print audit trail com liveness registado
- [ ] Anexo certificado PDF em `docs/dossier/06-identidade/`
