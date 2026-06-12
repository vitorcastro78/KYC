# Manual online (fonte)

Conteúdo curado para o centro de ajuda em `/help` na aplicação KYC.

- **Público:** analistas e supervisores de compliance.
- **Sincronização:** no build, os ficheiros `.md` são copiados para `src/KYC.Web/wwwroot/help-online/`.
- **Documentação técnica** (homologação, arquitectura) permanece em `docs/` e só aparece no menu Ajuda para administradores.

Para alterar o manual, edite os ficheiros numerados `01-` a `08-` e actualize `HelpDocManifest` em `KYC.Web/Services/Help/HelpDocEntry.cs` se adicionar novas páginas.
