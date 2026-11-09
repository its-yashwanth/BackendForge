
# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["MyFirstApi.csproj", "./"]

RUN dotnet restore "MyFirstApi.csproj"

COPY . .

RUN dotnet publish "MyFirstApi.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Stage 2: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "MyFirstApi.dll"]
