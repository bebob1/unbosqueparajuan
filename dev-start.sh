#!/bin/bash
# Script de inicio para el proyecto Umbraco
# Creado por IMERGI

echo "🚀 Iniciando Umbraco en Docker..."
echo ""
echo "📌 El proyecto estará disponible en: http://localhost:5001"
echo "📌 Para detener: Ctrl+C o 'docker compose down' en otra terminal"
echo ""

# Verificar si el puerto 5001 está ocupado
if lsof -Pi :5001 -sTCP:LISTEN -t >/dev/null 2>&1 ; then
    echo "⚠️  ADVERTENCIA: El puerto 5001 ya está en uso."
    echo "Procesos usando el puerto 5001:"
    lsof -Pi :5001 -sTCP:LISTEN
    echo ""
    echo "¿Querés matarlo y continuar? (yes/no)"
    read -r response
    if [[ "$response" == "yes" ]]; then
        echo "🔫 Matando proceso en puerto 5001..."
        lsof -ti :5001 | xargs kill -9
        sleep 2
    else
        echo "Operación cancelada."
        exit 1
    fi
fi

# Iniciar docker compose (v2 - sin guion)
docker compose up --build
