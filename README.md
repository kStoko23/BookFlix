# BookFlix

![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Minimal%20APIs-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Angular](https://img.shields.io/badge/Angular-21-DD0031?style=for-the-badge&logo=angular&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-3178C6?style=for-the-badge&logo=typescript&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-18-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)
![Entity Framework Core](https://img.shields.io/badge/EF%20Core-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![Tailwind CSS](https://img.shields.io/badge/Tailwind%20CSS-4-06B6D4?style=for-the-badge&logo=tailwindcss&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-Auth-000000?style=for-the-badge&logo=jsonwebtokens&logoColor=white)
![xUnit](https://img.shields.io/badge/xUnit-Tests-5E2B97?style=for-the-badge)
![Testcontainers](https://img.shields.io/badge/Testcontainers-Integration%20Tests-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![GitHub Actions](https://img.shields.io/badge/GitHub%20Actions-CI-2088FF?style=for-the-badge&logo=githubactions&logoColor=white)
![Scalar](https://img.shields.io/badge/Scalar-OpenAPI-0F172A?style=for-the-badge)

<sub>
Who doesn't love badges, right?
</sub>

---

A full-stack demo application, a book library in style of Netflix. Built for learning purposes, demonstrating JWT authentication, RESTful CRUD APIs, and a modern SPA frontend. I wanted to learn fullstack development and decided on Angular & .NET. Used both of them at Uni and enjoyed working with them for no particular reasons, I guess that's because they were my first technologies used so that's probably why. 

I tried to not use AI first, but develop as much as I can by myself (again to learn that stuff better), but there were situations that Claude did a good chunk of work. Mainly I used him for styling & design cues, because I didn't want to conecrn myself a lot about those  with this project. My main goal was to learn Angular & .NET, so I gave up styling to LLM so I could focus on working on core funcionality. I also used Claude during implementing validation, to check if I missed some case that data would be invalid, but could pass validation. Other than that I asked some general questions about proper structurization of a project (both Api and Web part) and some weird bugs I encountered and couldn't figure out on my own.

I also developed a test suite for the backend part, including both unit and integration tests. I tried to cover as many cases as I could think of and overall I think I did a good job at that. All endpoints, validators and services are covered with tests, all passing. In total there are 150 tests in `Api.Tests/`.

---

## Purpose

BookFlix lets authenticated users maintain their own book collection: add books with metadata (title, author, category, ISBN, rating), browse, edit, and delete them. Each user's library is public for other users and protected against other users deleting/editing with JWT. The project exists as a learning reference for:

- Building a secure REST API with ASP.NET Core minimal APIs
- Implementing JWT-based authentication with refresh tokens
- Consuming a backend from an Angular SPA with HTTP interceptors, route guards
- Containerising a multi-service app with Docker Compose

---

## Technology Stack

### Backend (`Api/`)

| Concern              | Choice                               |
| -------------------- | ------------------------------------ |
| Runtime              | .NET 10 / ASP.NET Core               |
| API style            | Minimal APIs                         |
| ORM                  | Entity Framework Core 10             |
| Database             | PostgreSQL 18                        |
| Auth                 | JWT Bearer (HMAC-SHA256)             |
| Password hashing     | BCrypt.Net                           |
| API documentation    | Scalar OpenAPI                       |
| Rate limiting        | ASP.NET Core built-in (fixed window) |
| Testing              | xUnit, Testcontainers, ASP.NET Core MVC Testing |
| CI                   | GitHub Actions                       |

### Frontend (`Web/`)

| Concern   | Choice         |
| --------- | -------------- |
| Framework | Angular 21     |
| Styling   | Tailwind CSS 4 |

## Architecture

```
┌─────────────────────────────────────────────────────┐
│  Browser                                            │
│  Angular SPA  (localhost:4200 → nginx:80)           │
│       │                                             │
│       │  /api/* (proxied by nginx)                  │
│       ▼                                             │
│  ASP.NET Core API  (api:8080)                       │
│       │                                             │
│       │  EF Core                                    │
│       ▼                                             │
│  PostgreSQL  (postgres:5432)                        │
└─────────────────────────────────────────────────────┘
```

### Key decisions

**Minimal APIs over controllers** - The backend uses ASP.NET Core's minimal API style (`MapBooksEndpoints`, `MapAuthEndpoints`). As it was my first time working with ASP.NET Core I didn't want to overcomplicate things for myself. Also, it's a fairly simple API that shouldn't require more advanced approaches.

**JWT without ASP.NET Identity** - Auth is handled manually via a `JwtService` that signs tokens with `HMAC-SHA256` and passwords are hashed with BCrypt. I wanted to implement basic JWT auth by myself as much as I could and wantd to limit abstracion, to learn more about the basics.

**Refresh token via HttpOnly Cookie** - Refresh tokens are stored server-side in the database as BCrypt hashes and passed to the client exclusively via an `HttpOnly`, `SameSite=Strict`, `Secure` (in prod env) cookie, making them inaccessible to JavaScript and significantly reducing XSS exposure. The cookie is scoped to `/api/auth`. Each refresh rotates the token - the old one is revoked and a new one is issued - and if a previously revoked token is ever reused, all active tokens for that user are immediately invalidated as a theft detection measure.

**Functional Angular interceptor** - The frontend attaches the JWT from `localStorage` to every outgoing request via a standalone `authInterceptor` function registered in `app.config.ts`. Route guards protect authenticated pages.

**Rate limiting on auth endpoints** - A stricter fixed-window limiter (5 req/min) is applied to `/auth/*` to mitigate brute-force attempts, while general API traffic allows 100 req/min.

**Data validation** - I tried to protect backend from errors as much as I could how to, so I used seperate `BookValidator.cs` and `AuthValdiator.cs` classes t validate input data, before any operations with DB. I might have missed some, but I don't think I did. PS. Validating ISBN is quite complex, so I opted for _not so perfect_ Regex pattern, that checkes the general structure, but doesn' check the checksum. For the purposes of this app it's sufficient, but I'm aware it could be added.

**Global exception handling** - Unexpected exceptions are handled by a centralized exception handler returning RFC7807 ProblemDetails responses. This keeps endpoint implementations cleaner and ensures clients always receive a consistent error format without exposing internal details.

**Scalar for API documentation** - Instead of Swagger UI the project uses Scalar as the OpenAPI client. I simply find it cleaner and nicer to work with during development while still relying on the generated OpenAPI specification underneath.

**Continuous Integration** - Every push and pull request triggers a GitHub Actions workflow that restores dependencies, builds the solution, runs the complete test suite and publishes the API artifact. I wanted every commit on the main branch to always represent a working application.

---

## Running with Docker

```bash
docker compose up --build
```

| Service    | URL                   |
| ---------- | --------------------- |
| Frontend   | http://localhost:4200 |
| API        | http://localhost:8080 |
| PostgreSQL | localhost:5432        |

The API applies EF Core migrations on startup so the database schema is created automatically.
When the API is running, interactive documentation is available at

```bash
http://localhost:8080/scalar/v1
```
---

## Running locally (development)

**Backend**

```bash
cd Api
dotnet run
# API available at http://localhost:8080
```

**Frontend**

```bash
cd Web
npm install
ng build && ng serve
# App available at http://localhost:4200
```

The development frontend points directly at `http://localhost:8080/api` (see `environment.ts`) so no proxy is needed locally.

---

## Project structure
```
BookFlix/
├── Api/                  # ASP.NET Core backend
│   ├── Entities/         # Book, BookCategory enum and User entity
│   ├── Features/         # Auth & Book endpoints, with response models
│   ├── Data/             # EF Core DbContext, configurations
│   ├── Migrations/       # Migrations
│   ├── Program.cs
│   └── Dockerfile
├── Api.Tests/            # Api test suite
│   ├── Integration/      # Integration tests using Testcontainers + PostgreSQL
│   │   ├── Auth/         # Login, Register, Refresh, Logout endpoint tests
│   │   ├── Books/        # CRUD endpoint tests for all book endpoints
│   │   ├── BooksApiFactory.cs    # WebApplicationFactory with Testcontainers setup
│   │   └── IntegrationTestBase.cs # Shared base class with helpers and lifecycle
│   ├── Unit/
│   │   ├── AuthValidatorTests.cs # AuthValidator tests
│   │   ├── BookValidatorTests.cs # BookValidator tests
│   │   └── JwtTests.cs           # JwtService tests
├── Web/                  # Angular frontend
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/     # Auth interceptor, guards, services, data models
│   │   │   ├── features/ # Books, home, auth, profile pages
│   │   │   └── shared/   # Reusable components, pipes
│   │   └── environments/
│   ├── nginx.conf
│   └── Dockerfile
└── compose.yaml
```

---

## Future improvements

There are still plenty of things I'd like to improve as I continue learning:

- Email verification and password reset
- Role-based authorization
- Search, filtering and pagination
- Media uploads & storage
- Caching
- Background jobs

Will most likely expand on those in future project with the same stack, bu using Controllers instead of MinimalAPIs this time.
