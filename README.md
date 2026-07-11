# Enhanzer Assignment

Responsive two-page coding assignment built with Angular and ASP.NET Core. The browser authenticates through the local API, the API calls the Enhanzer staging endpoint, stores returned locations in SQL Server, and protects the Purchase Bill workflow with a secure HttpOnly cookie.

## Features

- Login page with validation, password visibility toggle, spinner, and friendly errors.
- External login proxy using `API_Action = GetLoginData`, `Device_Id = D001`, email as `Company_Code` and `Username`, and entered password as `Pw`.
- Cookie-based local authenticated session.
- Protected Purchase Bill page with user header and logout.
- Batch dropdown populated from `Location_Details`.
- Fruit autocomplete for Mango, Apple, Banana, Orange, Grapes, Kiwi, and Strawberry.
- Decimal purchase calculations and backend recalculation before persistence.
- SQL Server schema, EF Core mappings, tests, and setup documentation.

## Technology Stack

- Frontend: Angular 21, TypeScript strict mode, standalone components, Reactive Forms, Angular Material.
- Backend: .NET 8, ASP.NET Core Web API, EF Core, SQL Server, cookie authentication.
- Tests: Vitest/Angular unit tests and xUnit backend tests.

## Prerequisites

- Node.js compatible with Angular 21. This environment had Node `22.20.0`, which is below Angular 22's minimum, so Angular 21 is used.
- npm 10+
- .NET 8 SDK
- SQL Server or SQL Server Express/LocalDB

## Configuration

Backend configuration can be provided in `appsettings.Development.json`, environment variables, or user secrets.

```powershell
$env:ConnectionStrings__DefaultConnection="Server=localhost;Database=EnhanzerAssignment;Trusted_Connection=True;TrustServerCertificate=True"
$env:ExternalApi__BaseUrl="https://ez-staging-api.azurewebsites.net/api/External_Api/POS_Api/Invoke"
$env:Authentication__ExpirationMinutes="60"
```

No passwords or sample credentials are stored in the repository.

## Database Setup

Run the SQL script in SQL Server Management Studio or `sqlcmd`:

```powershell
sqlcmd -S localhost -d EnhanzerAssignment -i database/schema.sql
```

EF Core migration commands:

```powershell
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project backend/src/Enhanzer.Assignment.Infrastructure --startup-project backend/src/Enhanzer.Assignment.Api
dotnet ef database update --project backend/src/Enhanzer.Assignment.Infrastructure --startup-project backend/src/Enhanzer.Assignment.Api
```

## Run the Application

Backend:

```powershell
dotnet restore backend/Enhanzer.Assignment.sln
dotnet run --project backend/src/Enhanzer.Assignment.Api
```

Frontend:

```powershell
cd frontend
npm install
npm start
```

Open `http://localhost:4200`.

## Tests and Builds

```powershell
dotnet build backend/Enhanzer.Assignment.sln
dotnet test backend/Enhanzer.Assignment.sln
cd frontend
npm test -- --watch=false
npm run build
```

## API Endpoints

- `POST /api/auth/login`
- `POST /api/auth/logout`
- `GET /api/auth/me`
- `GET /api/locations`
- `POST /api/purchase-bills`
- `GET /health`

## Calculation Formulas

- Gross Cost = Standard Cost x Quantity
- Discount Amount = Gross Cost x Discount Percentage / 100
- Total Cost = Gross Cost - Discount Amount
- Total Selling = Standard Price x Quantity

Monetary values are rounded to two decimals using `MidpointRounding.AwayFromZero`.

## Assumptions

- The PDF screenshots describe a minimal centered login UI and a blue-header purchase bill UI; the PDF file itself was blocked by sandbox permissions during implementation, so the attached text requirements drove the build.
- A successful external response is identified by finding a `User_Locations` collection in the returned JSON, including nested or string-encoded JSON bodies.
- A dummy invalid-login probe returned HTTP 200 with `Status_Code: 401`, message `Un-Authorize POS API Request.`, and `Response_Body: null`; the backend maps that wrapped response to local HTTP 401.
- Empty `User_Locations` is treated as an authenticated but location-less user, because the requirements say empty collections must be handled safely.

## Troubleshooting

- If cookies do not persist locally, confirm Angular calls `https://localhost:7055` and the API CORS origin includes `http://localhost:4200`.
- If login returns 502, inspect API logs for external service timeout, invalid JSON, or an unexpected response shape.
- If Angular 22 is installed accidentally, upgrade Node to the required version or keep Angular 21 as this project does.

## Security Considerations

- Passwords are not logged or stored.
- Angular never calls the external API directly.
- The session cookie is HttpOnly and secure outside Development.
- API errors return sanitized ProblemDetails responses.

## Screenshots

Add final screenshots after running locally:

- Login page
- Purchase Bill desktop
- Purchase Bill mobile

## Submission Checklist

- Angular build passes.
- .NET build and tests pass on a machine with the .NET 8 SDK.
- SQL script is included.
- README, architecture notes, and demo script are included.
- No secrets or sample credentials are committed.
