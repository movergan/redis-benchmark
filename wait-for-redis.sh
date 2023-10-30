#!/bin/bash

# Define the Redis host and port
REDIS_HOST="redis-sentinel"
REDIS_PORT=26379

# Define the maximum number of attempts to ping Redis
MAX_ATTEMPTS=30

# Sleep time between each attempt (adjust as needed)
SLEEP_SECONDS=1

# Function to ping Redis and check if it's available
check_redis() {
 echo ping | telnet  $REDIS_HOST $REDIS_PORT  | grep -q "PONG"
}

# Loop until Redis is available or maximum attempts are reached
attempts=0
while ! check_redis && [ $attempts -lt $MAX_ATTEMPTS ]; do
  attempts=$((attempts + 1))
  echo "Waiting for Redis to become available... Attempt $attempts"
  sleep $SLEEP_SECONDS
done

if check_redis; then
  echo "Redis is available. Starting your application."
  exec "$@"
else
  echo "Could not connect to Redis. Exiting."
  exit 1
fi
