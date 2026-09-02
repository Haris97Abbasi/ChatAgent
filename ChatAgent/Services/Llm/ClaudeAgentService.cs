using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChatAgent.Models;
using Microsoft.Extensions.Options;

namespace ChatAgent.Services.Llm;

public sealed class ClaudeAgentService : IAgentService
{
    private const string ToolName = "record_label_progress";

    private static readonly string StaticSystemPrompt = LoadSystemPrompt();
    private static readonly JsonElement ToolInputSchema = LoadToolInputSchema();

    private static readonly JsonSerializerOptions LabelJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _httpClient;
    private readonly ClaudeOptions _options;

    public ClaudeAgentService(HttpClient httpClient, IOptions<ClaudeOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<AgentTurnResult> ProcessTurnAsync(
        LabelData currentLabel,
        IReadOnlyList<ChatMessage> conversationHistory,
        CancellationToken cancellationToken = default)
    {
        var request = new ClaudeRequest
        {
            Model = _options.Model,
            MaxTokens = _options.MaxTokens,
            System = BuildSystemPrompt(currentLabel),
            Messages = conversationHistory
                .Select(m => new ClaudeMessage(m.Role == ChatRole.User ? "user" : "assistant", m.Text))
                .ToList(),
            Tools = [new ClaudeTool(ToolName, "Records the assistant's understanding of the label so far.", ToolInputSchema)],
            ToolChoice = new ClaudeToolChoice(ToolName)
        };

        using var httpResponse = await _httpClient.PostAsJsonAsync("", request, cancellationToken: cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        var claudeResponse = await httpResponse.Content.ReadFromJsonAsync<ClaudeResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Claude returned an empty response.");

        var toolUseBlock = claudeResponse.Content.FirstOrDefault(b => b.Type == "tool_use")
            ?? throw new InvalidOperationException("Claude did not return a tool_use response.");

        var toolInput = toolUseBlock.Input.Deserialize<ClaudeToolInput>()
            ?? throw new InvalidOperationException("Could not parse Claude's tool input.");

        return MapToAgentTurnResult(toolInput);
    }

    private static string BuildSystemPrompt(LabelData currentLabel)
    {
        var labelJson = JsonSerializer.Serialize(currentLabel, LabelJsonOptions);
        return $"{StaticSystemPrompt}\nCurrent known label data (JSON):\n{labelJson}";
    }

    private static AgentTurnResult MapToAgentTurnResult(ClaudeToolInput input)
    {
        var barcodeType = Enum.TryParse<BarcodeType>(input.BarcodeType, ignoreCase: true, out var parsed)
            ? parsed
            : (BarcodeType?)null;

        return new AgentTurnResult
        {
            LabelDataPatch = new LabelData
            {
                ProductName = input.ProductName,
                Volume = input.Volume,
                BarcodeType = barcodeType,
                BarcodeData = input.BarcodeData,
                Ingredients = input.Ingredients,
                BestBefore = input.BestBefore,
                Manufacturer = input.Manufacturer
            },
            Conflicts = input.Conflicts.Select(c => new LabelConflict(c.Field, c.Description)).ToList(),
            Reply = input.Reply,
            ReadyToGenerate = input.ReadyToGenerate
        };
    }

    private static string LoadSystemPrompt()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Services", "Llm", "SystemPrompt.txt");
        return File.ReadAllText(path);
    }

    private static JsonElement LoadToolInputSchema()
    {
        const string schema = """
        {
          "type": "object",
          "properties": {
            "product_name": { "type": "string", "description": "The beverage's product name. Omit if not known or not updated this turn." },
            "volume": { "type": "string", "description": "Fill volume/size, e.g. '0.5 L'. Omit if not known or not updated this turn." },
            "barcode_type": { "type": "string", "enum": ["Ean13", "Code128"], "description": "Barcode symbology. Omit unless the user specifies or changes it." },
            "barcode_data": { "type": "string", "description": "The barcode's data: digits for Ean13, any text for Code128. Omit if not known or not updated this turn." },
            "ingredients": { "type": "string", "description": "Omit unless the user mentions ingredients." },
            "best_before": { "type": "string", "description": "Omit unless the user mentions a best-before date." },
            "manufacturer": { "type": "string", "description": "Omit unless the user mentions a manufacturer." },
            "conflicts": {
              "type": "array",
              "description": "Fields where the user's latest message contradicts previously known data without clearly correcting it.",
              "items": {
                "type": "object",
                "properties": {
                  "field": { "type": "string" },
                  "description": { "type": "string" }
                },
                "required": ["field", "description"]
              }
            },
            "reply": { "type": "string", "description": "The natural-language chat message to show the user this turn." },
            "ready_to_generate": { "type": "boolean", "description": "True only when product_name, volume, and a valid barcode_data for the current barcode_type are all known and there are no unresolved conflicts." }
          },
          "required": ["conflicts", "reply", "ready_to_generate"]
        }
        """;

        return JsonDocument.Parse(schema).RootElement;
    }
}

internal sealed class ClaudeRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("max_tokens")]
    public required int MaxTokens { get; init; }

    [JsonPropertyName("system")]
    public required string System { get; init; }

    [JsonPropertyName("messages")]
    public required List<ClaudeMessage> Messages { get; init; }

    [JsonPropertyName("tools")]
    public required List<ClaudeTool> Tools { get; init; }

    [JsonPropertyName("tool_choice")]
    public required ClaudeToolChoice ToolChoice { get; init; }
}

internal sealed record ClaudeMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content);

internal sealed record ClaudeTool(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("input_schema")] JsonElement InputSchema);

internal sealed record ClaudeToolChoice([property: JsonPropertyName("name")] string Name)
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "tool";
}

internal sealed class ClaudeResponse
{
    [JsonPropertyName("content")]
    public List<ClaudeContentBlock> Content { get; init; } = [];
}

internal sealed class ClaudeContentBlock
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "";

    [JsonPropertyName("input")]
    public JsonElement Input { get; init; }
}

internal sealed class ClaudeToolInput
{
    [JsonPropertyName("product_name")]
    public string? ProductName { get; init; }

    [JsonPropertyName("volume")]
    public string? Volume { get; init; }

    [JsonPropertyName("barcode_type")]
    public string? BarcodeType { get; init; }

    [JsonPropertyName("barcode_data")]
    public string? BarcodeData { get; init; }

    [JsonPropertyName("ingredients")]
    public string? Ingredients { get; init; }

    [JsonPropertyName("best_before")]
    public string? BestBefore { get; init; }

    [JsonPropertyName("manufacturer")]
    public string? Manufacturer { get; init; }

    [JsonPropertyName("conflicts")]
    public List<ClaudeConflict> Conflicts { get; init; } = [];

    [JsonPropertyName("reply")]
    public required string Reply { get; init; }

    [JsonPropertyName("ready_to_generate")]
    public bool ReadyToGenerate { get; init; }
}

internal sealed class ClaudeConflict
{
    [JsonPropertyName("field")]
    public required string Field { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }
}
