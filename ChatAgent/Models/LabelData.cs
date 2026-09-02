namespace ChatAgent.Models;

public sealed class LabelData
{
    public string? ProductName { get; set; }
    public string? Volume { get; set; }
    public BarcodeType? BarcodeType { get; set; }
    public string? BarcodeData { get; set; }
    public string? Ingredients { get; set; }
    public string? BestBefore { get; set; }
    public string? Manufacturer { get; set; }
}
