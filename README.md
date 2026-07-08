# Task Management API

## Project Overview
This project is a robust Task Management Backend API built with ASP.NET Core. It follows a lightweight, Domain-Driven Design (DDD)-style Clean Architecture to ensure separation of concerns, high maintainability, and scalability.

## Technologies Used
- ASP.NET Core Web API
- .NET
- Entity Framework Core
- ASP.NET Identity
- JWT Authentication
- SQLite
- Redis
- Docker Compose
- Swagger

## Architecture
The solution is structured into four logical layers (projects/folders) following Clean Architecture principles:

- **API (Controllers & Program.cs)**: The entry point of the application. It handles HTTP requests, routing, JWT authentication, and global exception handling. It depends on the Application layer and contains no business logic.
- **Application**: Contains the core use cases, service implementations, DTOs, and repository interfaces. This layer orchestrates business processes and acts as a bridge between the API and Domain.
- **Domain**: The core of the system. It contains enterprise logic, entities, enums, and domain exceptions. This layer has absolutely no external dependencies.
- **Infrastructure**: Responsible for external concerns such as data access (Entity Framework Core DbContext and repository implementations), Redis caching integration, and background processing services.

## Features
- User Registration
- User Login
- JWT Authentication
- Role-based Authorization
- Admin User Management
- Soft Delete
- Task Management
- Ownership Authorization
- Background Processing using `BackgroundService`
- Redis Caching
- Business Rules (Duplicate Prevention & Sorting)
- Swagger Documentation

## Project Structure
```text
.Net-Task/
├── Controllers/         # API Layer: Endpoints
├── Application/         # Application Layer: Services, DTOs, Interfaces
├── Domain/              # Domain Layer: Entities, Enums
├── Infrastructure/      # Infrastructure Layer: EF Core, Repositories, Background Jobs
├── Middleware/          # Global Exception Handling
├── Migrations/          # EF Core Database Migrations
├── appsettings.json     # Application Configuration
├── docker-compose.yml   # Redis Container Configuration
└── Program.cs           # Application Entry Point & DI Container
```

## Setup Instructions

Follow these steps to get the project up and running locally:

### 1. Clone the repository
```bash
git clone <repository-url>
cd .Net-Task
```

### 2. Restore packages
```bash
dotnet restore
```

### 3. Apply migrations
Ensure the SQLite database is created and up to date:
```bash
dotnet ef database update
```
*(Note: The application is also configured to seed the database on startup automatically.)*

### 4. Start Redis using Docker Compose
Ensure Docker Desktop is running, then spin up the Redis container:
```bash
docker compose up -d
```

### 5. Run the application
```bash
dotnet run
```
The application will be accessible at `https://localhost:<port>` (the exact port will be shown in the console output).

## Seeded Admin User
Upon the first run, the database is automatically seeded with an administrator account for immediate testing:
- **Email**: test@test.com
- **Password**: 1234

## Redis
The application leverages Redis to improve the performance of read-heavy operations:
- **Cache-Aside Pattern**: Redis is used as a distributed cache utilizing the cache-aside pattern.
- **Get Task by ID Caching**: Requests to fetch a specific task are intercepted. If the task exists in the cache, it is returned immediately. Otherwise, it is fetched from the database, serialized, and cached for 10 minutes.
- **Cache Invalidation**: Whenever a task is updated (e.g., status changes), the corresponding cache entry is aggressively invalidated to prevent returning stale data.

## Background Processing
- Tasks are placed into an in-memory queue immediately after creation.
- A hosted `BackgroundService` continuously listens to this queue and processes tasks asynchronously in the background.
- Processing is currently simulated by updating the task status after a short artificial delay, representing a heavy computational workload or third-party API integration.

## Business Logic
The task module enforces several critical business rules securely and efficiently:
- **Duplicate Task Prevention**: A user cannot create a task with the exact same title on the exact same calendar day.
- **Sorting by Priority and CreatedAt**: Task retrieval is strictly ordered by highest Priority first, followed by newest CreatedAt date.

**Query Optimization**: `LINQ` and `IQueryable` were intentionally used so that filtering and sorting mechanisms translate natively to SQL and execute entirely at the database level. This guarantees efficient queries and completely avoids inefficient in-memory materialization.

## API Documentation
Swagger UI is enabled and integrated out of the box. 
- Navigate to the `/swagger` endpoint when running the application to view all available routes.
- **Testing**: JWT authentication is supported directly within Swagger. You can authenticate via the Login endpoint, copy the token, and use the "Authorize" button at the top to test secured endpoints interactively.

## Assumptions
- **Architecture**: The Clean Architecture separation is implemented logically using folders within a single `.csproj` monolith to keep the codebase lightweight for assessment purposes, rather than maintaining four separate physical class library projects.
- **Database**: SQLite is used for simplicity and portability in the development environment. In production, a more robust RDBMS (e.g., PostgreSQL or SQL Server) would be swapped in via Dependency Injection.
- **Graceful Degradation**: If the Redis container is unavailable, the system assumes it should fail gracefully. It catches connection exceptions, logs a warning, and falls back to fetching data directly from the SQLite database to ensure API availability.
