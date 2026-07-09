# Etapa 1: instalar dependencias y compilar el bundle de producción
FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .

# VITE_API_URL se "hornea" dentro del bundle en build time (Vite no lee
# variables de entorno en runtime) — por eso es un build ARG, no un ENV del
# contenedor final. docker-compose.yml lo pasa como build.args.
ARG VITE_API_URL=http://localhost:5080
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

# Etapa 2: servir los archivos estáticos ya compilados con nginx
FROM nginx:alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
