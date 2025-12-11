# GitHub Actions - CI/CD Pipeline

## Workflows Planificados

### 🔧 Backend (.NET 8)

| Archivo              | Trigger                       | Acciones                            |
| :------------------- | :---------------------------- | :---------------------------------- |
| `backend-ci.yml`     | Push a `develop`, PR a `main` | Build, Test, Lint                   |
| `backend-deploy.yml` | Push a `main`                 | Build Docker, Deploy a DigitalOcean |

### 🎨 Frontend Admin (Angular)

| Archivo            | Trigger                       | Acciones                           |
| :----------------- | :---------------------------- | :--------------------------------- |
| `admin-ci.yml`     | Push a `develop`, PR a `main` | Build, Lint, Test                  |
| `admin-deploy.yml` | Push a `main`                 | Build, Deploy a DigitalOcean/Nginx |

### 📱 Frontend Operaciones (React PWA)

| Archivo                  | Trigger                       | Acciones          |
| :----------------------- | :---------------------------- | :---------------- |
| `operaciones-ci.yml`     | Push a `develop`, PR a `main` | Build, Lint       |
| `operaciones-deploy.yml` | Push a `main`                 | Build PWA, Deploy |

### 📲 Frontend Campo (React PWA)

| Archivo            | Trigger                       | Acciones          |
| :----------------- | :---------------------------- | :---------------- |
| `campo-ci.yml`     | Push a `develop`, PR a `main` | Build, Lint       |
| `campo-deploy.yml` | Push a `main`                 | Build PWA, Deploy |

---

## Dominios Objetivo

| Servicio             | URL                      |
| :------------------- | :----------------------- |
| API Backend          | `api.macrostasis.lat`    |
| Frontend Admin       | `admin.macrostasis.lat`  |
| Frontend Operaciones | `ops.macrostasis.lat`    |
| Frontend Campo       | `driver.macrostasis.lat` |

---

**Nota:** Los workflows se implementarán cuando los proyectos estén configurados.
