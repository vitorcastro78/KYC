# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.0.1-beta] - Unreleased

### Added

- ContextMemory as the sole LLM gateway (scoring, OCR vision, report wiki).
- Release pipeline: release-please, GHCR Docker images, Windows appliance, PyPI/npm SDKs, announce workflows.
- Canonical docs packs under `docs/pt|en|es/` with consolidated operations guide.

### Removed

- Local dual-LLM / Ollama / direct cloud LLM clients and report `pgvector` embeddings.
- Legacy docs root duplicates and operational satellite markdown files.
