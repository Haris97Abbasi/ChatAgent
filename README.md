# ChatAgent

A chat-based agent that turns a natural-language description of a beverage product — possibly
incomplete or contradictory — into a print-ready product label with a real barcode.

Built for TEC-IT's ".NET Software Engineering" home task ("Chat-Agent für druckfertige
Getränkeetiketten"). An employee describes a product over several chat messages; the agent asks
focused follow-up questions when required information is missing or when two messages conflict,
and once it has a complete, valid label it renders a preview with a real barcode image ready to
print.

## What it does

- Multi-turn chat UI (Blazor Server) — not a form, not a single request/response.
- An LLM (Claude) extracts structured label data (product name, volume, barcode type/data,
  optional ingredients / best-before / manufacturer) from the conversation each turn, decides
  whether anything is missing or conflicting, and writes the next chat reply.
- Deterministic validation (EAN-13 length/checksum, required-field completeness) always has the
  final say — the LLM's own claim that a label is "ready" is verified in code, not trusted blindly.
- The barcode image itself comes from the real [TEC-IT Barcode API](https://barcode.tec-it.com/),
  requested through a small backend proxy so the API key never reaches the browser.
- The finished label renders as a preview card with a working **Print** button (browser print, via
  a dedicated print stylesheet).
- The agent's own chat replies automatically match whichever language (German or English) the user
  is writing in; a small EN/DE toggle additionally covers the static UI text (buttons, labels,
  deterministic validation messages) which don't follow the conversation language on their own.
- A "Start new label" button resets the conversation without restarting the app.

## Project layout

- `ChatAgent/` — the Blazor Web App (Interactive Server render mode, .NET 10).
- `ChatAgent.Tests/` — xUnit tests for the deterministic validators and the TEC-IT query-string
  builder (pure logic, no network calls).
- `ChatAgent.slnx` — solution file.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download).
- An **Anthropic API key**. Use a standard key from the
  [Anthropic Console](https://console.anthropic.com/) (Settings → API Keys) — an
  identity-linked/organization key that requires an `anthropic-workspace-id` header is not
  supported here.
- A **TEC-IT Barcode API** `accessid`.

Neither credential is ever hard-coded — both are read from .NET User Secrets (see below).

## Setup

1. Restore and build:

   ```
   dotnet build ChatAgent.slnx
   ```

2. Configure your secrets (run from the repository root):

   ```
   dotnet user-secrets set "TecIt:AccessId" "<your-tec-it-access-id>" --project ChatAgent/ChatAgent.csproj
   dotnet user-secrets set "Claude:ApiKey" "<your-anthropic-api-key>" --project ChatAgent/ChatAgent.csproj
   ```

   These are stored outside the repository by the .NET User Secrets tooling, not in
   `appsettings.json` and not in source control.

## Running the app

```
dotnet run --project ChatAgent
```

Then open the URL printed in the console (typically `http://localhost:5149`). Alternatively, open
`ChatAgent.slnx` in Visual Studio and run/debug the `ChatAgent` project directly (F5).

## Running the tests

```
dotnet test ChatAgent.slnx
```

Covers `Ean13Validator`, `LabelValidator`, and the TEC-IT barcode query-string builder — all pure,
deterministic logic with no external calls.

## Using it

Just start describing the product, e.g.:

> "New cola, 500ml, EAN 4006381333931"

or split it across several messages — the agent will ask for whatever's still missing (a barcode
number, a product name, ...) and flag it if you say something that contradicts an earlier message,
instead of silently guessing. Once everything required is known and valid, a label preview with a
real barcode appears on the right, ready to print.
