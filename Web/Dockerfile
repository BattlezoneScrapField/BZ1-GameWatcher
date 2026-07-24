# Stage 1: Build the Angular app
# node:21 reached end of life; 22 is the current LTS line.
FROM node:22-alpine AS builder

WORKDIR /usr/local/app

# Copy the manifests first so `npm ci` is only re-run when dependencies actually change.
COPY package.json package-lock.json ./
RUN npm ci

COPY . .

# `ng build` already defaults to the production configuration; the old `--prod` was consumed by
# npm rather than passed to ng, so it did nothing.
RUN npm run build

# Stage 2: Serve the built app with NGINX
FROM nginx:1.27-alpine

RUN rm -rf /usr/share/nginx/html/*

COPY --from=builder /usr/local/app/dist/bz1-game-watcher/browser/ /usr/share/nginx/html

EXPOSE 80
