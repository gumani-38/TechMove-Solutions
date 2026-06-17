# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Install Node.js
RUN apt-get update && apt-get install -y curl gnupg \
    && curl -fsSL https://deb.nodesource.com/setup_22.x | bash - \
    && apt-get install -y nodejs

# Copy csproj and restore
COPY ["Gumani_Moila_EAPD7111w_POE.csproj", "."]
RUN dotnet restore "Gumani_Moila_EAPD7111w_POE.csproj"

# Copy package files first
COPY package*.json ./

# Install npm packages
RUN npm ci

# Copy remaining files
COPY . .

RUN npx tailwindcss -i ./wwwroot/css/site.css -o ./wwwroot/css/output.css --minify

RUN dotnet build "Gumani_Moila_EAPD7111w_POE.csproj" -c Release -o /app/build --verbosity detailed


# Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Gumani_Moila_EAPD7111w_POE.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# Runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Gumani_Moila_EAPD7111w_POE.dll"]