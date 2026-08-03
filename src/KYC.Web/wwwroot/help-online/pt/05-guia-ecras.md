# Guia por ecrã

Referência rápida: o que encontrar em cada área da aplicação.

## Dashboard (`/dashboard`)

- Total de casos, casos em curso, aprovados hoje, em revisão humana.
- Lista dos casos mais recentes.
- Atalho **Novo caso**.
- Actualizações em tempo real quando casos mudam de estado.

## Lista de casos (`/cases`)

| Coluna / elemento | Descrição |
|-------------------|-----------|
| Score | Pontuação de risco 0–100 |
| DDC | Simplificada / Standard / Enhanced |
| Badge SAR | Estado da comunicação à UIF |
| Estado | Pendente, Em progresso, Em revisão, Aprovado, Rejeitado |

Clique numa linha para abrir o detalhe.

## Detalhe do caso (`/cases/{id}`)

| Zona | Conteúdo |
|------|----------|
| Hero | Nome, NIF, score, nível de risco, estado, DDC |
| Barra de progresso | Triagem automática em curso |
| Acções | Aprovar, Rejeitar, Revisão, Refazer triagem, Relatório, PDF |
| Conformidade BdP | Cartão amarelo — identidade, SAR, congelamento, fundos |
| Partes | Cartões com triagem e identidade |
| Sinais | Lista com confirmar / descartar |
| Documentos | Upload e estado de ingestão |
| Timeline | Histórico de risco e auditoria resumida |
| Grafo UBO | Pré-visualização e link para ecrã completo |

## Novo caso (`/cases/new`)

Formulário de abertura + pré-visualização de resolução de entidade por NIF.

## Relatório (`/cases/{id}/report`)

Relatório HTML completo para leitura e impressão.

## Grafo UBO (`/cases/{id}/ubo`)

Visualização hierárquica, tabela e indicadores PEP/sanções por nó.

## Detalhe da parte (`/cases/{id}/parties/{partyId}`)

Foco numa parte: identidade, sinais filtrados, triagem individual.

## Administração (perfil Admin)

| Ecrã | Função |
|------|--------|
| Utilizadores | Gestão de contas (quando aplicável) |
| RPB BdP | Relatório de prevenção do branqueamento anual |
| Scoring | Versões do motor de scoring |
| DPIA | Registo de avaliação de impacto |
| Configurações | PAC activa, parâmetros globais |
| Audit log | Trilho de auditoria (também Auditor) |
