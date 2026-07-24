# Stage 1: Build Angular 18 App
FROM node:21-alpine AS builder

# Set our working directory
WORKDIR /usr/local/app

# Add the source code to app
COPY ./ /usr/local/app/

# Install dependencies
RUN npm ci

# Generate the build of the application
RUN npm run build --prod

# Stage 2: Combine Angular and Node.js with NGINX
FROM nginx:latest

# Remove the default Nginx files.
RUN rm -rf /usr/share/nginx/html/*

# Copy the Angular build from the builder stage to NGINX
COPY --from=builder /usr/local/app/dist/bz1-game-watcher/browser/ /usr/share/nginx/html

# Expose port 80
EXPOSE 80