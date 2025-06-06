# ADM Proyectos - Sistema de Administración de Proyectos

## Descripción
Sistema de administración de proyectos desarrollado con arquitectura limpia (.NET), que permite gestionar propuestas de proyectos con flujos de aprobación personalizables.

## Estructura del Proyecto
- **ADMProyectos**: Aplicación de consola (proyecto principal)
- **API**: Web API para servicios REST
- **Application**: Lógica de aplicación y servicios
- **Domain**: Entidades del dominio
- **Infrastructure**: Acceso a datos y persistencia

## Requisitos Previos
- Visual Studio 2022 o superior
- .NET 8.0 SDK
- SQL Server (LocalDB o instancia completa)
- Entity Framework Core Tools

## Instrucciones de Configuración

### 1. Configuración de Base de Datos

#### Paso 1: Configurar la cadena de conexión
1. Abrir el archivo `API/appsettings.json`
2. Verificar/modificar la cadena de conexión según tu entorno:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ADMProyectosDB;Integrated Security=True"
  }
}
```

#### Paso 2: Generar y aplicar migraciones
1. Abrir la solución `ADMProyectos.sln` en Visual Studio
2. **IMPORTANTE**: Establecer temporalmente **API** como proyecto de inicio:
   - Clic derecho en el proyecto **API** → "Set as Startup Project"
3. Abrir la Consola del Administrador de Paquetes:
   - Tools → NuGet Package Manager → Package Manager Console
4. **IMPORTANTE**: Seleccionar **Infrastructure** como "Default project" en el dropdown de la consola
5. Ejecutar los siguientes comandos:
```
Add-Migration Init
Update-Database
```

### 2. Configuración para Ejecución

#### Paso 3: Configurar proyecto de inicio para la aplicación
1. **IMPORTANTE**: Cambiar el proyecto de inicio a **ADMProyectos** (aplicación de consola):
   - Clic derecho en el proyecto **ADMProyectos** → "Set as Startup Project"
2. Compilar la solución: Build → Build Solution (Ctrl+Shift+B)

## Ejecución del Proyecto

### Ejecutar la Aplicación de Consola
1. Presionar F5 o Ctrl+F5 para ejecutar
2. La aplicación mostrará un menú interactivo en consola
3. Seguir las opciones del menú para:
   - Crear propuestas de proyectos
   - Ver propuestas existentes
   - Gestionar pasos de aprobación
   - Actualizar y eliminar proyectos

### Ejecutar la API (Opcional)
Si desea probar la API por separado:
1. Establecer **API** como proyecto de inicio
2. Ejecutar con F5 (se abrirá Swagger UI)
3. La API estará disponible en: `https://localhost:7xxx` o `http://localhost:5xxx`

## Funcionalidades Principales

### Gestión de Proyectos
- ✅ Crear propuestas de proyectos
- ✅ Visualizar propuestas existentes
- ✅ Actualizar información de proyectos
- ✅ Eliminar proyectos
- ✅ Gestión de pasos de aprobación

### Flujo de Aprobación
- ✅ Generación automática de pasos de aprobación según reglas de negocio
- ✅ Validación de roles de usuario
- ✅ Estados de proyecto (Observado, Pendiente, Aprobado, Rechazado)

## Arquitectura

### Capas del Sistema
- **Presentation (ADMProyectos)**: Interfaz de usuario por consola
- **API**: Controladores REST
- **Application**: Casos de uso, servicios y DTOs
- **Domain**: Entidades de negocio
- **Infrastructure**: Repositorios, contexto de BD y migraciones

### Patrones Implementados
- Repository Pattern
- Command Query Responsibility Segregation (CQRS)
- Dependency Injection
- Clean Architecture

## Resolución de Problemas

### Error de Conexión a Base de Datos
- Verificar que SQL Server esté ejecutándose
- Confirmar la cadena de conexión en `appsettings.json`
- Asegurar que la base de datos se haya creado con `Update-Database`

### Error de Migraciones
- Eliminar la carpeta `Migrations` en el proyecto Infrastructure
- Ejecutar nuevamente `Add-Migration Init` y `Update-Database`

### Proyecto no Inicia Correctamente
- Verificar que **ADMProyectos** (no API) esté configurado como proyecto de inicio
- Compilar toda la solución antes de ejecutar

## Notas Importantes

⚠️ **CONFIGURACIÓN CRÍTICA**:
1. Para migraciones: API como startup project + Infrastructure como default project
2. Para ejecución: ADMProyectos como startup project

⚠️ **TIPO DE APLICACIÓN**: 
Este es un proyecto de **aplicación de consola**, no una aplicación web.

---