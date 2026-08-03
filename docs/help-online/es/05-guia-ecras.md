# Guía por pantalla

Referencia rápida: qué encontrar en cada área de la aplicación.

## Dashboard (`/dashboard`)

- Total de casos, casos en curso, aprobados hoy, en revisión humana.
- Lista de los casos más recientes.
- Acceso directo **Nuevo caso**.
- Actualizaciones en tiempo real cuando los casos cambian de estado.

## Lista de casos (`/cases`)

| Columna / elemento | Descripción |
|--------------------|-------------|
| Score | Puntuación de riesgo 0–100 |
| DDC | Simplificada / Estándar / Mejorada |
| Badge SAR | Estado de la comunicación a la UIF |
| Estado | Pendiente, En curso, En revisión, Aprobado, Rechazado |

Haga clic en una fila para abrir el detalle.

## Detalle del caso (`/cases/{id}`)

| Zona | Contenido |
|------|-----------|
| Hero | Nombre, NIF, score, nivel de riesgo, estado, DDC |
| Barra de progreso | Cribado automático en curso |
| Acciones | Aprobar, Rechazar, Revisión, Rehacer cribado, Informe, PDF |
| Compliance BdP | Tarjeta amarilla — identidad, SAR, congelación, fondos |
| Partes | Tarjetas con cribado e identidad |
| Señales | Lista para confirmar / descartar |
| Documentos | Carga y estado de ingestión |
| Timeline | Historial de riesgo y auditoría resumida |
| Grafo UBO | Vista previa y enlace a pantalla completa |

## Nuevo caso (`/cases/new`)

Formulario de apertura + vista previa de resolución de entidad por NIF.

## Informe (`/cases/{id}/report`)

Informe HTML completo para lectura e impresión.

## Grafo UBO (`/cases/{id}/ubo`)

Visualización jerárquica, tabla e indicadores PEP/sanciones por nodo.

## Detalle de la parte (`/cases/{id}/parties/{partyId}`)

Foco en una parte: identidad, señales filtradas, cribado individual.

## Administración (perfil Admin)

| Pantalla | Función |
|----------|---------|
| Usuarios | Gestión de cuentas (cuando corresponda) |
| RPB BdP | Informe anual de prevención del blanqueo de capitales |
| Scoring | Versiones del motor de scoring |
| DPIA | Registro de evaluación de impacto |
| Configuración | PAC activa, parámetros globales |
| Audit log | Rastro de auditoría (también Auditor) |
