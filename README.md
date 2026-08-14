# HelpDesk Lite

HelpDesk Lite is a production-oriented internal support-ticket MVP implementing **Submit → Assign → Handle → Resolve → Track**. The application combines the approved responsive Azure Horizon interface with server-enforced authentication, authorization, persistence, and secure image delivery.

## Architecture

- `src/` — React 19, TypeScript, Vite, Tailwind CSS, and reusable UI components
- `src/api/` — credentialed API clients and DTO mapping
- `backend/HelpDeskLite.Api/` — ASP.NET Core 9 controllers, services, Identity, EF Core, and SQL Server persistence
- `backend/HelpDeskLite.Api.Tests/` — HTTP integration/security tests with an isolated database and temporary attachment storage
- `backend/HelpDeskLite.Api/Data/Migrations/` — checked-in SQL Server migration history

Authentication uses an HTTP-only ASP.NET Core Identity cookie. Tokens and attachments are never stored in localStorage. Vite proxies `/api` to `http://localhost:5098` during development.

## Roles and features

- **Employee:** sign in, submit tickets, view only their own tickets, track activity, and upload/view ticket images.
- **SupportAgent:** use the active queue and all-ticket view, search/filter, assign ownership, change status, add progress updates, and view images.
- **Manager:** view database-derived metrics, support workload, tickets, activity, and images in read-only mode.

There is no public registration or administration endpoint.

## Prerequisites and database setup

- Node.js and npm
- .NET SDK 9
- SQL Server

The checked-in connection string is a local Windows-authentication development default, not a production secret. Override it without editing source:

```powershell
$env:ConnectionStrings__DefaultConnection="Server=YOUR_SERVER;Database=HelpDeskLite;Trusted_Connection=True;TrustServerCertificate=True"
```

Restore dependencies, configure a strong development-only seed password, and apply migrations:

```powershell
npm install
dotnet tool restore
dotnet restore backend\HelpDeskLite.sln
$env:SeedUsers__Password="choose-a-strong-development-password"
dotnet tool run dotnet-ef database update --project backend\HelpDeskLite.Api\HelpDeskLite.Api.csproj --startup-project backend\HelpDeskLite.Api\HelpDeskLite.Api.csproj
```

The application uses EF Core migrations, not `EnsureCreated`, for SQL Server.

## Run locally

Terminal 1 (with `SeedUsers__Password` configured):

```powershell
dotnet run --project backend\HelpDeskLite.Api\HelpDeskLite.Api.csproj
```

Terminal 2:

```powershell
npm run dev
```

- Frontend: `http://localhost:5173`
- Backend: `http://localhost:5098`

## Development accounts

All accounts use the value supplied through `SeedUsers__Password`. Development seeding never runs in Production.

| Name | Email | Role |
| --- | --- | --- |
| Omar Mohamed | `omar@helpdesklite.local` | Employee |
| Ahmed Hassan | `ahmed@helpdesklite.local` | SupportAgent |
| Sara Ali | `sara@helpdesklite.local` | SupportAgent |
| Mona Adel | `mona@helpdesklite.local` | SupportAgent |
| Manager User | `manager@helpdesklite.local` | Manager |

## API and pagination

The API provides `/api/auth/login`, `/logout`, and `/me`; employee and support ticket lists/details; ticket creation, assignment, status, and comments; the support queue; support-agent lookup; manager summary; and authorized attachment upload/list/open endpoints.

`GET /api/tickets`, `GET /api/tickets/my`, and `GET /api/support/queue` accept `page` and `pageSize` (defaults 1 and 20, maximum 100), plus server-side search/status/category/owner filters. Responses include items, total/page metadata, and aggregate filtered counts.

## Secure image attachments

Metadata is stored in SQL Server. Files default to `backend/HelpDeskLite.Api/App_Data/attachments`, outside static/executable content, and use random GUID physical names. Override production storage with:

```powershell
$env:AttachmentStorage__RootPath="D:\HelpDeskLite\attachments"
$env:AttachmentStorage__MaxFileSizeBytes="5242880"
$env:AttachmentStorage__MaxFilesPerTicket="3"
```

Temporary MVP limits are PNG, JPG/JPEG, and WebP only, 5 MB per image, and three images per ticket. The API validates extension, declared MIME type, size, count, and image signature. Every metadata/download request is authorized; physical paths are never returned. Production still needs a durable storage, backup, retention, permissions, and malware-scanning policy.

## Security and production behavior

- HTTP-only, SameSite Strict Identity cookies; Secure is forced outside Development.
- Eight-hour sliding sessions and 15-minute lockout after five failed attempts.
- API-friendly 401/403 responses with no authentication redirects.
- Generic logged Problem Details for unexpected failures; no client stack traces.
- HSTS and HTTPS redirection outside Development.
- Secrets and development attachment storage are gitignored.

Supply production connection strings and storage configuration through environment variables or a secret/configuration provider. Configure the production web host to fall back non-API routes to `index.html` for direct SPA refreshes.

## Verification

```powershell
dotnet restore backend\HelpDeskLite.sln
dotnet build backend\HelpDeskLite.sln
dotnet test backend\HelpDeskLite.sln
npm run lint
npm run build
```

## Current MVP assumptions and limitations

- New tickets receive `Medium` priority; final priority rules require confirmation.
- Categories are validated prototype strings: IT Support, Network, Email, Access & Accounts, and Other.
- Image limits and retention rules require stakeholder confirmation.
- Personal notification preferences and Configuration Preview remain illustrative frontend concepts.
- No Knowledge Base, SLA engine, advanced analytics, notifications, external integrations, real-time updates, or administrative provisioning UI.
