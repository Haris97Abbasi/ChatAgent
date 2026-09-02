using ChatAgent.Models;

namespace ChatAgent.Services.Llm;

public interface IAgentService
{
    Task<AgentTurnResult> ProcessTurnAsync(
        LabelData currentLabel,
        IReadOnlyList<ChatMessage> conversationHistory,
        CancellationToken cancellationToken = default);
}
