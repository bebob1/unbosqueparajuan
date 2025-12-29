#!/bin/bash
# Script para verificar que tenés todo lo necesario instalado
# Creado por IMERGI

echo "🔍 Verificando requisitos del proyecto..."
echo ""

# Verificar Docker
if command -v docker &> /dev/null; then
    DOCKER_VERSION=$(docker --version)
    echo "✅ Docker instalado: $DOCKER_VERSION"
else
    echo "❌ Docker NO está instalado"
    echo "   Instalá desde: https://www.docker.com/get-started"
    exit 1
fi

# Verificar Docker Compose v2
if docker compose version &> /dev/null; then
    COMPOSE_VERSION=$(docker compose version)
    echo "✅ Docker Compose v2 disponible: $COMPOSE_VERSION"
elif command -v docker-compose &> /dev/null; then
    COMPOSE_VERSION=$(docker-compose --version)
    echo "⚠️  Tenés Docker Compose v1 (deprecated): $COMPOSE_VERSION"
    echo "   Se recomienda actualizar a Docker Compose v2"
    echo "   Los scripts de este proyecto usan 'docker compose' (sin guion)"
else
    echo "❌ Docker Compose NO está instalado"
    exit 1
fi

# Verificar que Docker esté corriendo
if docker info &> /dev/null; then
    echo "✅ Docker daemon está corriendo"
else
    echo "❌ Docker daemon NO está corriendo"
    echo "   Iniciá Docker Desktop o el servicio de Docker"
    exit 1
fi

# Verificar puerto 5001
if lsof -Pi :5001 -sTCP:LISTEN -t >/dev/null 2>&1; then
    echo "⚠️  Puerto 5001 está ocupado por:"
    lsof -Pi :5001 -sTCP:LISTEN
else
    echo "✅ Puerto 5001 está disponible"
fi

echo ""
echo "🎉 Todo listo! Podés ejecutar: ./dev-start.sh"
