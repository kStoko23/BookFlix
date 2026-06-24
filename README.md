# BookFlix

A full-stack demo application, a book library in style of Netflix. Built for learning purposes, demonstrating JWT authentication, RESTful CRUD APIs, and a modern SPA frontend. I wanted to learn fullstack development and decided on Angular & .NET. Used both of them at Uni and enjoyed working with them for no particular reasons, I guess that's because they were my first technologies used so that's probably why. 

I tried to not use AI first, but develop as much as I can by myself (again to learn that stuff better), but there were situations that Claude did a good chunk of work. Mainly I used him for styling & design cues, because I didn't want to conecrn myself a lot about those  with this project. My main goal was to learn Angular & .NET, so I gave up styling to LLM so I could focus on working on core funcionality. I also used Claude during implementing validation, to check if I missed some case that data would be invalid, but could pass validation. Other than that I asked some general questions about proper structurization of a project (both Api and Web part) and some weird bugs I encountered and couldn't figure out on my own.

I'm aware of lack of proper testing suite for the backend and will want to work on that next. I'm aware of problems it generates at least, so there's that.

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

| Concern          | Choice                               |
| ---------------- | ------------------------------------ |
| Runtime          | .NET 10 / ASP.NET Core               |
| API style        | Minimal APIs                         |
| ORM              | Entity Framework Core 10             |
| Database         | PostgreSQL 18                        |
| Auth             | JWT Bearer (HMAC-SHA256)             |
| Password hashing | BCrypt.Net                           |
| Rate limiting    | ASP.NET Core built-in (fixed window) |

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

**Functional Angular interceptor** - The frontend attaches the JWT from `localStorage` to every outgoing request via a standalone `authInterceptor` function registered in `app.config.ts`. Route guards protect authenticated pages.

**Rate limiting on auth endpoints** - A stricter fixed-window limiter (5 req/min) is applied to `/auth/*` to mitigate brute-force attempts, while general API traffic allows 100 req/min.

**Data validation** - I tried to protect backend from errors as much as I could how to, so I used seperate `BookValidator.cs` and `AuthValdiator.cs` classes t validate input data, before any operations with DB. I might have missed some, but I don't think I did. PS. Validating ISBN is quite complex, so I opted for _not so perfect_ Regex pattern, that checkes the general structure, but doesn' check the checksum. For the purposes of this app it's sufficient, but I'm aware it could be added.

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
│   ├── Unit/
│   │   ├── AuthValidatorTests.cs # AuthValidator tests
│   │   ├── BookValidatorTests.cs # BookValidator test
│   │   ├── JwtTests.cs           # JwtService tests
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
