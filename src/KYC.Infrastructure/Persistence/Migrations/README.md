# EF Core migrations (KYC)

Baseline: `InitialCreate` (squashed 2026-08-10) + `AddAuditImmutabilityTrigger`.

Report knowledge lives in ContextMemory Global Wiki — there is no local `pgvector` / `report_embeddings` table.

**Apply migrations explicitly** (not on app startup):

```bash
dotnet ef database update --project src/KYC.Infrastructure --startup-project src/KYC.Web
# or
dotnet KYC.Web.dll --migrate-only
```

**Existing databases** after the squash: `--migrate-only` runs `KycMigrationBaseline` first (inserts `InitialCreate` into `__EFMigrationsHistory` when the schema already exists), then applies pending migrations such as the audit-immutability trigger.
