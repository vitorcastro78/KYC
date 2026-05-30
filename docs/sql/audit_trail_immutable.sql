-- Trigger PostgreSQL: impedir UPDATE/DELETE em audit_entries (homologação BdP)
-- Executar manualmente na BD de produção/homologação após revisão DBA.

CREATE OR REPLACE FUNCTION prevent_audit_mutation()
RETURNS TRIGGER AS $$
BEGIN
    RAISE EXCEPTION 'audit_entries é imutável';
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS audit_entries_immutable ON audit_entries;
CREATE TRIGGER audit_entries_immutable
    BEFORE UPDATE OR DELETE ON audit_entries
    FOR EACH ROW EXECUTE FUNCTION prevent_audit_mutation();
