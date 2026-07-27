#!/usr/bin/env bash
# Run KYC stack from GHCR (or build locally with --build).
# Usage:
#   ./scripts/docker-run.sh
#   ./scripts/docker-run.sh --build
#   ./scripts/docker-run.sh --db-only
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

BUILD=0
DB_ONLY=0
TAG="${KYC_TAG:-latest}"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --build) BUILD=1; shift ;;
    --db-only) DB_ONLY=1; shift ;;
    --tag) TAG="$2"; shift 2 ;;
    *) echo "Unknown arg: $1"; exit 1 ;;
  esac
done

if [[ ! -f .env && -f .env.example ]]; then
  echo "==> Creating .env from .env.example (edit passwords before production use)"
  cp .env.example .env
fi

if [[ "$DB_ONLY" -eq 1 ]]; then
  echo "==> Starting Postgres only (docker-compose.db.yml)"
  docker compose -f docker-compose.db.yml up -d
  echo "Stop: docker compose -f docker-compose.db.yml down"
  exit 0
fi

if [[ "$BUILD" -eq 1 ]]; then
  echo "==> Building and starting stack (docker compose up --build)"
  docker compose up --build -d
else
  echo "==> Pulling GHCR images and starting (docker-compose.ghcr.yml)"
  export KYC_TAG="$TAG"
  docker compose -f docker-compose.ghcr.yml pull
  docker compose -f docker-compose.ghcr.yml up -d
fi

PORT="${KYC_WEB_PORT:-8080}"
echo ""
echo "Web:     http://localhost:${PORT}"
echo "Health:  http://localhost:${PORT}/health"
echo "RabbitMQ management: http://localhost:15672"
echo ""
echo "Stop:    docker compose down"
echo "         docker compose -f docker-compose.ghcr.yml down"
