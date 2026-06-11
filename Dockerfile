FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build

COPY src/JobFinderNet.Core/JobFinderNet.Core.csproj src/JobFinderNet.Core/
COPY src/JobFinderNet.Infrastructure/JobFinderNet.Infrastructure.csproj src/JobFinderNet.Infrastructure/
COPY src/JobFinderNet.Api/JobFinderNet.Api.csproj src/JobFinderNet.Api/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore src/JobFinderNet.Api/JobFinderNet.Api.csproj

COPY src/ src/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish src/JobFinderNet.Api/JobFinderNet.Api.csproj -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /publish .
HEALTHCHECK --interval=15s --timeout=5s --retries=3 CMD curl -sf http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "JobFinderNet.Api.dll"]
