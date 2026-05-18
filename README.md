# Loop

Loop is a .NET 10 loyalty and rewards platform built with Clean Architecture. The solution is split into presentation, application, domain, infrastructure, and shared-kernel projects, with architecture and integration tests.

## What Loop provides

- User authentication and token refresh/logout flows
- User profile management, including profile image upload
- Points balance management and points redemption QR flows
- Shop admin login and shop administration workflows
- Stamp collection and stamp redemption flows
- Receipt OCR processing for loyalty and rewards scenarios
- Catalog browsing for categories, shops, and offers
- Swagger/OpenAPI documentation
- Health checks
- Structured logging with Serilog
- PostgreSQL persistence with Entity Framework Core

## Solution structure

- `Loop.Web.Api` - ASP.NET Core Web API presentation layer
- `Loop.Application` - application use cases, commands, queries, and abstractions
- `Loop.Domain` - domain entities and business rules
- `Loop.Infrastructure` - persistence, authentication, storage, and external integrations
- `Loop.SharedKernel` - shared primitives and common domain types
- `tests/Loop.ArchitectureTests` - architecture rules and project boundaries
- `tests/Loop.IntegrationTests` - integration tests

## Prerequisites

- .NET 10 SDK
- PostgreSQL
- Optional: Seq for log exploration

## Running locally

1. Restore and build the solution.
2. Configure your connection string and any required authentication settings.
3. Run the `Loop.Web.Api` project.
4. Open Swagger to explore the API endpoints.

## API highlights

### Authentication
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`
- `POST /api/auth/forgot-password`
- `POST /api/auth/reset-password`

### Users
- `POST /api/users/register`
- `GET /api/users/{id}`
- `GET /api/users/me`
- `GET /api/users/points/balance`
- `POST /api/users/points`
- `POST /api/users/points/redemption-qr`
- `PUT /api/users/me`
- `POST /api/users/profile-image`

### Shop admins
- `POST /api/shop-admins/login`
- `POST /api/shop-admins/refresh`
- `POST /api/shop-admins/points/redemption-qr/confirm`

### Categories, shops, offers
- `GET /api/categories?mallId={mallId}`
- `GET /api/shops?mallId={mallId}&categoryId={categoryId}&searchTerm={searchTerm}`
- `GET /api/shops/{shopId}`
- `GET /api/offers?mallId={mallId}`
- `GET /api/offers/{mallId}/{shopId}`

### Stamps
- `GET /api/stamps?mallId={mallId}&shopId={shopId}`
- `GET /api/stamps/completed`
- `GET /api/stamps/active`
- `POST /api/stamps/{stampId}/redemption-qr`
- `POST /api/stamps/redemption-qr/confirm`
- `POST /api/stamps/{stampId}/collection-qr`
- `POST /api/stamps/collection-qr/scan`

### User stamp cards
- `GET /api/users/stamp-cards`

### Receipts
- `POST /api/receipts/ocr`

## Notes

- The application uses role/policy-based authorization for user and shop-admin endpoints.
- Health checks are exposed at `GET /health`.
- Swagger is enabled in the API project.

## Testing

Run the architecture and integration test projects from Visual Studio or the command line after configuring the required dependencies.
