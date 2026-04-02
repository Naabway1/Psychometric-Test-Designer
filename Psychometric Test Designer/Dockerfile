FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore first for cache efficiency
COPY ["Psychometric Test Designer.csproj", "./"]
RUN dotnet restore "Psychometric Test Designer.csproj"

# Copy everything and build
COPY . ./
RUN dotnet publish "Psychometric Test Designer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Development
ENV POSTGRES_DB=psychometric_db
ENV POSTGRES_USER=postgres
ENV POSTGRES_PASSWORD=12345

EXPOSE 80
ENTRYPOINT ["dotnet", "Psychometric Test Designer.dll"]
