# Acesso e perfis de utilizador

## Entrar na plataforma

1. Abra o endereço fornecido pela sua instituição (homologação ou produção).
2. Inicie sessão com a conta corporativa (**Microsoft Entra ID**, com MFA quando exigido).
3. Em ambiente de desenvolvimento local, pode usar a conta de teste configurada pelo administrador.

## Perfis (roles)

| Perfil | O que pode fazer na aplicação |
|--------|-------------------------------|
| **Analista** (`KYC.Analyst`) | Criar e tratar casos, triagem, conformidade, relatórios, aprovar casos de risco baixo quando permitido |
| **Supervisor** (`KYC.Supervisor`) | Tudo o que o analista faz, mais segundo aprovador em EDD e alertas SAR em tempo real |
| **Administrador** (`KYC.Admin`) | Configurações (PAC, scoring, DPIA), relatório RPB BdP, utilizadores |
| **Auditor** (`KYC.Auditor`) | Consulta do registo de auditoria (audit log) |

> **Nota:** Se conseguir entrar mas vir «Acesso negado» numa página, a sua conta não tem a role necessária. Peça ao administrador `KYC.Analyst` ou superior.

## Navegação principal

| Menu | Destino | Utilidade |
|------|---------|-----------|
| Dashboard | `/dashboard` | Visão geral de casos e alertas |
| Casos KYC | `/cases` | Lista de todos os casos |
| Novo caso | `/cases/new` | Abrir um novo processo KYC |
| Manual | `/help` | Este guia |

## Boas práticas de segurança

- Não partilhe a sessão com outro colega — cada acção fica no audit trail com o seu utilizador.
- Termine sessão ao sair do posto (`Sair` no canto superior direito).
- Em caso de dúvida sobre dados pessoais, consulte o DPO da instituição (RGPD).
