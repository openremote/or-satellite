#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["or-satellite/or-satellite.csproj", "or-satellite/"]
RUN dotnet restore "or-satellite/or-satellite.csproj"
COPY . .
WORKDIR "/src/or-satellite"
RUN dotnet build "or-satellite.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "or-satellite.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "or-satellite.dll"]

RUN apt-get update
RUN apt-get install -y \
	nco

RUN mkdir Copernicus Copernicus/Downloads Copernicus/Extraction Copernicus/Processed

RUN curl https://github.com/Siyabent/LocSearch/releases/download/v0.1/LocSearchCore > /app/Copernicus/LocSearchCore