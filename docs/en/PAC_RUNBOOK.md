# Runbook — Customer Acceptance Policy (PAC)

> Law 83/2017, Art. 24 — minimum acceptance and rejection criteria.

## Active version

1. Admin → **Settings** — “Active PAC” card (version, thresholds, legal basis).
2. Database: `customer_acceptance_policies` table with `IsActive = true`.
3. Automatic seed: `ComplianceSeedHostedService` creates PAC `1.0.0` if it does not exist.

## New version (PAC v2+)

1. Admin → **Settings** → version field (e.g. `1.1.0`) → **Activate v2+**.
2. `CreateCustomerAcceptancePolicyCommand` deactivates the previous version and copies parameters.
3. New cases receive `LegalBasisRef` = `PAC/{version}/Lei83/2017-Art24`.

## Validation when starting a case

`StartKycCaseCommandHandler` runs `PolicyComplianceValidator` **before** saving the case:

| Rule | Effect |
|------|--------|
| CAE in `ProhibitedCaeActivitiesJson` | `PolicyViolationException` (auto-reject) |
| Prohibited / offshore jurisdiction | Auto-reject or violation |
| PEP in the structure | Auto-reject (PAC configuration) |

## Tests

- `StartKycCaseCommandHandlerTests` — CAE `92000` rejected
- `ComplianceHandlersIntegrationTests` — PAC at case start

## Homologation evidence

Settings screenshot + rejected-case log → `docs/dossier/01-pac/`.
