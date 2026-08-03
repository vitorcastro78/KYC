# Online manual (source)

Content curated for the help centre at `/help` in the KYC application.

- **Audience:** compliance analysts and supervisors.
- **Synchronization:** during the build, `.md` files are copied to `src/KYC.Web/wwwroot/help-online/`.
- **Technical documentation** (staging, architecture) remains in `docs/` and only appears in the Help menu for administrators.

To change the manual, edit the numbered `01-` to `08-` files and update `HelpDocManifest` in `KYC.Web/Services/Help/HelpDocEntry.cs` if you add new pages.
