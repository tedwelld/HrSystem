FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM nginx:1.27-alpine AS final
WORKDIR /usr/share/nginx/html

COPY docker/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/hr-system-web/browser/ /usr/share/nginx/html/
COPY docker/app-config.js /usr/share/nginx/html/app-config.js

EXPOSE 80
