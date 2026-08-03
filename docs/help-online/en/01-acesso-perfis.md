# Access and user roles

## Sign in to the platform

1. Open the address provided by your institution (staging or production).
2. Sign in using your corporate account (**Microsoft Entra ID**, with MFA when required).
3. In a local development environment, you can use the test account configured by the administrator.

## Roles

| Role | What you can do in the application |
|------|-------------------------------------|
| **Analyst** (`KYC.Analyst`) | Create and handle cases, screening, compliance, reports, approve low-risk cases when allowed |
| **Supervisor** (`KYC.Supervisor`) | Everything an analyst can do, plus second approver for EDD and real-time SAR alerts |
| **Administrator** (`KYC.Admin`) | Settings (PAC, scoring, DPIA), BdP RPB report, users |
| **Auditor** (`KYC.Auditor`) | View the audit log |

> **Note:** If you can sign in but see “Access denied” on a page, your account does not have the required role. Ask the administrator for `KYC.Analyst` or higher.

## Main navigation

| Menu | Destination | Purpose |
|------|-------------|---------|
| Dashboard | `/dashboard` | Overview of cases and alerts |
| KYC cases | `/cases` | List of all cases |
| New case | `/cases/new` | Open a new KYC process |
| Manual | `/help` | This guide |

## Security best practices

- Do not share your session with another colleague — every action is recorded in the audit trail with your user account.
- Sign out when leaving your workstation (`Sign out` in the upper-right corner).
- If you have questions about personal data, consult your institution’s DPO (GDPR).
