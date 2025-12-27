# Umbraco 17 CMS - Docker Development Environment

Este proyecto contiene una instalación de **Umbraco 17 CMS** configurada para ejecutarse en un entorno Docker.

## 📋 Requisitos Previos

- [Docker](https://www.docker.com/get-started) (versión 20.10 o superior)
- [Docker Compose](https://docs.docker.com/compose/install/) (versión 2.0 o superior)

## 🚀 Inicio Rápido

### 1. Clonar el Repositorio

```bash
git clone <url-del-repositorio>
cd Umbraco
```

### 2. Iniciar el Contenedor

```bash
docker-compose up
```

El contenedor se iniciará y Umbraco estará disponible en:

**🌐 http://localhost:5000**

### 3. Configuración Inicial de Umbraco

La primera vez que accedas a la aplicación, se te pedirá completar el asistente de instalación:

1. Ingresa los datos del administrador (nombre, email, contraseña)
2. Umbraco creará automáticamente la base de datos SQLite
3. Una vez completado, podrás acceder al backoffice en: **http://localhost:5000/umbraco**

## 📁 Estructura del Proyecto

```
Umbraco/
├── Dockerfile              # Imagen Docker con .NET 10 SDK y Umbraco
├── docker-compose.yml      # Configuración de servicios Docker
├── start.sh                # Script de inicialización (referencia)
├── Umbraco.sln            # Solución de Visual Studio
└── src/                   # Código fuente de la aplicación
    ├── UmbracoSite.csproj # Proyecto .NET
    ├── Program.cs         # Punto de entrada de la aplicación
    ├── appsettings.json   # Configuración principal
    ├── appsettings.Development.json
    ├── App_Data/          # Datos de la aplicación (SQLite DB)
    ├── Models/            # Modelos generados
    ├── Views/             # Vistas Razor
    ├── wwwroot/           # Archivos estáticos y media
    └── umbraco/           # Archivos del CMS
```

## ⚙️ Configuración

### Base de Datos

El proyecto utiliza **SQLite** como base de datos por defecto, configurada en `appsettings.json`:

```json
"ConnectionStrings": {
  "umbracoDbDSN": "Data Source=|DataDirectory|/Umbraco.sqlite.db;Cache=Shared;Foreign Keys=True;Pooling=True",
  "umbracoDbDSN_ProviderName": "Microsoft.Data.Sqlite"
}
```

La base de datos se almacena en `src/umbraco/Data/Umbraco.sqlite.db` y persiste entre reinicios del contenedor gracias al volumen montado.

### Variables de Entorno

El contenedor está configurado con las siguientes variables de entorno:

- `ASPNETCORE_ENVIRONMENT=Development`
- `ASPNETCORE_URLS=http://+:5000`
- `Umbraco__CMS__Global__UseHttps=false`

### Puertos

- **Puerto 5000**: Aplicación Umbraco (HTTP)
