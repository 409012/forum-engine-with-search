FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

COPY . ./
RUN dotnet restore src/FEwS.Forums.API
RUN dotnet publish -c Release -o out src/FEwS.Forums.API

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

CMD ["dotnet", "src/FEwS.Forums.API.dll"]