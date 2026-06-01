# Novo caso e triagem automática

## Abrir um novo caso

**Menu:** Casos → **Novo caso** (`/cases/new`)

| Campo | O que preencher |
|-------|-----------------|
| NIF | Identificador fiscal do tomador (9 dígitos, PT) |
| Montante | Valor de crédito ou exposição pretendida |
| Relação | Ocasional ou continuada |
| CAE | Código de atividade económica (quando aplicável) |
| Denominação social (manual) | **Obrigatório** se o sistema não resolver o nome em RCBE/GLEIF |

### O que acontece a seguir

1. A **Política de Aceitação de Clientes (PAC)** valida CAE e jurisdição **antes** de gravar o caso.
2. Se a PAC rejeitar, o caso **não é criado** — corrija os dados ou contacte o administrador (Configurações → PAC).
3. Caso criado: estado **Em progresso** e arranque da **triagem automática**.

> **Dica:** Nomes genéricos do tipo «Entidade {NIF}» indicam falha de resolução. Volte ao formulário e preencha a denominação manual correcta.

## Acompanhar a triagem

No **detalhe do caso** (`/cases/{id}`):

- A **barra de progresso** mostra o módulo em execução (sanções, media, scoring, relatório, etc.).
- A percentagem actualiza em tempo real (SignalR) e por consulta à base de dados.
- Quando termina a 100 %, o relatório KYC fica disponível.

### Refazer triagem automática

Use **Refazer triagem automática** quando:

- Adicionou partes ou documentos depois da primeira triagem;
- Quer regenerar sinais e relatório com dados actualizados.

Confirme no diálogo — a operação pode demorar vários minutos.

## Sinais de risco

Após a triagem, reveja a lista de **sinais** no detalhe do caso:

| Acção | Quando usar |
|-------|-------------|
| **Confirmar** | A correspondência em listas/media é válida para este cliente |
| **Descartar** | Falso positivo — registe o motivo implícito na acção |
| **Registar sinal manual** | APIs falharam ou há risco não detectado automaticamente |

## Triagem de uma só parte

- No cartão da parte, use **Triagem desta parte**, ou
- Abra **Detalhe da parte** (`/cases/{id}/parties/{partyId}`) e execute a triagem individual.

## Grafo UBO

- No detalhe do caso, expanda **Grafo UBO** para ver a estrutura societária.
- Ecrã dedicado: botão para abrir o grafo completo (`/cases/{id}/ubo`).
