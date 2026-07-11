# Enhanzer Assignment

Two-page full stack assignment built with Angular 22 and ASP.NET Core Web API. The app authenticates through the Enhanzer staging API, stores returned user locations in SQL Server, and protects the Purchase Bill workflow after login.

## Features

- Login page with required field validation, password visibility toggle, loading state, and meaningful error messages.
- Backend proxy for the external API endpoint:
  `https://ez-staging-api.azurewebsites.net/api/External_Api/POS_Api/Invoke`.
- Login request sends `API_Action = GetLoginData`, `Device_Id = D001`, email as `Company_Code` and `Username`, and password as `Pw`.
- Successful login reads `Response_Body[0].User_Locations` and saves `Location_Code` / `Location_Name` into `Location_Details`.
- Cookie-based authenticated session with protected Purchase Bill routes and APIs.
- Refresh support on the Purchase Bill page using a non-secret session snapshot for user email and locations.
- Purchase Bill form with fruit autocomplete, batch dropdown, validations, line-item table, and item summary.
- Backend recalculates purchase totals before saving bills.

## Technology Stack

- Frontend: Angular 22, TypeScript 6, Angular Material, Reactive Forms, npm.
- Backend: .NET 8, ASP.NET Core Web API, C#, EF Core, SQL Server.
- Tests: Angular/Vitest tests and xUnit backend tests.

## Requirements Covered

- Basic login page based on the provided design.
- External API authentication through the ASP.NET Core backend.
- Input validation and friendly validation/error messages.
- Unauthorized access prevention for the Purchase Bill page.
- Secure HttpOnly authentication cookie.
- SQL Server persistence for `Location_Details`.
- Item autocomplete values: Mango, Apple, Banana, Orange, Grapes, Kiwi, Strawberry.
- Batch dropdown populated from saved `Location_Details`.
- Total Cost and Total Selling calculations.
- Add item table and summary totals.
- REST API integration, component-based Angular structure, reusable components, error handling, and loading indicators.

## Prerequisites

- Node.js 24.18.0 or another Angular 22 compatible Node version.
- npm 11+.
- .NET 8 SDK.
- SQL Server, SQL Server Express, LocalDB, or a hosted SQL Server database.

Check versions:

```powershell
node -v
npm -v
dotnet --version
```

## Configuration

The repository does not need real passwords committed in `appsettings.json`. Use environment variables or user secrets for real database credentials.

Local SQL Server example:

```powershell
$env:ConnectionStrings__DefaultConnection="Server=localhost;Database=EnhanzerAssignment;Trusted_Connection=True;TrustServerCertificate=True"
```

Hosted SQL Server example:

```powershell
$env:ConnectionStrings__DefaultConnection="Data Source=SQL8020.site4now.net;Initial Catalog=db_acbe07_methsarasilva;User Id=db_acbe07_methsarasilva_admin;Password=YOUR_DB_PASSWORD;Encrypt=True;TrustServerCertificate=True;"
```

External API settings can also be overridden:

```powershell
$env:ExternalApi__BaseUrl="https://ez-staging-api.azurewebsites.net/api/External_Api/POS_Api/Invoke"
$env:ExternalApi__DeviceId="D001"
$env:Authentication__ExpirationMinutes="60"
```

## Database Setup

The backend creates `Location_Details` before saving login locations. The full SQL script is also included at:

```text
database/schema.sql
```

Run it manually if you want to create all tables up front:

```powershell
sqlcmd -S localhost -d EnhanzerAssignment -i database/schema.sql
```

For hosted SQL Server:

```powershell
sqlcmd -S SQL8020.site4now.net -d db_acbe07_methsarasilva -U db_acbe07_methsarasilva_admin -P YOUR_DB_PASSWORD -i database/schema.sql
```

## Run The Backend

```powershell
dotnet restore backend/Enhanzer.Assignment.sln
dotnet run --project backend/src/Enhanzer.Assignment.Api --urls "https://localhost:7055;http://localhost:5055"
```

Swagger:

```text
https://localhost:7055/swagger
```

## Run The Frontend

```powershell
cd frontend
npm install
npm start
```

Open:

```text
http://localhost:4200
```

Use `localhost`, not `127.0.0.1`, so the CORS and cookie settings match the development configuration.

## Login Test Credentials

Use the credentials provided for the assignment/testing environment:

```text
Email: info@enhanzer.com
Password: Welcome#3
```

Do not store real passwords in source control.

## API Endpoints

- `POST /api/auth/login`
- `POST /api/auth/logout`
- `GET /api/auth/me`
- `GET /api/locations`
- `POST /api/purchase-bills`
- `GET /health`

## Purchase Calculations

- Gross Cost = Standard Cost x Quantity.
- Discount Amount = Gross Cost x Discount Percentage / 100.
- Total Cost = Gross Cost - Discount Amount.
- Total Selling = Standard Price x Quantity.
- Summary Total Items = number of rows.
- Summary Total Quantity = sum of row quantities.
- Summary Total Cost = sum of row total costs.
- Summary Total Selling = sum of row total selling values.

## Tests And Builds

Backend:

```powershell
dotnet test backend/Enhanzer.Assignment.sln
```

Frontend:

```powershell
cd frontend
npm run build
npm test -- --watch=false
```

Verified locally with:

- Node.js `v24.18.0`
- npm `11.16.0`
- Angular packages `22.0.x`
- .NET `8`

## Project Structure

```text
backend/
  src/
    Enhanzer.Assignment.Api
    Enhanzer.Assignment.Application
    Enhanzer.Assignment.Domain
    Enhanzer.Assignment.Infrastructure
  tests/
frontend/
  src/app/core
  src/app/features/login
  src/app/features/purchase-bill
  src/app/shared
database/
  schema.sql
```

## Security Notes

- Passwords are never stored in the frontend or database.
- The frontend calls only the local backend, not the external login API directly.
- The backend session uses an HttpOnly cookie.
- A small non-secret `sessionStorage` snapshot is used only to keep the Purchase Bill page stable after refresh.
- API errors are returned as sanitized ProblemDetails responses.

## Submission Checklist

- Push the final project to GitHub.
- Include `database/schema.sql`.
- Keep real passwords out of committed files.
- Confirm `npm run build` passes.
- Confirm `dotnet test backend/Enhanzer.Assignment.sln` passes.
- Record a 5-10 minute demo showing login, location save, batch dropdown, item add, summary update, page refresh, and purchase bill save.
- Complete the required submission form.
