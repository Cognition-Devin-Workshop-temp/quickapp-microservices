FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5006

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Services/Analytics/Analytics.API/Analytics.API.csproj", "Services/Analytics/Analytics.API/"]
COPY ["Services/Analytics/Analytics.Domain/Analytics.Domain.csproj", "Services/Analytics/Analytics.Domain/"]
COPY ["Services/Analytics/Analytics.Infrastructure/Analytics.Infrastructure.csproj", "Services/Analytics/Analytics.Infrastructure/"]
COPY ["Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]
COPY ["Shared/Shared.Infrastructure/Shared.Infrastructure.csproj", "Shared/Shared.Infrastructure/"]
RUN dotnet restore "Services/Analytics/Analytics.API/Analytics.API.csproj"
COPY . .
WORKDIR "/src/Services/Analytics/Analytics.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5006
ENTRYPOINT ["dotnet", "Analytics.API.dll"]
