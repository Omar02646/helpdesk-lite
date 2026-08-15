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

## Internal User Provisioning

HelpDesk Lite intentionally has no public registration, Admin role, or account-management page. Every person signs in through an ASP.NET Core Identity **User** account, and engineering/IT intentionally assigns that account exactly one application role:

- `Employee`
- `SupportAgent`
- `Manager`

Run the separate provisioning tool from a trusted terminal. It uses `UserManager<ApplicationUser>` and `RoleManager<IdentityRole>`, applies the same password policy as the API, confirms internal users' email addresses, rejects duplicate email addresses, and never prints passwords or connection strings.

For the local Development database, the tool reads the API's checked-in local connection setting when run from the repository root:

```powershell
dotnet run --project backend\HelpDeskLite.UserProvisioning\HelpDeskLite.UserProvisioning.csproj
```

The interactive flow requests display name, email, role, password, and confirmation. Password input is hidden in an interactive console. Name, email, and role can optionally be supplied without putting the password in shell history:

```powershell
dotnet run --project backend\HelpDeskLite.UserProvisioning\HelpDeskLite.UserProvisioning.csproj -- --name "New Employee" --email "employee@example.com" --role Employee
```

For intentional automation only, the password can come from the process-scoped `HELPDESKLITE_PROVISIONING_PASSWORD` environment variable. Never use a plaintext password command-line argument.

To provision against MonsterASP from a trusted local PC:

```powershell
$env:ConnectionStrings__DefaultConnection="REMOTE_MONSTERASP_CONNECTION_STRING"
dotnet run --project backend\HelpDeskLite.UserProvisioning\HelpDeskLite.UserProvisioning.csproj
Remove-Item Env:ConnectionStrings__DefaultConnection
Remove-Item Env:HELPDESKLITE_PROVISIONING_PASSWORD -ErrorAction SilentlyContinue
```

Apply migrations before provisioning. Never share or commit user passwords, database credentials, or provisioning environment variables. The tool is run explicitly and is not included in API startup or the hosted website's public interface.

## MonsterASP.NET deployment

HelpDesk Lite is prepared for one ASP.NET Core 9 website on MonsterASP.NET. In Production, ASP.NET Core serves both `/api/...` and the compiled React SPA. Browser refreshes on React routes fall back to `wwwroot/index.html`; unmatched `/api/...` routes remain JSON 404 responses and are never handled by the SPA.

1. In the MonsterASP Control Panel, create an ASP.NET Core 9 website and note its deployment target, but do not upload credentials into this repository.
2. Create an MSSQL database and obtain the server, database, SQL username, and password from the control panel.
3. Supply the production connection string as the `ConnectionStrings__DefaultConnection` application setting/environment variable. A typical shape is `Server=HOST;Database=DATABASE;User Id=USER;Password=PASSWORD;Encrypt=True;TrustServerCertificate=True`; use the exact values and options MonsterASP provides.
4. Configure `ASPNETCORE_ENVIRONMENT=Production`. Configure `AttachmentStorage__RootPath=App_Data/attachments`, `AttachmentStorage__MaxFileSizeBytes=5242880`, and `AttachmentStorage__MaxFilesPerTicket=3`. Ensure the application identity has modify permission for the chosen persistent website folder. Do not place that directory under `wwwroot`.
5. Apply migrations from a trusted local terminal using the remote connection string:

   ```powershell
   $env:ConnectionStrings__DefaultConnection="MONSTERASP_CONNECTION_STRING"
   dotnet tool restore
   dotnet tool run dotnet-ef database update --project backend\HelpDeskLite.Api\HelpDeskLite.Api.csproj --startup-project backend\HelpDeskLite.Api\HelpDeskLite.Api.csproj --configuration Release
   ```

6. Publish from the repository root. The publish target runs `npm ci`, runs the Vite production build, and includes `dist` as `wwwroot` automatically:

   ```powershell
   dotnet publish backend\HelpDeskLite.Api\HelpDeskLite.Api.csproj -c Release
   ```

7. Upload the contents of `backend\HelpDeskLite.Api\bin\Release\net9.0\publish` using the MonsterASP WebDeploy profile or ZIP/FTP workflow. Upload the folder contents—not an extra parent directory. The Web SDK generates the ASP.NET Core Module `web.config`.
8. In the control panel, confirm ASP.NET Core 9, the Production environment, connection-string setting, and write permissions, then restart the website. Verify `/health`, `/login`, a direct refresh on `/tickets`, authentication, and a deliberately unknown `/api/...` route.
9. Create a test ticket with an image, reload its details, and confirm the authorized image remains available. Confirm the file exists in the configured persistent attachment directory but is not directly web-accessible.
10. Keep database, FTP, WebDeploy, and demo-account credentials out of Git, screenshots, logs, and support messages. Rotate any credential that is accidentally disclosed.

### Intentional demo-account provisioning

`DevelopmentSeeder` remains strictly Development-only and never executes on the hosted Production site. Provision portfolio/demo accounts explicitly with the Internal User Provisioning tool from a trusted local machine using the remote MonsterASP connection setting. Keep the hosted website in Production and never configure `SeedUsers__Password` there.

### Production hosting notes

- The frontend calls relative `/api/...` paths, so browser requests stay on the MonsterASP website origin. The Vite `/api` proxy remains Development-only.
- `App_Data/attachments` resolves relative to the deployed application content root and is served only through authorized controller actions.
- HTTPS redirection, HSTS, and Secure cookies are enabled in Production. ASP.NET Core Module/IIS integration supplies the original request scheme to the application.
- `/health` returns only `{ "status": "Healthy" }`; it intentionally performs no database query and exposes no diagnostics.
- Set `BuildFrontendOnPublish=false` only for specialized CI workflows that have already supplied `wwwroot`; the normal publish command should use the default integrated build.

## Current MVP assumptions and limitations

- New tickets receive `Medium` priority; final priority rules require confirmation.
- Categories are validated prototype strings: IT Support, Network, Email, Access & Accounts, and Other.
- Image limits and retention rules require stakeholder confirmation.
- Personal notification preferences and Configuration Preview remain illustrative frontend concepts.
- No Knowledge Base, SLA engine, advanced analytics, notifications, external integrations, real-time updates, or administrative provisioning UI.
