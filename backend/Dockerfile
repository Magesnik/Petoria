FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY backend/Petoria.sln ./
COPY backend/Petoria/Petoria.csproj Petoria/
COPY backend/Petoria.Core/Petoria.Core.csproj Petoria.Core/
COPY backend/Petoria.Infrastructure/Petoria.Infrastructure.csproj Petoria.Infrastructure/
COPY backend/Petoria.Constants/Petoria.Constants.csproj Petoria.Constants/

# Restore dependencies
RUN dotnet restore Petoria/Petoria.csproj

# Copy all source files
COPY backend/ .

# Publish
RUN dotnet publish Petoria/Petoria.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Petoria.dll"]
