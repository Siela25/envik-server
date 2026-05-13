# envik-server

Backend API for the [envik](https://github.com/Siela25/envik) CLI tool — a `.env` file manager. Handles user authentication, organization and project management, and encrypted environment variable storage.

## Status

- [done] User registration and login with JWT authentication
- [done] Refresh token storage in Redis
- [done] PostgreSQL schema with full domain model (users, orgs, projects, environments)
- [done] Docker setup for local development
- [done] Kubernetes manifests for local cluster (minikube)
- [done] Separate migration job for K8s deployments
- [wip] Organization and project CRUD endpoints
- [wip] Environment variable storage with AES-256-GCM encryption
- [planned] OAuth (Google, GitHub)
- [planned] Request validation (FluentValidation)
- [planned] Error handling middleware
- [planned] Rate limiting

## Why

Side project to learn Kubernetes, Grafana, and Prometheus on a real workload. The envik CLI needs a backend to sync `.env` files across machines and teams.

## Tech stack

- **.NET 10 / ASP.NET Core** — Web API
- **PostgreSQL 16** — primary database, EF Core with code-first migrations
- **Redis 7** — refresh token storage (TTL-based, no persistence needed)
- **JWT (HS256)** — access tokens in Authorization header, 15 min expiry
- **StackExchange.Redis** — Redis client
- **FluentValidation** — request validation (in progress)
- **AutoMapper** — entity-to-DTO mapping (in progress)
- **Docker + Kubernetes** — containerized, runs on minikube locally

## Quick start

### Local development (Docker Compose)

Prerequisites: Docker Desktop, .NET 10 SDK

```bash
# Start PostgreSQL and Redis
docker-compose up -d

# Apply migrations
dotnet ef database update --project src/EnvikServer.Infrastructure --startup-project src/EnvikServer.API

# Run API (hot reload)
dotnet watch --project src/EnvikServer.API
```

API runs on `http://localhost:5089`. Swagger at `/swagger`.

### Kubernetes (minikube)

Prerequisites: Docker Desktop, minikube, kubectl

```bash
# Start cluster
minikube start --driver=docker

# Build and load images
docker build -t envik-server:latest .
docker build -t envik-migrator:latest -f migrator.Dockerfile .
minikube image load envik-server:latest
minikube image load envik-migrator:latest

# Create secrets file (gitignored, fill in your values)
# Linux/macOS
cp k8s/secrets.yaml.example k8s/secrets.yaml
# Windows
copy k8s\secrets.yaml.example k8s\secrets.yaml

# Apply manifests
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secrets.yaml
kubectl apply -f k8s/postgres/
kubectl apply -f k8s/redis/

# Run migrations
kubectl apply -f k8s/migrate-job.yaml
kubectl wait --for=condition=complete job/envik-migrate -n envik --timeout=60s

# Deploy API
kubectl apply -f k8s/api/

# Get API URL
minikube service envik-api-service -n envik --url
```

When redeploying after code changes:

```bash
kubectl scale deployment envik-api -n envik --replicas=0
minikube image rm envik-server:latest
docker build -t envik-server:latest .
minikube image load envik-server:latest
kubectl scale deployment envik-api -n envik --replicas=2
```

## Architecture

```
envik-server/
├── src/
│   ├── EnvikServer.Core/           # Domain entities, interfaces, DTOs
│   ├── EnvikServer.Application/    # Business logic (services)
│   ├── EnvikServer.Infrastructure/ # EF Core, repositories, Redis, encryption
│   └── EnvikServer.API/            # Controllers, middleware, Program.cs
└── k8s/
    ├── api/                        # API deployment and service
    ├── postgres/                   # StatefulSet, PVC, service
    ├── redis/                      # Deployment and service
    ├── configmap.yaml
    ├── secrets.yaml                # gitignored
    └── migrate-job.yaml            # Run before deploying API
```

## Key decisions

- **JWT in Authorization header** — straightforward for CLI usage; cookies would be more appropriate for a browser-facing app
- **Separate migration Job** — avoids race conditions when multiple API replicas start simultaneously; migrations run as a prerequisite step before deploying the API
- **Redis for refresh tokens** — native TTL support, no need for a cleanup job; tokens are ephemeral by design (pod restart invalidates sessions, acceptable tradeoff)
- **StatefulSet for PostgreSQL** — predictable pod names and stable storage binding vs Deployment; single replica, not intended for production HA
