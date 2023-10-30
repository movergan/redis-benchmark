# Use the official .NET SDK as a build image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copy the .csproj and restore as distinct layers
COPY RedisBenchmark/RedisBenchmark/RedisBenchmark.csproj ./
RUN dotnet restore

# Copy everything else and build the app
COPY RedisBenchmark/RedisBenchmark ./
RUN dotnet publish -c Release -o out

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Set environment variables for Redis Sentinel
ENV REDIS_SENTINEL_HOSTNAME your_sentinel_host
ENV REDIS_PASSWORD your_password
ENV TOTAL_OPERATIONS 100

# Add wait-for-it.sh script and make it executable
#COPY wait-for-redis.sh /usr/wait-for-redis.sh
#RUN chmod +x /usr/wait-for-redis.sh

# Expose the port your app is listening on (if applicable)
EXPOSE 80

# Specify the project to run
#CMD ["dotnet", "RedisBenchmark.dll"]
CMD dotnet RedisBenchmark.dll
