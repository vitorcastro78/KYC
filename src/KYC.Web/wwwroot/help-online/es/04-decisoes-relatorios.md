# Decisiones, informe y documentos

## Estados del caso

| Estado | Significado habitual |
|--------|----------------------|
| Pendiente | Caso creado, cribado aún no finalizado |
| En curso | Cribado o análisis en curso |
| En revisión | Requiere intervención humana (sanciones, SAR, scoring elevado, etc.) |
| Aprobado | Decisión positiva registrada |
| Rechazado | Decisión negativa con motivo |

## Aprobar un caso

1. Confirme que **no hay ningún mensaje de bloqueo** junto al botón Aprobar.
2. Bloqueos frecuentes: UBO sin verificar, origen de los fondos pendiente (EDD), señales críticas por confirmar.
3. En **EDD**, elija el segundo aprobador en el modal.
4. Confirme la aprobación.

Los casos de **bajo riesgo** (score ≤ 30, sin señales graves) pueden ser **autoaprobados** por el motor — verifique el estado después del cribado.

## Rechazar o solicitar revisión

- **Rechazar** — motivo obligatorio; úselo cuando la relación comercial no deba continuar.
- **Solicitar revisión manual** — envía el caso a la cola de revisión sin rechazarlo definitivamente.

## Informe KYC

| Acción | Dónde |
|--------|-------|
| Ver informe HTML | **Ver informe** en el detalle del caso (`/cases/{id}/report`) |
| Exportar PDF | Botón **PDF** en el detalle o en el informe |

El informe incluye resumen ejecutivo, partes, señales, scoring, recomendación y notas de transparencia (RGPD Art. 22).

> **Consejo:** Si falla el PDF, intente primero abrir el informe HTML. Si el error persiste, consulte [Resolución de problemas](07-resolucao-problemas.md).

## Carga de documentos

En el detalle del caso:

1. **Enviar documento** — PDF, DOCX o imagen (hasta ~25 MB).
2. Asócielo opcionalmente a una parte y al tipo (identificación, estados financieros, UBO, etc.).
3. El procesamiento es **asíncrono** — estado: Pendiente → Procesando → Completado / Fallido.
4. Tras completarse, puede activar el **recribado** para incorporar los hechos extraídos.

## Añadir una parte manualmente

**Añadir parte** — accionista, UBO, órgano social, apoderado, con opción de cribado inmediato después de guardar.
