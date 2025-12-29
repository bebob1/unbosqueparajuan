# Cambios Realizados - Configuración Multiplataforma

Este documento resume todos los cambios realizados para hacer que Umbraco 17 funcione correctamente en Docker de forma multiplataforma (especialmente macOS).

Desarrollado por **IMERGI** - Diciembre 2024

---

## 🎯 Objetivo

Configurar Umbraco 17 CMS para que funcione en Docker de forma consistente en:
- macOS (Intel y Apple Silicon/ARM64)
- Linux (x86_64 y ARM64)
- Windows

Con soporte completo para:
- Hot reload (`dotnet watch`)
- SQLite como base de datos
- Desarrollo en tiempo real
- Sin conflictos de permisos o compatibilidad

---

## 📝 Problemas Encontrados y Soluciones

### 1. Puerto 5000 Ocupado en macOS

**Problema**: macOS Monterey y superiores usan el puerto 5000 para AirPlay Receiver por defecto.

**Solución**:
- Cambiado puerto de `5000` → `5001` en:
  - `docker-compose.yml`: `ports: - "5001:5001"`
  - `Dockerfile`: Script de inicio usa `--urls http://+:5001`
  - `src/Properties/launchSettings.json`: `applicationUrl: "http://localhost:5001"`

**Archivos modificados**:
- `docker-compose.yml`
- `Dockerfile`
- `README.md`

---

### 2. SQLite Incompatible Entre Arquitecturas

**Problema**: Archivos `.sqlite.db` creados en Linux x86_64 no funcionan en macOS ARM64 (Apple Silicon) debido a diferencias en:
- Arquitectura del procesador
- File locking del filesystem
- Permisos (UID/GID vs ACLs)

**Solución**:
- Base de datos vive en **volumen Docker** en lugar de filesystem del host
- Archivos media también en volumen Docker
- Path absoluto en lugar de `|DataDirectory|`

**Cambios en `docker-compose.yml`**:
```yaml
volumes:
  - ./src:/app/project:delegated  # Código fuente
  - umbraco-data:/app/project/App_Data  # BD SQLite (volumen Docker)
  - umbraco-media:/app/project/wwwroot/media  # Media (volumen Docker)
  - umbraco-packages:/root/.nuget/packages  # Caché NuGet
```

**Cambios en `src/appsettings.json`**:
```json
"ConnectionStrings": {
  "umbracoDbDSN": "Data Source=/app/project/App_Data/Umbraco.sqlite.db;Cache=Shared;Foreign Keys=True;Pooling=True;Mode=ReadWriteCreate",
  "umbracoDbDSN_ProviderName": "Microsoft.Data.Sqlite"
}
```

**Archivos modificados**:
- `docker-compose.yml`
- `src/appsettings.json`

---

### 3. Directorio Residual `UmbracoSite/`

**Problema**: El template de Umbraco creó un subdirectorio `src/UmbracoSite/` que no se limpió correctamente, causando conflictos en el build.

**Solución**:
- Eliminado manualmente el directorio `src/UmbracoSite/`
- Actualizado `.dockerignore` para ignorar directorios de build

**Comando ejecutado**:
```bash
rm -rf src/UmbracoSite src/bin src/obj
```

**Archivos modificados**:
- `.dockerignore` (creado nuevo)

---

### 4. Conflicto de Nombres en MSBuild

**Problema**: MSBuild intentaba crear un archivo ejecutable llamado `UmbracoSite` en `bin/Debug/net10.0/UmbracoSite`, pero ya existía un directorio con ese nombre, causando error:
```
error MSB3024: Could not copy file "apphost" to "bin/Debug/net10.0/UmbracoSite", 
because the destination is a folder instead of a file
```

**Solución**:
- Cambiado el nombre del assembly en el `.csproj` a `UmbracoApp`
- Esto hace que el ejecutable se llame `UmbracoApp.dll` en lugar de `UmbracoSite.dll`

**Cambios en `src/UmbracoSite.csproj`**:
```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <!-- ... otros settings ... -->
  <!-- Workaround para Docker + macOS build issue: rename the output executable -->
  <AssemblyName>UmbracoApp</AssemblyName>
</PropertyGroup>
```

**Archivos modificados**:
- `src/UmbracoSite.csproj`

---

### 5. Script de Inicio Mejorado

**Problema**: El script de inicio necesitaba:
- Limpiar builds anteriores
- Manejar permisos correctamente
- Usar `dotnet watch` para hot reload
- Debug verbose para troubleshooting

**Solución**:
- Script actualizado con `set -x` para debug
- Limpieza de `bin/obj` antes de arrancar
- Permisos 777 en `App_Data` y `wwwroot/media`
- `exec` para que dotnet watch sea PID 1

**Cambios en `Dockerfile`**:
```bash
#!/bin/bash
set -x  # Debug mode

cd /app/project

# ... verificaciones ...

# Limpiar build artifacts anteriores
rm -rf bin obj 2>/dev/null || true

# Permisos
chmod -R 777 /app/project/App_Data 2>/dev/null || true
chmod -R 777 /app/project/wwwroot/media 2>/dev/null || true

# Iniciar con dotnet watch
exec dotnet watch run --no-restore --urls "http://+:5001" --verbose
```

**Archivos modificados**:
- `Dockerfile`

---

### 6. Docker Compose v2

**Problema**: El proyecto usaba referencias a `docker-compose` (v1, deprecated) en lugar de `docker compose` (v2, actual).

**Solución**:
- Actualizados todos los scripts y documentación para usar `docker compose`
- Agregada nota en README sobre la diferencia

**Archivos modificados**:
- `dev-start.sh`
- `reset-db.sh`
- `README.md`

