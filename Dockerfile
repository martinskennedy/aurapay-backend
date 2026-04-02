FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY AuraPay.sln ./
COPY src/AuraPay.WebAPI/AuraPay.WebAPI.csproj src/AuraPay.WebAPI/
COPY src/AuraPay.Application/AuraPay.Application.csproj src/AuraPay.Application/
COPY src/AuraPay.Domain/AuraPay.Domain.csproj src/AuraPay.Domain/
COPY src/AuraPay.Infrastructure/AuraPay.Infrastructure.csproj src/AuraPay.Infrastructure/

RUN dotnet restore src/AuraPay.WebAPI/AuraPay.WebAPI.csproj

COPY . .
RUN dotnet publish src/AuraPay.WebAPI/AuraPay.WebAPI.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "AuraPay.WebAPI.dll"]