# Política de Criptografía — KYC AI Platform

> **Versión:** 1.0 · Alineada con los controles técnicos implementados

## 1. Cifrado en tránsito

| Canal | Algoritmo / protocolo | Configuración |
|-------|------------------------|---------------|
| Navegador ↔ aplicación | TLS 1.2+ | HTTPS obligatorio en homologación/producción; HSTS |
| API externas (UIF, BdP, identidad) | TLS 1.2+ | .NET HttpClient con validación de certificado |
| PostgreSQL | TLS opcional | `KYC_DB_CONNECTION` con `SSL Mode` según la infraestructura |
| ContextMemory | TLS recomendado | Red interna o TLS en el proxy inverso |

## 2. Cifrado en reposo

| Dato | Método | Responsable |
|------|--------|-------------|
| PostgreSQL | TDE / cifrado de volumen (infraestructura) | Equipo de infraestructura / proveedor cloud |
| Archivos de documentos `Data/cases/` | Cifrado de disco del servidor | SO / volumen LUKS o cifrado de almacenamiento |
| Secretos | Azure Key Vault o variables de entorno | DevOps |
| Copias de seguridad BD | Cifradas (AES-256) | Procedimiento de backup PRD |

## 3. Gestión de claves

- Rotación de claves API de UIF/identidad: anual o tras un incidente
- `IdentityVerification:WebhookSecret`: rotación con ventana dual en los proveedores
- Certificados TLS: renovación automática (Let's Encrypt / cert manager)

## 4. Algoritmos aprobados

- Simétrico: AES-256-GCM
- Hash: SHA-256 (HMAC de webhook, integridad)
- Asimétrico: RSA-2048+ o ECDSA P-256+ (TLS)

## 5. Prohibiciones

- Almacenar contraseñas en texto plano (excepto seeds de desarrollo documentados)
- Algoritmos obsoletos (MD5, SHA-1 para seguridad, SSLv3)

## 6. Evidencia

- Configuración: `Program.cs` (cookies Secure), `_Host`, TLS de nginx
- Pen test: validar TLS y cabeceras — `docs/es/OPERACOES_E_HOMOLOGACAO.md` §6
