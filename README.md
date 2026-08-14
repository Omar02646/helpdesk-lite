# HelpDesk Lite

HelpDesk Lite is an internal support-ticket MVP with a React/Vite frontend and an ASP.NET Core 9 Web API backed by SQL Server and ASP.NET Core Identity.

## Architecture

- `src/` — approved Azure Horizon React + TypeScript interface
- `src/api/` — cookie-authenticated API clients
- `backend/HelpDeskLite.Api/` — controllers, Identity, EF Core domain and SQL Server persistence
- `backend/HelpDeskLite.Api.Tests/` — HTTP integration tests using an isolated in-memory test database
- `backend/HelpDeskLite.Api/Data/Migrations/` — checked-in SQL Server migration history

Authentication uses an HTTP-only Identity application cookie. The browser never stores an authentication token in localStorage. The Vite development server proxies `/api` to `http://localhost:5098`.

## Prerequisites

- Node.js and npm
- .NET SDK 9
- SQL Server accessible with the configured connection string

The default development connection is:

```text
Server=localhost;Database=HelpDeskLite;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

Override it without editing source:

```powershell
$env:ConnectionStrings__DefaultConnection="Server=YOUR_SERVER;Database=HelpDeskLite;Trusted_Connection=True;TrustServerCertificate=True"
```

## First-time setup

Restore frontend and backend dependencies:

```powershell
npm install
dotnet tool restore
dotnet restore backend\HelpDeskLite.sln
```

Configure the development seed password. It is intentionally not committed:

```powershell
$env:SeedUsers__Password="choose-a-strong-development-password"
```

The password must be at least 10 characters. In Development, restarting the API with a different configured seed password updates the seeded accounts to that password.

Apply migrations:

```powershell
dotnet tool run dotnet-ef database update --project backend\HelpDeskLite.Api\HelpDeskLite.Api.csproj --startup-project backend\HelpDeskLite.Api\HelpDeskLite.Api.csproj
```

The application uses migrations and does not use `EnsureCreated` for the SQL Server database.

## Run locally

Terminal 1, with `SeedUsers__Password` configured:

```powershell
dotnet run --project backend\HelpDeskLite.Api\HelpDeskLite.Api.csproj
```

Terminal 2:

```powershell
npm run dev
```

- Frontend: http://localhost:5173
- Backend: http://localhost:5098

## Development accounts

All accounts use the value supplied through `SeedUsers__Password`:

| Name | Email | Role |
| --- | --- | --- |
| Omar Mohamed | `omar@helpdesklite.local` | Employee |
| Ahmed Hassan | `ahmed@helpdesklite.local` | SupportAgent |
| Sara Ali | `sara@helpdesklite.local` | SupportAgent |
| Mona Adel | `mona@helpdesklite.local` | SupportAgent |
| Manager User | `manager@helpdesklite.local` | Manager |

There is no public registration endpoint.

## API

- `POST /api/auth/login`, `POST /api/auth/logout`, `GET /api/auth/me`
- `GET /api/tickets/my`, `POST /api/tickets`
- `GET /api/tickets`, `GET /api/tickets/{id}`
- `PATCH /api/tickets/{id}/assignee`, `PATCH /api/tickets/{id}/status`
- `GET /api/tickets/{id}/comments`, `POST /api/tickets/{id}/comments`
- `GET /api/support/queue`
- `GET /api/users/support-agents`
- `GET /api/manager/summary`
- `POST /api/tickets/{ticketId}/attachments`
- `GET /api/tickets/{ticketId}/attachments`
- `GET /api/tickets/{ticketId}/attachments/{attachmentId}`

Employee ticket access is ownership-scoped. Support mutation endpoints require `SupportAgent`. Manager endpoints and ticket details are read-only. Unauthenticated calls return 401 and authenticated role violations return 403.

## Image attachments

Attachment metadata is stored in SQL Server. Image files are stored outside static and executable content at the development default `backend/HelpDeskLite.Api/App_Data/attachments`.

Override attachment storage through configuration:

```powershell
$env:AttachmentStorage__RootPath="D:\HelpDeskLite\attachments"
$env:AttachmentStorage__MaxFileSizeBytes="5242880"
$env:AttachmentStorage__MaxFilesPerTicket="3"
```

Temporary MVP assumptions are PNG, JPG/JPEG, and WebP only, a maximum of 5 MB per image, and three images per ticket. Final formats, limits, retention, and storage policy require stakeholder confirmation. Physical filenames are generated GUID values; original names are retained only as metadata.

## Verification

```powershell
dotnet build backend\HelpDeskLite.sln
dotnet test backend\HelpDeskLite.sln
npm run lint
npm run build
```

## Product assumptions

- New tickets receive `Medium` priority as a development-only default. Final priority rules require product confirmation.
- Categories remain validated strings (`IT Support`, `Network`, `Email`, `Access & Accounts`, `Other`) and can later migrate to a category entity.
- Attachment rules are temporary MVP assumptions: image-only, 5 MB each, and three files per ticket.
- Personal notification preferences and Configuration Preview remain illustrative frontend concepts.
