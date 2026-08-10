# KYC AI Platform — Python helpers

Thin utilities for calling a self-hosted [KYC](https://github.com/vitorcastro78/KYC) instance.

```bash
pip install kyc-ai-platform
```

```python
from kyc_ai_platform import client_kwargs, health_url
import httpx

base = "http://localhost:8080"
with httpx.Client(**client_kwargs(base_url=base)) as client:
    r = client.get("/health")
    r.raise_for_status()
```

See the repo README for Docker / GHCR quick start.
