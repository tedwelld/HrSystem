FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["HrSystem.Api/HrSystem.Api.csproj", "HrSystem.Api/"]
COPY ["HrSystem.Core/HrSystem.Core.csproj", "HrSystem.Core/"]
COPY ["HrSystem.Data/HrSystem.Data.csproj", "HrSystem.Data/"]

RUN dotnet restore "HrSystem.Api/HrSystem.Api.csproj"

COPY . .
WORKDIR /src/HrSystem.Api
RUN dotnet publish "HrSystem.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "HrSystem.Api.dll"]
