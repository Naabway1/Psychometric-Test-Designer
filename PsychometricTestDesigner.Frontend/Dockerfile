FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["PsychometricTestDesigner.Frontend.csproj", "./"]
RUN dotnet restore "PsychometricTestDesigner.Frontend.csproj"

COPY . ./
RUN dotnet publish "PsychometricTestDesigner.Frontend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ApiBaseUrl=http://app:80

EXPOSE 80
ENTRYPOINT ["dotnet", "PsychometricTestDesigner.Frontend.dll"]

