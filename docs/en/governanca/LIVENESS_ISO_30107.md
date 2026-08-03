# Liveness — ISO/IEC 30107-3

## Regulatory requirement

BdP Notice 1/2022 — remote verification with presentation attack detection (PAD).

## Platform implementation

| Component | Description |
|-----------|-------------|
| Provider | DigitalSign / API configured in `IdentityVerification:BaseUrl` |
| Methods | Videoconference, CMD, in person, qualified signature |
| `LivenessScore` field | Persisted in `case_parties` after webhook/polling |
| Audit | `IdentityVerified` entry with `liveness:{score}` |

## ISO/IEC 30107-3 compliance

| Level | Responsible party | Evidence |
|-------|-------------------|----------|
| PAD algorithm certification | **Identity provider** | Accredited laboratory certificate or report |
| Technical integration | KYC Platform | Webhook + polling + score storage |
| Operation | Institution | Selection of a method appropriate to the risk (EDD → not simplified) |

**Status:** 🟡 Partial — technical integration ✅; provider certificate 🌐 pending in the dossier.

## Staging checklist

- [ ] Provider contract references ISO 30107-3 or equivalent
- [ ] Videoconference test with liveness score above institutional threshold
- [ ] Audit-trail printout with recorded liveness
- [ ] Certificate PDF attached in `docs/dossier/06-identidade/`
