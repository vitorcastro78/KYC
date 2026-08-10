# Resolución de problemas

Guía para analistas y supervisores. Los problemas de infraestructura (servidor, base de datos) deben escalarse al equipo de TI con la sección «Para soporte técnico» del final.

## Cribado y progreso

| Qué ve | Causa probable | Qué hacer |
|--------|----------------|-----------|
| Barra de progreso detenida durante mucho tiempo | Motor de IA (ContextMemory) o cola de trabajo no disponible | Espere 2–3 min.; use **Rehacer cribado**; si persiste, contacte con TI |
| El progreso no se actualiza, pero el cribado continúa | Conexión WebSocket interrumpida | Actualice la página (F5); el porcentaje se sincroniza mediante la base de datos |
| «El cribado ha fallado» en pantalla | Error en el pipeline | Consulte con TI los logs de la aplicación; pruebe el recribado |
| Sin señales después del cribado | Entidad sin coincidencias o error parcial | Registre una señal manual; verifique las partes y el NIF |

## Aprobación y compliance

| Qué ve | Causa probable | Qué hacer |
|--------|----------------|-----------|
| **Aprobar** desactivado | Mensaje de bloqueo visible | Lea el mensaje: UBO pendiente, fondos EDD, etc. |
| «Aprobación bloqueada: UBO…» | Identidad no verificada | Sección amarilla → Verifique cada UBO/órgano |
| Falta el segundo aprobador | Caso EDD | Elija un supervisor en el modal de aprobación |
| Nombre «Entidad 123456789» | RCBE/GLEIF no han resuelto | Introduzca la denominación manual en el nuevo caso o corrija las partes |

## SAR y congelación

| Qué ve | Causa probable | Qué hacer |
|--------|----------------|-----------|
| Envío a la UIF fallido | Integración no configurada o no disponible | Registre la **referencia manual UIF** en la sección SAR |
| Estado SAR Pendiente | Comunicación manual registrada | Normal en contingencia — archive la referencia oficial |
| Alerta de congelación roja | Ha fallado la API BdP | Registre la **referencia manual BdP** después de confirmar la sanción |

## Informes y PDF

| Qué ve | Causa probable | Qué hacer |
|--------|----------------|-----------|
| «Informe no disponible» | Cribado no finalizado | Espere al 100 % o ejecute el recribado |
| El PDF no se abre / error | Servicio de conversión PDF en el servidor | Use el informe HTML; escale a TI |
| Texto extraño en el informe | Respuesta no válida del motor de IA | Rehaga el cribado; TI puede desactivar el enriquecimiento LLM |

## Casos y listas

| Qué ve | Causa probable | Qué hacer |
|--------|----------------|-----------|
| Lista de casos vacía | Sin datos o conexión a la BD | Confirme con TI que la base está disponible |
| Caso no creado tras el formulario | Rechazo de PAC | CAE/jurisdicción — Admin → Configuración → PAC |
| Acceso denegado después de iniciar sesión | Falta el role | Solicite `KYC.Analyst` al administrador |

## Documentos

| Qué ve | Causa probable | Qué hacer |
|--------|----------------|-----------|
| Documento en «Fallido» | Formato corrupto u OCR no disponible | Reenvíe el archivo; prefiera PDF con búsqueda |
| Mucho tiempo en «Procesando» | Cola de ingestión llena | Espere; rehaga el cribado tras «Completado» |

## Identidad y webhook

| Qué ve | Causa probable | Qué hacer |
|--------|----------------|-----------|
| Enlace de verificación vacío | Proveedor no configurado | Use **Verificado manualmente** |
| El estado no se actualiza después del portal | Webhook no recibido | TI: verifique el secret y la URL; use verificación manual |

---

## Para soporte técnico (TI)

| Síntoma | Verificación |
|---------|--------------|
| ContextMemory / scoring | Variable `CONTEXT_MEMORY_BASE_URL`, gateway ContextMemory accesible |
| Base de datos | PostgreSQL, migrations aplicadas |
| SignalR detrás de proxy | Cabeceras `Upgrade` y `Connection` en `/hubs/` |
| PDF | Chromium/Puppeteer en el contenedor `kyc-web` |
| Health | `GET /health` en la instancia |
| Audit | Tabla `audit_entries`, pantalla Admin → Audit log |

Logs de la aplicación: stdout del contenedor o servicio **kyc-web**.
