# Matriz de evaluación de riesgos de TI — KYC AI Platform

> **Versión:** 1.0 · **Revisión:** trimestral (próxima: _[fecha]_)

Escala: Probabilidad (1–5) × Impacto (1–5) = Exposición (1–25)

| ID | Riesgo | P | I | Exp. | Controles | Propietario | Estado |
|----|--------|---|---|------|-----------|-------------|--------|
| R01 | Fallo de BD sin backup | 2 | 5 | 10 | PRD + backups diarios | TI | 🟡 |
| R02 | Brecha de datos personales | 2 | 5 | 10 | TLS, RBAC, pen test, DPO | CISO | 🟡 |
| R03 | Falso negativo en sanciones | 3 | 5 | 15 | Listas OFAC/UE, confirmación de analista, métricas | Cumplimiento | ✅ |
| R04 | Falso positivo excesivo | 3 | 3 | 9 | Confirmación manual, métricas FP | Cumplimiento | ✅ |
| R05 | API de UIF indisponible | 3 | 4 | 12 | Cola SAR + registro manual | Cumplimiento | ✅ |
| R06 | Proveedor de identidad caído | 2 | 4 | 8 | Presencial + polling | Ops | ✅ |
| R07 | Alucinación del LLM en informe | 2 | 4 | 8 | Revisión humana, explicabilidad Art. 22 | Cumplimiento | ✅ |
| R08 | Abuso interno de administrador | 2 | 5 | 10 | Auditoría inmutable, roles, MFA | CISO | ✅ |
| R09 | Ransomware | 2 | 5 | 10 | Backups offline, PRD | TI | 🟡 |
| R10 | Incumplimiento de retención RGPD | 2 | 4 | 8 | Job DataRetention 5–7a | DPO | 🟡 |

## Cláusula de auditoría BdP (4.3)

Modelo para contratos de subcontratación y SLA:

> La institución y el Banco de Portugal, conforme a la Ley n.º 83/2017 y demás normativa aplicable, se reservan el derecho de auditar, directamente o a través de terceros mandatados, los sistemas de información y los registros de tratamiento de datos relacionados con el servicio KYC, con un preaviso razonable de 30 días hábiles, sin perjuicio de auditorías de emergencia en caso de incidente grave.

**Estado:** ✅ Texto disponible para su inclusión en modelos contractuales.
