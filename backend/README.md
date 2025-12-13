# Backend - Parhelion API

**Stack:** C# / .NET 8 Web API  
**Patrón:** Clean Architecture  
**Base de Datos:** PostgreSQL + Entity Framework Core

## Estructura Planificada

```
backend/
├── src/
│   ├── Parhelion.Domain/           # Entidades, Enums, Exceptions
│   ├── Parhelion.Application/      # DTOs, Interfaces, Validators
│   ├── Parhelion.Infrastructure/   # EF Core, Repositorios, Services
│   └── Parhelion.API/              # Controllers, JWT, Swagger
└── tests/
    └── Parhelion.Tests/
```

## Documentación API

Swagger disponible en: `/swagger`
