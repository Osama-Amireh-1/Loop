# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
ENV ASPNETCORE_ENVIRONMENT=Production

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/Loop.Web.Api/Loop.Web.Api.csproj", "src/Loop.Web.Api/"]
COPY ["src/Loop.Infrastructure/Loop.Infrastructure.csproj", "src/Loop.Infrastructure/"]
COPY ["src/Loop.Application/Loop.Application.csproj", "src/Loop.Application/"]
COPY ["src/Loop.Domain/Loop.Domain.csproj", "src/Loop.Domain/"]
COPY ["src/Loop.SharedKernel/Loop.SharedKernel.csproj", "src/Loop.SharedKernel/"]
RUN dotnet restore "./src/Loop.Web.Api/Loop.Web.Api.csproj"
COPY . .
WORKDIR "/src/src/Loop.Web.Api"
RUN dotnet build "./Loop.Web.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Loop.Web.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "Loop.Web.Api.dll"]