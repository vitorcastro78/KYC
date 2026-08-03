# Decisões, relatório e documentos

## Estados do caso

| Estado | Significado típico |
|--------|-------------------|
| Pendente | Caso criado, triagem ainda não concluída |
| Em progresso | Triagem ou análise em curso |
| Em revisão | Requer intervenção humana (sanções, SAR, scoring elevado, etc.) |
| Aprovado | Decisão positiva registada |
| Rejeitado | Decisão negativa com motivo |

## Aprovar um caso

1. Confirme que **não há mensagem de bloqueio** junto ao botão Aprovar.
2. Bloqueios frequentes: UBO não verificado, origem dos fundos em falta (EDD), sinais críticos por confirmar.
3. Em **EDD**, escolha o segundo aprovador no modal.
4. Confirme a aprovação.

Casos de **risco baixo** (score ≤ 30, sem sinais graves) podem ser **auto-aprovados** pelo motor — verifique o estado após a triagem.

## Rejeitar ou pedir revisão

- **Rejeitar** — motivo obrigatório; use quando a relação comercial não deve prosseguir.
- **Pedir revisão manual** — encaminha para fila de revisão sem rejeitar definitivamente.

## Relatório KYC

| Acção | Onde |
|-------|------|
| Ver relatório HTML | **Ver relatório** no detalhe do caso (`/cases/{id}/report`) |
| Exportar PDF | Botão **PDF** no detalhe ou no relatório |

O relatório inclui sumário executivo, partes, sinais, scoring, recomendação e notas de transparência (RGPD Art. 22).

> **Dica:** Se o PDF falhar, tente primeiro abrir o relatório HTML. Persistindo o erro, veja [Resolução de problemas](07-resolucao-problemas.md).

## Upload de documentos

No detalhe do caso:

1. **Enviar documento** — PDF, DOCX ou imagem (até ~25 MB).
2. Associe opcionalmente a uma parte e ao tipo (identificação, demonstrações, UBO, etc.).
3. O processamento é **assíncrono** — estado: Pendente → A processar → Concluído / Falhou.
4. Após conclusão, pode disparar **re-triagem** para incorporar factos extraídos.

## Adicionar parte manualmente

**Adicionar parte** — accionista, UBO, órgão social, procurador, com opção de triagem imediata após gravação.
