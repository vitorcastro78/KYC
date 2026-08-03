# Manual do utilizador e troubleshooting — KYC AI Platform

> Analistas, supervisores e administradores.

## 1. Acesso

| Ambiente | URL | Autenticação |
|----------|-----|--------------|
| Homologação | _[URL institucional]_ | Entra ID (MFA) |
| Dev local | `http://localhost:8080` | `admin@kyc.local` (ver `.env`) |

Roles: `KYC.Analyst`, `KYC.Supervisor`, `KYC.Admin`, `KYC.Auditor`.

## 2. Fluxos principais

### Novo caso
1. **Casos → Novo** — NIF, montante, relação, CAE
2. Se RCBE/GLEIF não resolverem a entidade, preencher **denominação social (manual)** (obrigatório)
3. Aguardar barra de progresso (triagem automática)
4. Revisar sinais → confirmar ou descartar; usar **Registar sinal manual** se APIs de triagem falharem

### Conformidade (card amarelo)
- **Identidade** — Verificar UBO/órgão social; link portal se pendente; **Verificado manualmente (sem API)** se prestador indisponível
- **SAR** — Narrativa ≥200 caracteres ou «não aplicável» ≥50; se urgente falhar na UIF, estado fica **Pendente** com registo manual da referência
- **Congelamento BdP** — Após confirmar sanção; se API falhar, registo manual da ref. BdP no alerta vermelho
- **EDD** — Origem fundos + segundo aprovador na aprovação

### Aprovar
- Botão **Aprovar** só activo sem mensagem de bloqueio
- Supervisor: segundo aprovador obrigatório em EDD

## 3. Troubleshooting

| Sintoma | Causa provável | Acção |
|---------|----------------|-------|
| Caso não arranca (PAC) | CAE/jurisdição proibida | Ver Settings → PAC; corrigir dados |
| Aprovar desactivado | UBO não verificado / fundos EDD | Secção conformidade |
| Triagem parada em % | Ollama indisponível | Verificar `OLLAMA_ENDPOINT`; reiniciar Ollama |
| Webhook identidade 401 | HMAC incorrecto | Alinhar `IdentityVerification:WebhookSecret` |
| PDF relatório erro | Puppeteer/Chromium | Logs `kyc-web`; reinstalar deps Docker |
| SAR falha produção | UIF URL em falta | Configurar `Uif:BaseUrl` ou registo manual na secção SAR (estado Pendente) |
| Congelamento BdP falhou | `BdpAssetFreeze:BaseUrl` | Registar ref. manual após confirmar sanção |
| Nome «Entidade {NIF}» | Sem RCBE/GLEIF | Corrigir no arranque (denominação manual) ou partes manuais |
| Lista casos vazia | BD / migrations | `dotnet ef database update` |
| SignalR sem updates | Proxy WebSocket | nginx: `Upgrade` headers para `/hubs/` |

## 4. Logs e suporte

- Application logs: stdout Docker `kyc-web`
- Audit: Admin → Audit log ou query `audit_entries`
- Health: `GET /health`

## 5. APIs (equipa técnica)

Ver [api/README.md](api/README.md) e Swagger `/swagger`.

## 6. Documentação relacionada

- [ANALISTA_QUICK_START.md](ANALISTA_QUICK_START.md)
- [OPERACOES_E_HOMOLOGACAO.md](OPERACOES_E_HOMOLOGACAO.md)
- [MATRIZ_REQUISITOS_INSTITUCIONAIS.md](MATRIZ_REQUISITOS_INSTITUCIONAIS.md)
