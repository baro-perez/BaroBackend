# BaroBackend

Case study game backend as a self-training as Backend developer
Oriented to game backend, but interested in a possible transition to any industry, so eventually non-game features might appear

## Technologies used
This is a (maybe non-exhaustive) log of the technologies used in this project

### Integrated in the codebase
- ASP.NET Core
- Docker
- PostgreSQL
- Scalar
- Swagger UI
- Npgsql
- EF Core

### Used on the developer/client side:
- Docker Desktop
- DBeaver

## Logging the work and learnings done

### Initial setup
- Installed and initialized an ASP.NET server
- GET/ping endpoint as demo (returning utc and local madrid time using TimeService)
- GET/version endpoint as demo
- Integrated Scalar as testing UI in development
- Integrated Swagger as testing UI in development
- Explored using configuration to switch between which UI is used

### Entities schema
- Created the initial DB schema with Player, ResourceBalance and LedgerEntry
- Tested the migration flow