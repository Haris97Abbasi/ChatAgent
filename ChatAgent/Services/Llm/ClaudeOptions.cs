namespace ChatAgent.Services.Llm;

public sealed class ClaudeOptions
{
    public const string SectionName = "Claude";

    public string BaseUrl { get; set; } = "https://api.anthropic.com/v1/messages";
    public string ApiKey { get; set; } = "";
    public string Model { get; set; } = "claude-sonnet-5";
    public int MaxTokens { get; set; } = 1024;
}
