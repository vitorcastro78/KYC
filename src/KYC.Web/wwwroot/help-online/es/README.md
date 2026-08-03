# Manual en línea (fuente)

Contenido seleccionado para el centro de ayuda en `/help` de la aplicación KYC.

- **Público:** analistas y supervisores de compliance.
- **Sincronización:** durante la compilación, los archivos `.md` se copian en `src/KYC.Web/wwwroot/help-online/`.
- **Documentación técnica** (homologación, arquitectura) permanece en `docs/` y solo aparece en el menú Ayuda para administradores.

Para modificar el manual, edite los archivos numerados del `01-` al `08-` y actualice `HelpDocManifest` en `KYC.Web/Services/Help/HelpDocEntry.cs` si añade nuevas páginas.
