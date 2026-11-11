git init
dotnet new gitignore
dotnet new editorconfig
dotnet new sln -n BaroBackend

dotnet new webapi -n BaroBackend.Api -o src/BaroBackend.Api
dotnet new classlib -n BaroBackend.Domain -o src/BaroBackend.Domain
dotnet new classlib -n BaroBackend.Infrastructure -o src/BaroBackend.Infrastructure
dotnet new xunit -n BaroBackend.Domain.Tests -o tests/BaroBackend.Domain.Tests

dotnet sln add src/BaroBackend.Api src/BaroBackend.Domain src/BaroBackend.Infrastructure tests/BaroBackend.Domain.Tests

dotnet add src/BaroBackend.Infrastructure reference src/BaroBackend.Domain
dotnet add src/BaroBackend.Api reference src/BaroBackend.Domain src/BaroBackend.Infrastructure
dotnet add tests/BaroBackend.Domain.Tests reference src/BaroBackend.Domain