# Conformidade BdP (secção amarela)

No **detalhe do caso**, o cartão **Conformidade BdP** concentra as obrigações regulatórias antes de aprovar.

## Verificação de identidade (UBO e órgãos sociais)

**Base:** Aviso BdP 1/2022 — identificar e verificar beneficiários efectivos e administradores.

| Estado no ecrã | Significado | O que fazer |
|----------------|-------------|-------------|
| Pendente | Ainda não verificado | Abrir **Verificar identidade** na parte |
| Verificado | Processo concluído | Pode prosseguir para aprovação (se restantes requisitos OK) |
| Verificado manualmente | Sem API do prestador | Use quando o portal externo está indisponível — justificação ≥ 20 caracteres |

**Métodos disponíveis no modal:**

- Verificação presencial (referência do documento)
- Sessão remota (link para portal, quando configurado)
- **Verificado manualmente (sem API)** — contingência documentada

> **Atenção:** Enquanto existir UBO ou órgão social **pendente**, o botão **Aprovar** permanece bloqueado com mensagem explicativa.

## Due diligence reforçada (EDD)

Quando o nível de DDC é **EDD**:

1. Preencha **Origem dos fundos** na secção de conformidade.
2. Na aprovação, seleccione o **segundo aprovador** (supervisor) — obrigatório.

## Comunicação à UIF (SAR)

| Situação | Acção |
|----------|--------|
| Operação suspeita a comunicar | **Comunicar à UIF** — narrativa **mínimo 200 caracteres** |
| SAR não aplicável | **SAR não aplicável** — justificação **mínimo 50 caracteres** |
| Urgente | Marque «Urgente» para submissão imediata (quando integração activa) |
| API UIF indisponível | Registe a **referência manual** no campo da secção SAR; estado fica **Pendente** |

Supervisores recebem alertas SAR em tempo real no grupo de supervisão.

## Congelamento de activos (BdP)

Após **confirmar** um sinal de sanções:

- O sistema pode notificar automaticamente o BdP e colocar o caso em **Em revisão**.
- Se a API falhar, aparece alerta vermelho — registe a **referência manual** de confirmação BdP.

## Discrepância RCBE

Na ficha de identidade de uma parte, use **Reportar discrepância RCBE** quando os dados do registo não coincidem com a documentação — fica registado no audit trail.

## Política de aceitação (PAC)

A PAC é validada **na criação do caso**. CAE ou jurisdição proibidos impedem a abertura. Administradores gerem versões em **Administração → Configurações**.
