# Dossier de homologação BdP — KYC Platform

Pasta para evidências do go-live regulatório (Instrução BdP 8/2024, Lei 83/2017, Aviso 1/2022).

## Estrutura sugerida

```
docs/dossier/
  README.md                 (este ficheiro)
  01-pac/                   PAC activa (export JSON ou print Admin → Settings)
  02-dpia/                  DPIA activa + caminho do documento
  03-scoring/               Versão scoring + prompt hash
  04-rpb/                   RPB anual: XML BdP + JSON interno + ref. submissão
  05-sar-uif/               Prints SAR submetido + ref. UIF (staging/prod)
  06-identidade/            Webhook HMAC + verificação party (Aviso 1/2022)
  07-congelamento/          Notificação BdP + confirmação
  08-audit/                 Extract audit trail (caso teste E2E)
  09-e2e/                   Checklist `docs/E2E_HOMOLOGACAO.md` assinado
  10-seguranca/             `docs/SECURITY_PEN_TEST_CHECKLIST.md` preenchido
```

## Como gerar evidências

1. Executar `docs/E2E_HOMOLOGACAO.md` (15 passos) em ambiente de homologação.
2. Admin → **Settings**: captura PAC, scoring e DPIA activos.
3. Admin → **RPB**: gerar ano corrente, exportar XML/JSON, submeter (role `KYC.Admin`).
4. Caso teste com sanção confirmada: print congelamento + audit `AssetFreezeNotificationSent`.
5. Anexar versões de ficheiros (data no nome: `RPB-2025-20260530.xml`).

## Responsáveis

| Área | Owner |
|------|--------|
| Compliance / PAC | Equipa compliance |
| RPB submissão | Admin KYC (`KYC.Admin`) |
| Segurança | Infra + pen test checklist |
| E2E | Analista AML + QA |
