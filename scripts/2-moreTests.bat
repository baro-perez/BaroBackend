dotnet new xunit -n BaroBackend.Api.Tests -o tests/BaroBackend.Api.Tests
dotnet new xunit -n BaroBackend.Infrastructure.Tests -o tests/BaroBackend.Infrastructure.Tests
dotnet sln add tests/BaroBackend.Api.Tests tests/BaroBackend.Infrastructure.Tests
dotnet add tests/BaroBackend.Api.Tests reference src/BaroBackend.Api
dotnet add tests/BaroBackend.Api.Tests package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/BaroBackend.Api.Tests package Testcontainers.PostgreSql
dotnet add tests/BaroBackend.Infrastructure.Tests reference src/BaroBackend.Infrastructure
dotnet add tests/BaroBackend.Infrastructure.Tests package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/BaroBackend.Infrastructure.Tests package Testcontainers.PostgreSql