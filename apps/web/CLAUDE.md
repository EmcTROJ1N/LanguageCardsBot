# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working on the web UI.

## What this is

Empty Vue 3 + Vite + TypeScript SPA. In production it is served behind the YARP `ApiGateway` (port 5050) through an `nginx:alpine` container. There is no authentication, no API integration, and no UI framework at this stage — only the infrastructure chain is proven end-to-end.

## Local development

```bash
cd apps/web
npm install
npm run dev
```

Vite serves on `http://localhost:5173` with HMR. The dev server proxies `/api/**` requests to `http://localhost:5050`, so calls to the gateway work the same as they will in production. Start the gateway separately when you need it:

```bash
# from repo root
docker compose up -d api-gateway
```

## Docker

Requires the shared external network:

```bash
docker network create language-cards-shared
```

Build and start:

```bash
# from repo root
docker compose up --build web-ui api-gateway
```

The `web-ui` container publishes no host ports — reach the SPA through the gateway at `http://localhost:5050/`.

## How routing works

- Browser → `http://localhost:5050/` → YARP catch-all route (`{**catch-all}`, `Order: 100`) → `web-ui:80` → nginx → `index.html` (SPA).
- Browser → `http://localhost:5050/api/cards/**` → YARP `cards-api` route (`Order: 1`) → Cards service.
- Browser → `http://localhost:5050/api/passport/**` → YARP `passport-api` route (`Order: 1`) → Passport service.

Nginx uses `try_files $uri $uri/ /index.html` — any unknown path serves the SPA and client-side routing is handled inside Vue Router. `index.html` is served with `Cache-Control: no-cache`; hashed JS/CSS assets in `/assets/` are cache-friendly by default.

## Stack

- Vue 3 (`<script setup>`), Vite, TypeScript.
- Vue Router registered with a single `/` route pointing to `src/views/Home.vue`.
- Pinia registered but empty (no stores yet).

No axios, no keycloak-js, no CSS framework. Those additions are planned as separate specs.

## Files

- `src/main.ts` — bootstraps the app, wires Router + Pinia.
- `src/App.vue` — top-level component, only contains `<router-view />`.
- `src/views/Home.vue` — the single route target ("It works" placeholder).
- `vite.config.ts` — Vite + Vue plugin + dev proxy for `/api`.
- `Dockerfile` — multi-stage: `node:22-alpine` builds, `nginx:alpine` serves.
- `nginx.conf` — SPA fallback + no-cache on `index.html`.
- `docker-compose.yml` — attaches to external `language-cards-shared` network.
