using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


class Program
{
    static async Task Main(string[] args)
    {
        string sentinelHostname = Environment.GetEnvironmentVariable("REDIS_SENTINEL_HOSTNAME");
        string password = Environment.GetEnvironmentVariable("REDIS_PASSWORD");

        var sentinelConfig = new ConfigurationOptions
        {
            ServiceName = "mymaster", // Replace with your Redis master name
            Password = password,
            AbortOnConnectFail = false, // Add this line to set abortConnect to false
        };
        // Add the Sentinel endpoint with a specific port (26379 is the default Sentinel port)
        sentinelConfig.EndPoints.Add(sentinelHostname);

        var sentinelConnection = ConnectionMultiplexer.Connect(sentinelConfig);

        var redis = sentinelConnection.GetDatabase();

        string totalOperationsEnv = Environment.GetEnvironmentVariable("TOTAL_OPERATIONS");
        if (!int.TryParse(totalOperationsEnv, out int totalOperations))
        {
            Console.WriteLine("Invalid value for TOTAL_OPERATIONS environment variable.");
            return;
        }

        Console.WriteLine($"Benchmarking Redis performance for {totalOperations} operations...");

        // Measure the time taken to set values
        var setWatch = new Stopwatch();
        setWatch.Start();

        Parallel.For(0, totalOperations, i =>
        {
            string key = GenerateRandomKey(230);
            string value = GenerateRandomString(400 * 1024); // 400KB string

            // Asynchronously set the value
            redis.StringSet(key, value, TimeSpan.FromSeconds(5));

            // Asynchronously get the value
            string storedValue = redis.StringGet(key,CommandFlags.PreferReplica);
            // Log intermediate statistics
            if (i % 100 == 0)
              {
                Console.WriteLine($"Completed {i} operations.");
              }
        });

        setWatch.Stop();
        double setElapsedSeconds = setWatch.Elapsed.TotalSeconds;

        Console.WriteLine($"Set and Get operations completed in {setElapsedSeconds} seconds.");
        Console.WriteLine($"Average time for a set operation: {setElapsedSeconds / totalOperations} seconds");

        sentinelConnection.Close();
    }

    static string GenerateRandomKey(int length)
    {
        if (length < 1)
        {
            throw new ArgumentException("Key length must be at least 1 character.");
        }

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        char[] key = new char[length];
        Random random = new Random();

        for (int i = 0; i < length; i++)
        {
            key[i] = chars[random.Next(chars.Length)];
        }

        return new string(key);
    }

    static string GenerateRandomString(int size)
    {
        Random random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, size)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}

