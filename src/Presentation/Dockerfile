FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Presentation/StudentApi.Presentation.csproj", "src/Presentation/"]
COPY ["src/Application/StudentApi.Application.csproj", "src/Application/"]
COPY ["src/Infrastructure/StudentApi.Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Domain/StudentApi.Domain.csproj", "src/Domain/"]

RUN dotnet restore "src/Presentation/StudentApi.Presentation.csproj"

COPY . .
WORKDIR "/src/src/Presentation"
RUN dotnet publish "StudentApi.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "StudentApi.Presentation.dll"]
