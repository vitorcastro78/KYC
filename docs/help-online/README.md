# Manual online (fonte)

Conteúdo curado para o centro de ajuda em `/help` na aplicação KYC.

- **Público:** analistas e supervisores de compliance.
- **Idiomas canónicos:** [`pt/`](pt/), [`en/`](en/), [`es/`](es/) — sem cópias na raiz desta pasta.
- **Sincronização:** no build, os `.md` de `pt|en|es` são copiados para `src/KYC.Web/wwwroot/help-online/{lang}/`.
- **Documentação técnica** (homologação, arquitectura): `docs/{pt,en,es}/` — menu Ajuda só para administradores.

Para alterar o manual, edite os ficheiros numerados `01-` a `08-` em cada idioma e actualize `HelpDocManifest` se adicionar páginas.
