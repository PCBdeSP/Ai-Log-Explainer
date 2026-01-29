using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        try
        {
            Console.WriteLine("LogAnalyzerAI\n");

            string apiKey = ConfigManager.GetApiKey();

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.Write("Please enter your OpenAI API key:\n> ");
                apiKey = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(apiKey))
                    throw new Exception("No API key provided.");

                ConfigManager.SaveApiKey(apiKey);
                Console.WriteLine("\nAPI key saved.\n");
            }

            Console.Write("To use this application, please copy the exact path to the log file and paste it.\n> ");
            string path = Console.ReadLine()?.Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(path))
                throw new Exception("No file path provided.");

            if (!File.Exists(path))
                throw new FileNotFoundException("File not found.", path);

            Console.WriteLine("\nReading and simplifying...\n");

            string logContent = ReadFileSafely(path, 12000);
            string prompt = BuildPrompt(logContent);

            string result = await OpenAIClient.Analyze(apiKey, prompt);

            Console.WriteLine("===== ANALYSIS REPORT =====\n");
            Console.WriteLine(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine("\n❌ ERROR:");
            Console.WriteLine(ex.ToString());
        }
        finally
        {
            Console.WriteLine("\nPress ENTER to exit...");
            Console.ReadLine();
        }
    }


    static string ReadFileSafely(string path, int maxChars)
    {
        using var reader = new StreamReader(path);
        char[] buffer = new char[maxChars];
        int read = reader.Read(buffer, 0, maxChars);
        return new string(buffer, 0, read);
    }

    static string BuildPrompt(string log)
    {
        return @$"
You must follow the output format exactly.

DO NOT:
- Introduce yourself
- Explain what you are
- Use phrases like ""As an AI"", ""I"", ""We"", ""In my opinion""
- Add any text before or after the required sections

ONLY output the following sections, in this exact order and wording:

Simplified Explanation:
- Clear, human-readable explanation of the issue.

Possible Causes:
- Bullet list of likely root causes.

Suggested Fixes:
- Bullet list of actionable fixes or next steps.

If information is missing or unclear, state uncertainty inside the relevant section.
Do not repeat the raw log.

LOG:
{log}";
    }
}

static class ConfigManager
{
    static string ConfigDir =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LogAnalyzerAI");

    static string ConfigPath => Path.Combine(ConfigDir, "config.json");

    public static string GetApiKey()
    {
        if (!File.Exists(ConfigPath))
            return null;

        var json = File.ReadAllText(ConfigPath);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("apiKey").GetString();
    }

    public static void SaveApiKey(string key)
    {
        Directory.CreateDirectory(ConfigDir);
        var json = JsonSerializer.Serialize(new { apiKey = key });
        File.WriteAllText(ConfigPath, json);
    }
}

static class OpenAIClient
{
    public static async Task<string> Analyze(string apiKey, string prompt)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var payload = new
        {
            model = "gpt-4.1-mini",
            temperature = 0.2,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = "You output structured technical analysis. You never introduce yourself."
                },
                new
                {
                    role = "user",
                    content = prompt
                }
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(
            "https://api.openai.com/v1/chat/completions",
            content);

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);

        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
    }
}
