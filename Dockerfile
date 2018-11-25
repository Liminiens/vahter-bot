FROM microsoft/dotnet:2.1-sdk AS build-dotnet
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.sln ./
COPY src/VahterBot/*.csproj ./src/VahterBot/
RUN dotnet restore

# Copy everything else and build
COPY src/. ./src/
WORKDIR /app/src/VahterBot
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/dotnet:2.1-runtime
WORKDIR /app
COPY --from=build-dotnet /app/src/VahterBot/out .
ENTRYPOINT ["dotnet", "VahterBot.dll"]