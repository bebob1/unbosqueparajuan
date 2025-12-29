#!/bin/bash
# Script para resetear completamente la base de datos y volúmenes de Umbraco
# Creado por IMERGI

echo "⚠️  ADVERTENCIA: Esto va a borrar TODA la base de datos y archivos media."
echo "¿Estás seguro? (yes/no)"
read -r response

if [[ "$response" != "yes" ]]; then
    echo "Operación cancelada."
    exit 0
fi

echo "🧹 Deteniendo contenedores..."
docker compose down

echo "🗑️  Eliminando volúmenes de datos..."
docker volume rm umbraco17_umbraco-data 2>/dev/null || true
docker volume rm umbraco17_umbraco-media 2>/dev/null || true

echo "🧼 Limpiando archivos SQLite locales si existen..."
rm -f src/App_Data/*.db 2>/dev/null || true
rm -f src/App_Data/*.sqlite* 2>/dev/null || true
rm -f src/umbraco/Data/*.db 2>/dev/null || true
rm -f src/umbraco/Data/*.sqlite* 2>/dev/null || true

echo "✅ Reset completado. Ahora podés ejecutar: docker compose up --build"
