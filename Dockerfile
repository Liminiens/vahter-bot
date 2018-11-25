FROM microsoft/dotnet:2.1-sdk AS build-dotnet
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/JobsBot/*.csproj ./src/JobsBot/
RUN dotnet restore

# Copy everything else and build
COPY src/. ./src/
WORKDIR /app/src/JobsBot
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine
WORKDIR /app
COPY --from=build-dotnet /app/out .
ENTRYPOINT ["dotnet", "JobsBot.dll"]