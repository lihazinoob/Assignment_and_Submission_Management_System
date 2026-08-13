FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY AssignmentSystem.sln ./
COPY src/LMS_Assignment.Api/LMS_Assignment.Api.csproj src/LMS_Assignment.Api/
COPY src/LMS_Assignment.Application/LMS_Assignment.Application.csproj src/LMS_Assignment.Application/
COPY src/LMS_Assignment.Infrastructure/LMS_Assignment.Infrastructure.csproj src/LMS_Assignment.Infrastructure/
COPY src/LMS_Assignment.Domain/LMS_Assignment.Domain.csproj src/LMS_Assignment.Domain/
RUN dotnet restore src/LMS_Assignment.Api/LMS_Assignment.Api.csproj

COPY src/ src/
RUN dotnet publish src/LMS_Assignment.Api/LMS_Assignment.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet LMS_Assignment.Api.dll"]
