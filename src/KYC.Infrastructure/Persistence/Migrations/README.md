# EF Core migrations (KYC)

Baseline: `InitialCreate` (squashed 2026-08-10) + `AddAuditImmutabilityTrigger`.

Report knowledge lives in ContextMemory Global Wiki — there is no local `pgvector` / `report_embeddings` table.

**Existing databases** that still have the old migration history: on startup, `KycMigrationBaseline` detects an existing schema without `20260810020359_InitialCreate` in `__EFMigrationsHistory` and inserts the baseline row so Migrate does not recreate tables. The audit-immutability migration then applies normally.

Dev/CI can still recreate the database from scratch.
