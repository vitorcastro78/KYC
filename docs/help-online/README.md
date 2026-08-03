# Manual online (fonte)

Conteúdo curado para o centro de ajuda em `/help` na aplicação KYC.

- **Público:** analistas e supervisores de compliance.
- **Idiomas:** português nesta pasta e em [`pt/`](pt/); inglês em [`en/`](en/); espanhol em [`es/`](es/).
- **Sincronização:** no build, os ficheiros `.md` (incl. `pt/`, `en/` e `es/`) são copiados para `src/KYC.Web/wwwroot/help-online/`.
- **Documentação técnica** (homologação, arquitectura) permanece em `docs/` (e traduções em `docs/pt/`, `docs/en/`, `docs/es/`) e só aparece no menu Ajuda para administradores.

Para alterar o manual, edite os ficheiros numerados `01-` a `08-` (e as pastas `pt/` / `en/` / `es/`) e actualize `HelpDocManifest` em `KYC.Web/Services/Help/HelpDocEntry.cs` se adicionar novas páginas.

