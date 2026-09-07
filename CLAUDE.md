# Ratbags.Articles.API

## What this is
ASP.NET Core Web API microservice for articles, part of the "Ratbags" personal microservices project. Sits behind an Ocelot API gateway — not called directly by a frontend. Talks to sibling services (comment counts, usernames) over Azure Service Bus request/response messaging rather than direct HTTP. It does **not** fetch an article's actual comments — see Gotchas.

## Stack
- .NET 10 (`net10.0`), ASP.NET Core Web API, C#, nullable + implicit usings enabled
- EF Core 8.0.10 + SQL Server, migrations in `Migrations/`
- Azure Service Bus (`Azure.Messaging.ServiceBus`) via the shared `Ratbags.Core` package, for request/response messaging with other services
- JWT bearer auth (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- Swagger/Swashbuckle with annotations
- Tests: NUnit + Moq + EF Core InMemory, in `Ratbags.Articles.API.Tests`

## Architecture
Layered: `Controllers` → `Services` → `Repositories` → `ApplicationDbContext` (EF Core).
- Every layer has an interface in `Interfaces/`, registered in `ServiceExtensions/DIServiceExtension.cs`.
- Request/response contracts live under `Models/API` (create/update commands), `Models/DTOs` (outbound DTOs), `Models/DB` (EF entities).
- Cross-service calls go through `Messaging/ArticlesServiceBusService.cs` (`IArticlesServiceBusService`) — not HTTP.
- Controllers catch exceptions and return 500 with a generic message; the real error is logged via `ILogger`.

## Commands
- Build: `dotnet build Ratbags.Articles.API.sln`
- Run API: `dotnet run --project Ratbags.Articles.API`
- Run tests: `dotnet test`
- Add EF migration: `dotnet ef migrations add <Name> --project Ratbags.Articles.API`
- Apply migrations: `dotnet ef database update --project Ratbags.Articles.API`

## NuGet
The private `Ratbags.Core` package comes from a GitHub Packages feed (`nuget.config`), authenticated via a `GITHUB_TOKEN` env var. Set `GITHUB_TOKEN` locally before `dotnet restore`, or restore fails with a missing-package error rather than an obvious auth error.

## Conventions
- Async everywhere with an `*Async` suffix; repositories/services return nullable results (e.g. `Article?`) for "not found" rather than throwing.
- DTOs are built field-by-field in the service layer — no AutoMapper.
- Test files are one per layer (`ControllerTests.cs`, `ServiceTests.cs`, `RepositoryTests.cs`), not one per class — add new tests into the matching file rather than creating new ones.

## Gotchas
- `ArticleDTO` returned from `GetByIdAsync` has no `Comments` field — that was removed along with the ASB call to Comments.API's comments-for-article flow (dead on both sides: `GetCommentsForArticleWorker`/`Handler` in Comments.API are gone too, and `comments-list-topic` is out of both services' config and the emulator's `Config.json`). If an article page ever needs to show comments, that's expected to be a separate call the frontend makes directly to Comments.API — not something Articles.API composes into its own response. `GetArticlesCommentsCount` (comment *counts* for the article list) and `GetUserNameDetails` (author lookup) are unaffected and still go over ASB as before.
- `ServiceTests.cs` is currently entirely commented out and references a stale `IMassTransitService` abstraction that no longer exists (renamed to `IArticlesServiceBusService`). Treat it as disabled, not a template to copy as-is.
- `appsettings.json`'s JWT secret is now a placeholder, but the certificate password is still a real value checked in. `UserSecretsId` is already configured for Development — put any new real secrets there instead of adding more to `appsettings.json`.
- Ports are inconsistent across files: `appsettings.json` uses 5010/5011, the `Dockerfile` exposes 5078/7159. Check which actually applies rather than assuming.
- CORS only allows `localhost:5000`/`5001` (the Ocelot gateway). Don't add a frontend origin directly here — that should go through the gateway config.
- Package versions (EF Core, JWT, etc.) are pinned to 8.0.10 even though `TargetFramework` is `net10.0` — looks like a leftover from a framework bump, not necessarily something to "fix" unprompted.

## Don't
- Don't call other Ratbags services over HTTP — use the Service Bus request/response pattern in `Messaging/`.
- Don't commit real secrets/connection strings beyond what's already there — use user secrets or env vars for anything new.
