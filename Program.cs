using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

class Program
{
    private static readonly object fileLock = new object();
    private static int completedRequests = 0; // Counter for completed requests

    static async Task Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("██   ██ ████████  ██████        ██████   ██████      ██    ██ ██████  ██          ███████ ██    ██ ███████ ███████ ███████ ██████ ");
        Console.WriteLine(" ██ ██     ██    ██            ██  ████ ██  ████     ██    ██ ██   ██ ██          ██      ██    ██    ███     ███  ██      ██   ██");
        Console.WriteLine("  ███      ██    ██      █████ ██ ██ ██ ██ ██ ██     ██    ██ ██████  ██          █████   ██    ██   ███     ███   █████   ██████");
        Console.WriteLine(" ██ ██     ██    ██            ████  ██ ████  ██     ██    ██ ██   ██ ██          ██      ██    ██  ███     ███    ██      ██   ██");
        Console.WriteLine("██   ██    ██     ██████        ██████   ██████       ██████  ██   ██ ███████     ██       ██████  ███████ ███████ ███████ ██   ██");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Made by XTC-00");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("");
        Console.WriteLine("Enter the base URL (e.g., https://example.com):");
        string baseUrl = Console.ReadLine()?.TrimEnd('/');

        if (string.IsNullOrEmpty(baseUrl))
        {
            Console.WriteLine("Base URL cannot be empty.");
            return;
        }

        Console.WriteLine("Enter the path to the wordlist file:");
        string wordlistPath = Console.ReadLine();

        if (string.IsNullOrEmpty(wordlistPath) || !File.Exists(wordlistPath))
        {
            Console.WriteLine("Invalid wordlist file path.");
            return;
        }

        // Create the results directory with the current date and time
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string resultsDirectory = $"results_{timestamp}";
        Directory.CreateDirectory(resultsDirectory);

        var httpClient = new HttpClient();
        var words = File.ReadAllLines(wordlistPath);
        var totalRequests = words.Length; // Total number of words/requests

        // Use a semaphore to limit the number of concurrent requests
        int maxConcurrency = 10; // Adjust this number based on your environment and API rate limits
        using var semaphore = new SemaphoreSlim(maxConcurrency);

        try
        {
            var tasks = new List<Task>();

            foreach (var word in words)
            {
                await semaphore.WaitAsync();

                tasks.Add(Task.Run(async () =>
                {
                    string url = $"{baseUrl}/{word}";

                    try
                    {
                        var response = await httpClient.GetAsync(url);
                        var contentLength = response.Content.Headers.ContentLength ?? 0;
                        string resultLine = $"URL: {url} | Status Code: {(int)response.StatusCode} | Size: {contentLength} bytes";

                        // Add the result to the ConcurrentBag for logging later
                        lock (fileLock)
                        {
                            string statusCodeFile = Path.Combine(resultsDirectory, $"{(int)response.StatusCode}.txt");
                            File.AppendAllText(statusCodeFile, resultLine + Environment.NewLine);
                        }
                    }
                    catch (HttpRequestException e)
                    {
                        string resultLine = $"URL: {url} | Request failed: {e.Message}";

                        lock (fileLock)
                        {
                            string errorFile = Path.Combine(resultsDirectory, "errors.txt");
                            File.AppendAllText(errorFile, resultLine + Environment.NewLine);
                        }
                    }
                    catch (Exception e)
                    {
                        string resultLine = $"URL: {url} | Unexpected error: {e.Message}";
                        Console.WriteLine(resultLine);

                        lock (fileLock)
                        {
                            string errorFile = Path.Combine(resultsDirectory, "errors.txt");
                            File.AppendAllText(errorFile, resultLine + Environment.NewLine);
                        }
                    }
                    finally
                    {
                        // Increment the completed requests counter in a thread-safe manner
                        int completed = Interlocked.Increment(ref completedRequests);

                        // Update the console with progress
                        Console.WriteLine($"Progress: {completed}/{totalRequests} requests completed.");

                        semaphore.Release();
                    }
                }));
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred during processing: {e.Message}");
            lock (fileLock)
            {
                string errorFile = Path.Combine(resultsDirectory, "errors.txt");
                File.AppendAllText(errorFile, $"An error occurred during processing: {e.Message}" + Environment.NewLine);
            }
        }

        Console.WriteLine($"Results saved in directory: {resultsDirectory}");
    }
}
