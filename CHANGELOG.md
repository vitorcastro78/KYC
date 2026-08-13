# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 1.0.0 (2026-08-13)


### Features

* ContextMemory-only LLM, docs packs, and release pipeline ([3fedead](https://github.com/vitorcastro78/KYC/commit/3fedead480654878a0dbc053e4fa186b5582c1d3))


### Bug Fixes

* baseline EF history after migration squash on existing DBs ([ec9bfc2](https://github.com/vitorcastro78/KYC/commit/ec9bfc2110d7f9506d137829a812166e3ea746b6))
* dump kyc-web container logs when deploy health fails ([cedc72e](https://github.com/vitorcastro78/KYC/commit/cedc72e3e694d1979f351e7e521ebd8d7f1bcb44))
* restore audit immutability trigger and boot Docker on deploy ([40a5469](https://github.com/vitorcastro78/KYC/commit/40a54698f1660b6651608549c624ef9b6aa84c86))
* search and start Docker more robustly on self-hosted deploy ([95b3b56](https://github.com/vitorcastro78/KYC/commit/95b3b564e6cf3f2be0c2a30306dd0614c857e58b))
* stop applying EF migrations on application startup ([2b3a1ab](https://github.com/vitorcastro78/KYC/commit/2b3a1ab7c0866c748350045315fc1ec424fac849))
* use ASCII-only strings in deploy-local PowerShell ([84e217c](https://github.com/vitorcastro78/KYC/commit/84e217cf6cc7a486145ee7223ab47aad43904336))

## [0.0.1-beta] - Unreleased

### Added

- ContextMemory as the sole LLM gateway (scoring, OCR vision, report wiki).
- Release pipeline: release-please, GHCR Docker images, Windows appliance, PyPI/npm SDKs, announce workflows.
- Canonical docs packs under `docs/pt|en|es/` with consolidated operations guide.

### Removed

- Local dual-LLM / Ollama / direct cloud LLM clients and report `pgvector` embeddings.
- Legacy docs root duplicates and operational satellite markdown files.
