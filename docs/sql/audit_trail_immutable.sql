-- Audit imutável (homologação BdP)
-- Preferir: dotnet ef database update (migration 20260529205723_BdpComplianceAndGtm)
-- Este script é equivalente manual se a migration já foi aplicada sem o trigger.

CREATE OR REPLACE FUNCTION prevent_audit_entry_mutation() RETURNS trigger AS $$
BEGIN
    RAISE EXCEPTION 'audit_entries are immutable';
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS tr_audit_entries_immutable ON audit_entries;
CREATE TRIGGER tr_audit_entries_immutable
    BEFORE UPDATE OR DELETE ON audit_entries
    FOR EACH ROW EXECUTE FUNCTION prevent_audit_entry_mutation();
