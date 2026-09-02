using ChatAgent.Models;
using ChatAgent.Services.Llm;
using ChatAgent.Services.Localization;
using ChatAgent.Services.Validation;

namespace ChatAgent.Services;

public sealed class ChatSessionState(IAgentService agentService, ILogger<ChatSessionState> logger)
{
    private const int MaxMessageLength = 2000;

    private readonly List<ChatMessage> _messages = [];

    public IReadOnlyList<ChatMessage> Messages => _messages;
    public LabelData CurrentLabel { get; private set; } = new();
    public bool IsProcessing { get; private set; }
    public bool IsLabelReady { get; private set; }
    public UiLanguage Language { get; private set; } = UiLanguage.English;

    public event Action? StateChanged;

    public void SetLanguage(UiLanguage language)
    {
        if (Language == language)
        {
            return;
        }

        Language = language;
        StateChanged?.Invoke();
    }

    public async Task SendUserMessageAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || IsProcessing)
        {
            return;
        }

        if (text.Length > MaxMessageLength)
        {
            _messages.Add(new ChatMessage(
                ChatRole.Agent,
                UiText.MessageTooLong(text.Length, MaxMessageLength, Language),
                DateTimeOffset.UtcNow));
            StateChanged?.Invoke();
            return;
        }

        _messages.Add(new ChatMessage(ChatRole.User, text, DateTimeOffset.UtcNow));
        IsProcessing = true;
        IsLabelReady = false;
        StateChanged?.Invoke();

        var replyText = await ProcessTurnAsync();

        _messages.Add(new ChatMessage(ChatRole.Agent, replyText, DateTimeOffset.UtcNow));
        IsProcessing = false;
        StateChanged?.Invoke();
    }

    private async Task<string> ProcessTurnAsync()
    {
        AgentTurnResult result;
        try
        {
            result = await agentService.ProcessTurnAsync(CurrentLabel, _messages);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Agent turn processing failed");
            return UiText.AssistantUnavailable(Language);
        }

        CurrentLabel = MergePatch(CurrentLabel, result.LabelDataPatch);

        var validation = LabelValidator.Validate(CurrentLabel);
        IsLabelReady = result.ReadyToGenerate && validation.IsValid && result.Conflicts.Count == 0;

        if (result.ReadyToGenerate && !IsLabelReady)
        {
            var reason = !validation.IsValid
                ? UiText.Get(validation.Errors[0].Code, Language)
                : result.Conflicts[0].Description;
            return UiText.HoldOnBeforeGenerate(reason, Language);
        }

        return result.Reply;
    }

    private static LabelData MergePatch(LabelData current, LabelData patch) => new()
    {
        ProductName = patch.ProductName ?? current.ProductName,
        Volume = patch.Volume ?? current.Volume,
        BarcodeType = patch.BarcodeType ?? current.BarcodeType,
        BarcodeData = patch.BarcodeData ?? current.BarcodeData,
        Ingredients = patch.Ingredients ?? current.Ingredients,
        BestBefore = patch.BestBefore ?? current.BestBefore,
        Manufacturer = patch.Manufacturer ?? current.Manufacturer
    };
}
