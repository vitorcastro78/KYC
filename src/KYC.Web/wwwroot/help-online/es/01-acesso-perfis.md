# Acceso y perfiles de usuario

## Acceder a la plataforma

1. Abra la dirección facilitada por su institución (homologación o producción).
2. Inicie sesión con su cuenta corporativa (**Microsoft Entra ID**, con MFA cuando sea necesario).
3. En un entorno de desarrollo local, puede usar la cuenta de prueba configurada por el administrador.

## Perfiles (roles)

| Perfil | Qué puede hacer en la aplicación |
|--------|-----------------------------------|
| **Analista** (`KYC.Analyst`) | Crear y gestionar casos, cribado, compliance, informes y aprobar casos de bajo riesgo cuando esté permitido |
| **Supervisor** (`KYC.Supervisor`) | Todo lo que hace el analista, además de segundo aprobador en EDD y alertas SAR en tiempo real |
| **Administrador** (`KYC.Admin`) | Configuración (PAC, scoring, DPIA), informe RPB BdP y usuarios |
| **Auditor** (`KYC.Auditor`) | Consulta del registro de auditoría (audit log) |

> **Nota:** Si puede iniciar sesión pero ve «Acceso denegado» en una página, su cuenta no tiene el role necesario. Solicite al administrador `KYC.Analyst` o superior.

## Navegación principal

| Menú | Destino | Utilidad |
|------|---------|-----------|
| Dashboard | `/dashboard` | Vista general de casos y alertas |
| Casos KYC | `/cases` | Lista de todos los casos |
| Nuevo caso | `/cases/new` | Abrir un nuevo proceso KYC |
| Manual | `/help` | Esta guía |

## Buenas prácticas de seguridad

- No comparta la sesión con otro compañero — cada acción queda registrada en el audit trail con su usuario.
- Cierre sesión al dejar el puesto (`Salir` en la esquina superior derecha).
- Si tiene dudas sobre los datos personales, consulte al DPO de su institución (RGPD).
