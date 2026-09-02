namespace ChatAgent.Services.TecIt;

public sealed class TecItOptions
{
    public const string SectionName = "TecIt";

    public string BaseUrl { get; set; } = "https://barcode.tec-it.com/barcode.ashx";
    public string AccessId { get; set; } = "";
}
