namespace ChatAgent.Models;

public enum ChatRole
{
    User,
    Agent
}

public sealed record ChatMessage(ChatRole Role, string Text, DateTimeOffset Timestamp);
