# Runbook — Política de Aceitação de Clientes (PAC)

> Lei 83/2017, Art. 24.º — critérios mínimos de aceitação e recusa.

## Versão activa

1. Admin → **Settings** — cartão «PAC activa» (versão, limiares, base legal).
2. Base de dados: tabela `customer_acceptance_policies` com `IsActive = true`.
3. Seed automático: `ComplianceSeedHostedService` cria PAC `1.0.0` se não existir.

## Nova versão (PAC v2+)

1. Admin → **Settings** → campo versão (ex. `1.1.0`) → **Activar v2+**.
2. `CreateCustomerAcceptancePolicyCommand` desactiva a versão anterior e copia parâmetros.
3. Novos casos recebem `LegalBasisRef` = `PAC/{versão}/Lei83/2017-Art24`.

## Validação no arranque do caso

`StartKycCaseCommandHandler` executa `PolicyComplianceValidator` **antes** de gravar o caso:

| Regra | Efeito |
|-------|--------|
| CAE em `ProhibitedCaeActivitiesJson` | `PolicyViolationException` (auto-reject) |
| Jurisdição proibida / offshore | Auto-reject ou violação |
| PEP na estrutura | Auto-reject (config PAC) |

## Testes

- `StartKycCaseCommandHandlerTests` — CAE `92000` rejeitado
- `ComplianceHandlersIntegrationTests` — PAC no arranque

## Evidência homologação

Print de Settings + log de caso rejeitado → `docs/dossier/01-pac/`.
