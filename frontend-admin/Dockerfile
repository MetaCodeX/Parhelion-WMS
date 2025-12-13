# ===================================
# PARHELION ADMIN (Angular) - Dockerfile
# Multi-stage build para producción
# ===================================

# --- STAGE 1: Build ---
FROM node:20-alpine AS build
WORKDIR /app

# Copiar archivos de dependencias primero (cache)
COPY package*.json ./

# Instalar dependencias
RUN npm ci --silent

# Copiar código fuente
COPY . .

# Build de producción
RUN npm run build

# --- STAGE 2: Nginx ---
FROM nginx:alpine AS runtime

# Copiar configuración de Nginx
COPY nginx.conf /etc/nginx/nginx.conf

# Copiar artefactos del build
COPY --from=build /app/dist/parhelion-admin/browser /usr/share/nginx/html

# Puerto
EXPOSE 80

# Comando de inicio
CMD ["nginx", "-g", "daemon off;"]
