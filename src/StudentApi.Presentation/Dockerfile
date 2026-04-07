FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/StudentApi.Presentation/StudentApi.Presentation.csproj", "src/StudentApi.Presentation/"]
COPY ["src/StudentApi.Application/StudentApi.Application.csproj", "src/StudentApi.Application/"]
COPY ["src/StudentApi.Infrastructure/StudentApi.Infrastructure.csproj", "src/StudentApi.Infrastructure/"]
COPY ["src/StudentApi.Domain/StudentApi.Domain.csproj", "src/StudentApi.Domain/"]

RUN dotnet restore "src/StudentApi.Presentation/StudentApi.Presentation.csproj"

COPY . .
WORKDIR "/src/src/StudentApi.Presentation"
RUN dotnet publish "StudentApi.Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "StudentApi.Presentation.dll"]
