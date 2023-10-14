FROM node:18-alpine AS builder
WORKDIR /app

ADD . .
RUN npm install
RUN npm run build

FROM nginx AS bundler
WORKDIR /var/www/itext/web

COPY --from=builder /app/dist .
COPY nginx.conf /etc/nginx/conf.d/default.conf
