# Resolução de problemas

Guia para analistas e supervisores. Problemas de infraestrutura (servidor, base de dados) devem ser escalados à equipa de TI com a secção «Para suporte técnico» no final.

## Triagem e progresso

| O que vê | Provável causa | O que fazer |
|----------|----------------|-------------|
| Barra de progresso parada durante muito tempo | Motor de IA (Ollama) ou fila de trabalho indisponível | Aguarde 2–3 min.; use **Refazer triagem**; se persistir, contacte TI |
| Progresso não actualiza mas triagem corre | Ligação WebSocket interrompida | Actualize a página (F5); a percentagem sincroniza pela base de dados |
| «Triagem falhou» no ecrã | Erro no pipeline | Consulte com TI os logs da aplicação; tente re-triagem |
| Sem sinais após triagem | Entidade sem correspondências ou falha parcial | Registar sinal manual; verificar partes e NIF |

## Aprovação e conformidade

| O que vê | Provável causa | O que fazer |
|----------|----------------|-------------|
| **Aprovar** desactivado | Mensagem de bloqueio visível | Leia a mensagem: UBO pendente, fundos EDD, etc. |
| «Aprovação bloqueada: UBO…» | Identidade não verificada | Secção amarela → Verificar cada UBO/órgão |
| Segundo aprovador em falta | Caso EDD | Escolha supervisor no modal de aprovação |
| Nome «Entidade 123456789» | RCBE/GLEIF não resolveram | Denominação manual no novo caso ou corrigir partes |

## SAR e congelamento

| O que vê | Provável causa | O que fazer |
|----------|----------------|-------------|
| Submissão UIF falhou | Integração não configurada ou indisponível | Registe **referência manual UIF** na secção SAR |
| Estado SAR Pendente | Comunicação manual registada | Normal em contingência — arquive a referência oficial |
| Alerta congelamento vermelho | API BdP falhou | Registe **referência manual BdP** após confirmar sanção |

## Relatórios e PDF

| O que vê | Provável causa | O que fazer |
|----------|----------------|-------------|
| «Relatório indisponível» | Triagem não concluída | Aguarde 100 % ou execute re-triagem |
| PDF não abre / erro | Serviço de conversão PDF no servidor | Use relatório HTML; escale a TI |
| Texto estranho no relatório | Resposta inválida do motor de IA | Re-triagem; TI pode desactivar enriquecimento LLM |

## Casos e listas

| O que vê | Provável causa | O que fazer |
|----------|----------------|-------------|
| Lista de casos vazia | Sem dados ou ligação à BD | Confirme com TI que a base está disponível |
| Caso não criado após formulário | Rejeição PAC | CAE/jurisdição — Admin → Configurações → PAC |
| Acesso negado após login | Role em falta | Pedir `KYC.Analyst` ao administrador |

## Documentos

| O que vê | Provável causa | O que fazer |
|----------|----------------|-------------|
| Documento em «Falhou» | Formato corrompido ou OCR indisponível | Reenvie ficheiro; preferir PDF pesquisável |
| Muito tempo em «A processar» | Fila de ingestão cheia | Aguarde; re-triagem após «Concluído» |

## Identidade e webhook

| O que vê | Provável causa | O que fazer |
|----------|----------------|-------------|
| Link de verificação vazio | Prestador não configurado | Use **Verificado manualmente** |
| Estado não actualiza após portal | Webhook não recebido | TI: verificar secret e URL; use verificação manual |

---

## Para suporte técnico (TI)

| Sintoma | Verificação |
|---------|-------------|
| Ollama / scoring | Variável `OLLAMA_ENDPOINT`, serviço Ollama activo |
| Base de dados | PostgreSQL, migrations aplicadas |
| SignalR atrás de proxy | Headers `Upgrade` e `Connection` em `/hubs/` |
| PDF | Chromium/Puppeteer no contentor `kyc-web` |
| Health | `GET /health` na instância |
| Audit | Tabela `audit_entries`, ecrã Admin → Audit log |

Logs da aplicação: stdout do contentor ou serviço **kyc-web**.
