"""Thin KYC AI Platform client helpers."""

from __future__ import annotations

from typing import Any, Mapping


def headers(
    *,
    bearer_token: str | None = None,
    extra: Mapping[str, str] | None = None,
) -> dict[str, str]:
    """Build HTTP headers for KYC Web API calls (cookie auth is browser-only)."""
    h: dict[str, str] = {"Content-Type": "application/json", "Accept": "application/json"}
    if bearer_token:
        h["Authorization"] = f"Bearer {bearer_token}"
    if extra:
        h.update(extra)
    return h


def client_kwargs(
    *,
    base_url: str,
    bearer_token: str | None = None,
    timeout: float = 30.0,
) -> dict[str, Any]:
    """Keyword args useful with httpx.Client / httpx.AsyncClient."""
    return {
        "base_url": base_url.rstrip("/"),
        "headers": headers(bearer_token=bearer_token),
        "timeout": timeout,
    }


def health_url(base_url: str) -> str:
    """Absolute health endpoint used by Compose / probes."""
    return base_url.rstrip("/") + "/health"


__all__ = ["headers", "client_kwargs", "health_url"]
