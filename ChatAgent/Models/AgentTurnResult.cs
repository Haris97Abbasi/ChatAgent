namespace ChatAgent.Models;

public sealed record LabelConflict(string Field, string Description);

public sealed class AgentTurnResult
{
    public LabelData LabelDataPatch { get; set; } = new();
    public List<LabelConflict> Conflicts { get; set; } = [];
    public required string Reply { get; set; }
    public bool ReadyToGenerate { get; set; }
}
