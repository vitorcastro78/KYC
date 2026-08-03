# Nuevo caso y cribado automático

## Abrir un nuevo caso

**Menú:** Casos → **Nuevo caso** (`/cases/new`)

| Campo | Qué completar |
|-------|---------------|
| NIF | Identificador fiscal del solicitante (9 dígitos, PT) |
| Importe | Valor de crédito o exposición solicitada |
| Relación | Ocasional o continuada |
| CAE | Código de actividad económica (cuando corresponda) |
| Denominación social (manual) | **Obligatorio** si el sistema no resuelve el nombre en RCBE/GLEIF |

### Qué sucede después

1. La **Política de Aceptación de Clientes (PAC)** valida el CAE y la jurisdicción **antes** de guardar el caso.
2. Si la PAC lo rechaza, el caso **no se crea** — corrija los datos o contacte con el administrador (Configuración → PAC).
3. Caso creado: estado **En curso** e inicio del **cribado automático**.

> **Consejo:** Los nombres genéricos del tipo «Entidad {NIF}» indican un error de resolución. Vuelva al formulario e introduzca la denominación manual correcta.

## Seguir el cribado

En el **detalle del caso** (`/cases/{id}`):

- La **barra de progreso** muestra el módulo en ejecución (sanciones, medios, scoring, informe, etc.).
- El porcentaje se actualiza en tiempo real (SignalR) y mediante consulta a la base de datos.
- Al llegar al 100 %, el informe KYC estará disponible.

### Rehacer el cribado automático

Use **Rehacer el cribado automático** cuando:

- Haya añadido partes o documentos después del primer cribado;
- Quiera regenerar señales e informe con datos actualizados.

Confirme en el diálogo — la operación puede tardar varios minutos.

## Señales de riesgo

Tras el cribado, revise la lista de **señales** en el detalle del caso:

| Acción | Cuándo usarla |
|--------|---------------|
| **Confirmar** | La coincidencia en listas/medios es válida para este cliente |
| **Descartar** | Falso positivo — el motivo queda registrado implícitamente en la acción |
| **Registrar señal manual** | Las API han fallado o existe un riesgo no detectado automáticamente |

## Cribado de una sola parte

- En la tarjeta de la parte, use **Cribado de esta parte**, o
- Abra **Detalle de la parte** (`/cases/{id}/parties/{partyId}`) y ejecute el cribado individual.

## Grafo UBO

- En el detalle del caso, expanda **Grafo UBO** para ver la estructura societaria.
- Pantalla específica: botón para abrir el grafo completo (`/cases/{id}/ubo`).
