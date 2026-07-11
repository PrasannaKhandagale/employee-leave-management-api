# Employee Leave Management API

A portfolio-ready API-testing training application built with ASP.NET Core 8, EF Core SQLite, JWT, Swagger, Docker, and Postman.

## Run locally

Requirements: .NET 8 SDK.

```bash
dotnet restore
dotnet run
```

Open `http://localhost:5091/swagger`.

## Seeded accounts

| Role | Email | Password |
|---|---|---|
| Employee | employee@test.com | Password@123 |
| Manager | manager@test.com | Password@123 |
| Admin | admin@test.com | Password@123 |

## Typical testing flow

1. Login as employee and save token.
2. Read employee 101 balance.
3. Submit future leave.
4. Login as manager and approve it.
5. Login as employee and verify balance reduction.
6. Cancel before the start date and verify balance restoration.

## Docker

```bash
docker build -t leave-management-api .
docker run -p 8080:8080 -e Jwt__Key="replace-with-a-long-secret-at-least-32-characters" leave-management-api
```

Open `http://localhost:8080/swagger`.

## Production note

SQLite inside an ephemeral container can be reset during redeployment. That is useful for a training API. For durable shared testing, replace SQLite with PostgreSQL or attach persistent storage.