---

## 🆕 Archivos Nuevos Creados

### 1. `.dockerignore`
Evita copiar archivos innecesarios al build de Docker:
- Archivos de BD SQLite (*.db, *.sqlite)
- Directorios de build (bin/, obj/)
- Archivos de sistema (.DS_Store)
- Logs

### 2. `check-requirements.sh`
Script helper para verificar requisitos antes de arrancar:
- Docker instalado y corriendo
- Docker Compose v2 disponible
- Puerto 5001 libre

### 3. `dev-start.sh`
Script helper para iniciar el proyecto:
- Verifica si puerto 5001 está ocupado
- Ofrece matar el proceso automáticamente
- Ejecuta `docker compose up --build`

### 4. `reset-db.sh`
Script para resetear base de datos y media:
- Detiene contenedores
- Elimina volúmenes Docker
- Limpia archivos SQLite locales si existen

---

## 📦 Volúmenes Docker

### Configuración Final

```yaml
volumes:
  umbraco-packages:  # Caché de NuGet packages
  umbraco-data:      # Base de datos SQLite
  umbraco-media:     # Archivos media subidos
```

### ¿Por Qué Volúmenes en Lugar de Bind Mounts?

**Ventajas**:
1. **Permisos consistentes**: No hay conflictos entre Linux (contenedor) y macOS (host)
2. **Performance**: Mucho más rápido en macOS que compartir archivos del host
3. **Compatibilidad**: SQLite funciona igual en todas las arquitecturas
4. **Aislamiento**: Los datos están protegidos dentro de Docker

**Desventajas**:
1. No podés abrir el archivo SQLite directamente con DB Browser u otras herramientas en tu Mac
2. Para backups necesitás copiar desde el volumen Docker

**Cómo Acceder a los Datos**:
```bash
# Copiar BD desde el volumen
docker cp umbraco17-umbraco-1:/app/project/App_Data/Umbraco.sqlite.db ./backup.db

# Copiar BD hacia el volumen
docker cp ./backup.db umbraco17-umbraco-1:/app/project/App_Data/Umbraco.sqlite.db
```

---

## 🔥 Hot Reload Configurado

El proyecto usa `dotnet watch` que detecta cambios en:
- **Archivos C# (*.cs)**: Recompila y recarga automáticamente
- **Archivos Razor (*.cshtml)**: Recarga sin rebuild
- **Archivos CSS/JS**: Recarga automática

**NO detecta cambios en**:
- `appsettings.json` → Requiere `docker compose restart umbraco`
- `*.csproj` → Requiere `docker compose restart umbraco`
- Archivos en `bin/` u `obj/` → Gestionados por dotnet

---

## 🎨 Mejoras al README

- Agregada sección de **Características**
- Documentados todos los **Problemas Comunes** con soluciones
- Sección de **Notas Técnicas** explicando la implementación
- Estructura de **Volúmenes** documentada
- Sección de **Hot Reload** con detalles
- **Changelog** agregado
- Instrucciones más claras y concisas

---

## 🧪 Testing Realizado

El proyecto fue testeado exitosamente en:

- ✅ **macOS 14 Sonoma (Apple Silicon M1/M2)**: Funcionando perfectamente
- ✅ **Docker Desktop 28.2.2**: Versión verificada
- ✅ **Docker Compose v2.39.2**: Verificado
- ✅ **Puerto 5001**: Sin conflictos
- ✅ **Hot Reload**: Detecta cambios correctamente
- ✅ **SQLite**: Base de datos se crea y funciona
- ✅ **Umbraco Install**: Wizard funciona correctamente

---

## 📊 Resumen de Cambios por Archivo

| Archivo | Acción | Razón |
|---------|--------|-------|
| `docker-compose.yml` | Modificado | Puerto 5001, volúmenes Docker |
| `Dockerfile` | Modificado | Script mejorado, puerto 5001 |
| `src/appsettings.json` | Modificado | Path absoluto para SQLite |
| `src/UmbracoSite.csproj` | Modificado | AssemblyName=UmbracoApp |
| `.dockerignore` | Creado | Evitar copiar archivos innecesarios |
| `check-requirements.sh` | Creado | Helper para verificar requisitos |
| `dev-start.sh` | Creado | Helper para iniciar proyecto |
| `reset-db.sh` | Creado | Helper para resetear BD |
| `README.md` | Actualizado | Documentación completa |
| `src/UmbracoSite/` | Eliminado | Directorio residual que causaba conflictos |
| `src/bin/`, `src/obj/` | Eliminados | Se regeneran en cada build |

---

## 🚀 Comandos Útiles

```bash
# Iniciar proyecto
./dev-start.sh

# Verificar requisitos
./check-requirements.sh

# Resetear BD
./reset-db.sh

# Ver logs en vivo
docker compose logs -f umbraco

# Reiniciar después de cambios en appsettings
docker compose restart umbraco

# Limpiar TODO y empezar de cero
docker compose down -v --rmi all
rm -rf src/bin src/obj
docker compose up --build
```

---

## 💡 Lecciones Aprendidas

1. **Volúmenes Docker son esenciales** para compatibilidad multiplataforma con SQLite
2. **AssemblyName personalizado** evita conflictos de nombres en builds
3. **Puerto 5001** evita conflictos con servicios del sistema en macOS
4. **`dotnet watch`** requiere configuración especial en Docker
5. **Permisos 777** en directorios críticos evita problemas en macOS
6. **Path absoluto** es más confiable que `|DataDirectory|` en Docker
7. **Scripts helpers** mejoran enormemente la experiencia de desarrollo

---

**Desarrollado por IMERGI** | Diciembre 2024
