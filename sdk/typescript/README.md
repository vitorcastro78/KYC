# KYC AI Platform — TypeScript helpers

Thin utilities for calling a self-hosted [KYC](https://github.com/vitorcastro78/KYC) instance.

```bash
npm install kyc-ai-platform
```

```ts
import { clientOptions, healthUrl } from "kyc-ai-platform";

const { baseUrl, headers } = clientOptions({ baseUrl: "http://localhost:8080" });
const res = await fetch(healthUrl(baseUrl), { headers });
```

See the repo README for Docker / GHCR quick start.
