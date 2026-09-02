using ChatAgent.Models;

namespace ChatAgent.Services;

public sealed class ChatSessionState
{
    private readonly List<ChatMessage> _messages = [];

    public IReadOnlyList<ChatMessage> Messages => _messages;
    public LabelData CurrentLabel { get; private set; } = new();
    public bool IsProcessing { get; private set; }

    public event Action? StateChanged;

    public async Task SendUserMessageAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || IsProcessing)
        {
            return;
        }

        _messages.Add(new ChatMessage(ChatRole.User, text, DateTimeOffset.UtcNow));
        IsProcessing = true;
        StateChanged?.Invoke();

        var reply = await GenerateStubAgentReplyAsync(text);
        _messages.Add(new ChatMessage(ChatRole.Agent, reply, DateTimeOffset.UtcNow));

        IsProcessing = false;
        StateChanged?.Invoke();
    }

    private static async Task<string> GenerateStubAgentReplyAsync(string userText)
    {
        await Task.Delay(500);
        return $"(placeholder reply) You said: \"{userText}\".";
    }
}
