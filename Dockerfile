# The scraper as a container image, for the production Container Apps Job (infra/modules/scraper-job.bicep).
# Build from the repository root: docker build -t bonus-scraper .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/ src/
RUN dotnet publish src/BonusScraper.Cli -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app .
# The JSON and the recorded model answers are written next to the app; the image runs as the
# non-root user the .NET images ship with, so it owns only these two directories.
RUN mkdir data recordings && chown "$APP_UID" data recordings
USER $APP_UID
ENTRYPOINT ["dotnet", "bonus-scraper.dll"]
CMD ["scrape"]
