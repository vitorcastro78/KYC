# EF Core migrations (KYC)

Single baseline: `InitialCreate` (squashed 2026-08-10).

Report knowledge lives in ContextMemory Global Wiki — there is no local `pgvector` / `report_embeddings` table.

**Existing databases** that still have the old migration history must either:

1. Recreate the database (dev/CI), or
2. Baseline manually: ensure schema matches the current model, then insert  
   `20260810020359_InitialCreate` into `__EFMigrationsHistory` and remove obsolete history rows.
