# Ai Log Explainer (CLI)

Ai Log Explainer is a C# command-line application that uses OpenAI to analyze .log and .txt files.
It reads logs as raw plaintext, summarizes the issue in clear language, identifies likely root causes, and suggests actionable fixes.

Built for developers who want fast signal instead of manually parsing noisy logs.

# Features

- Plaintext analysis of .log and .txt files

- AI-generated simplified explanation of errors

- Bullet-pointed possible causes

- Bullet-pointed suggested fixes

- Strict, predictable output format

- Secure local storage of API key

- Lightweight, single-executable CLI

# How It Works

On first run, the app asks for your OpenAI API key

The key is stored locally in your AppData directory

You paste the full path to a log file

The application:

Reads up to the first 12,000 characters of the file

Sends the content to OpenAI with a strict analysis prompt

Outputs a structured analysis report to the console

# Requirements

- .NET 6.0 or newer

Internet connection

OpenAI API key

Running the Application

Build and run the project using the .NET CLI or your IDE:

`dotnet run`


Follow the on-screen prompts to enter:

Your OpenAI API key (first run only)

The full path to a .log or .txt file

# Supported Files

- .log

- .txt

Other formats are rejected.

# Output Format


The AI is strictly instructed to output only the following sections, in this exact order:
```plaintext
Simplified Explanation:
- Clear, human-readable explanation of the issue.

Possible Causes:
- Bullet list of likely root causes.

Suggested Fixes:
- Bullet list of actionable fixes or next steps.


No introductions, no disclaimers, no repeated log content.
```

API Key Storage

The API key is stored locally at:

`%APPDATA%\LogAnalyzerAI\config.json`

The key is only accessed by the application.

# Technical Details

Reads logs with a safe character cap (12,000 chars)

Uses HttpClient and OpenAI Chat Completions API

Model: gpt-4.1-mini

Temperature: 0.2 (low randomness, consistent output)

Fully async execution

# Limitations

Only the first 12,000 characters are analyzed

Very large or multi-error logs may be partially summarized

AI suggestions are best-effort and must be validated

No chunking or streaming yet

# Disclaimer

This tool provides diagnostic assistance, not guaranteed fixes.
Always review suggestions before applying them in production.
