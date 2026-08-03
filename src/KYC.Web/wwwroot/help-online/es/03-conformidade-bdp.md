# Compliance BdP (sección amarilla)

En el **detalle del caso**, la tarjeta **Compliance BdP** concentra las obligaciones regulatorias antes de aprobar.

## Verificación de identidad (UBO y órganos sociales)

**Base:** Aviso BdP 1/2022 — identificar y verificar a los beneficiarios efectivos y administradores.

| Estado en pantalla | Significado | Qué hacer |
|--------------------|-------------|-----------|
| Pendiente | Aún no verificado | Abrir **Verificar identidad** en la parte |
| Verificado | Proceso terminado | Puede continuar con la aprobación (si el resto de requisitos está OK) |
| Verificado manualmente | Sin API del proveedor | Úselo cuando el portal externo no esté disponible — justificación ≥ 20 caracteres |

**Métodos disponibles en el modal:**

- Verificación presencial (referencia del documento)
- Sesión remota (enlace al portal, cuando esté configurado)
- **Verificado manualmente (sin API)** — contingencia documentada

> **Atención:** Mientras exista un UBO u órgano social **pendiente**, el botón **Aprobar** permanecerá bloqueado con un mensaje explicativo.

## Diligencia debida reforzada (EDD)

Cuando el nivel de DDC es **EDD**:

1. Complete **Origen de los fondos** en la sección de compliance.
2. En la aprobación, seleccione el **segundo aprobador** (supervisor) — obligatorio.

## Comunicación a la UIF (SAR)

| Situación | Acción |
|----------|--------|
| Operación sospechosa que comunicar | **Comunicar a la UIF** — narrativa de **mínimo 200 caracteres** |
| SAR no aplicable | **SAR no aplicable** — justificación de **mínimo 50 caracteres** |
| Urgente | Marque «Urgente» para el envío inmediato (cuando la integración esté activa) |
| API UIF no disponible | Registre la **referencia manual** en el campo de la sección SAR; el estado queda **Pendiente** |

Los supervisores reciben alertas SAR en tiempo real en el grupo de supervisión.

## Congelación de activos (BdP)

Después de **confirmar** una señal de sanciones:

- El sistema puede notificar automáticamente al BdP y colocar el caso **En revisión**.
- Si falla la API, aparece una alerta roja — registre la **referencia manual** de confirmación del BdP.

## Discrepancia RCBE

En la ficha de identidad de una parte, use **Informar discrepancia RCBE** cuando los datos del registro no coincidan con la documentación — quedará registrado en el audit trail.

## Política de aceptación (PAC)

La PAC se valida **al crear el caso**. Los CAE o jurisdicciones prohibidos impiden la apertura. Los administradores gestionan versiones en **Administración → Configuración**.
