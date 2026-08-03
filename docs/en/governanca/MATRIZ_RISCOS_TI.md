# IT Risk Assessment Matrix — KYC AI Platform

> **Version:** 1.0 · **Review:** quarterly (next: _[date]_)

Scale: Likelihood (1–5) × Impact (1–5) = Exposure (1–25)

| ID | Risk | L | I | Exp. | Controls | Owner | Status |
|----|------|---|---|------|----------|-------|--------|
| R01 | Database failure without backup | 2 | 5 | 10 | DRP + daily backups | IT | 🟡 |
| R02 | Personal-data breach | 2 | 5 | 10 | TLS, RBAC, pen test, DPO | CISO | 🟡 |
| R03 | Sanctions false negative | 3 | 5 | 15 | OFAC/EU lists, analyst confirmation, metrics | Compliance | ✅ |
| R04 | Excessive false positive | 3 | 3 | 9 | Manual confirmation, FP metrics | Compliance | ✅ |
| R05 | FIU API unavailable | 3 | 4 | 12 | SAR queue + manual record | Compliance | ✅ |
| R06 | Identity provider down | 2 | 4 | 8 | In-person + polling | Ops | ✅ |
| R07 | LLM report hallucination | 2 | 4 | 8 | Human review, Art. 22 explainability | Compliance | ✅ |
| R08 | Insider abuse by administrator | 2 | 5 | 10 | Immutable audit, roles, MFA | CISO | ✅ |
| R09 | Ransomware | 2 | 5 | 10 | Offline backups, DRP | IT | 🟡 |
| R10 | RGPD retention non-compliance | 2 | 4 | 8 | DataRetention job 5–7y | DPO | 🟡 |

## BdP audit clause (4.3)

Template for subcontracting and SLA agreements:

> The institution and Banco de Portugal, under Law no. 83/2017 and any other applicable legislation, reserve the right to audit, directly or through appointed third parties, the information systems and data-processing records related to the KYC service, with reasonable prior notice of 30 business days, without prejudice to emergency audits in the event of a serious incident.

**Status:** ✅ Text available for inclusion in contract templates.
