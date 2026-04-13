FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./src ./src
RUN dotnet restore ./src/InventoryHold.WebApi/InventoryHold.WebApi.csproj

# Copy everything else and build
COPY . .
WORKDIR /app/src/InventoryHold.WebApi
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/src/InventoryHold.WebApi/out .
ENTRYPOINT ["dotnet", "InventoryHold.WebApi.dll"]
