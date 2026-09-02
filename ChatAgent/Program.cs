using ChatAgent.Components;
using ChatAgent.Models;
using ChatAgent.Services;
using ChatAgent.Services.Llm;
using ChatAgent.Services.TecIt;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<TecItOptions>(builder.Configuration.GetSection(TecItOptions.SectionName));
builder.Services.AddHttpClient<TecItBarcodeClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<TecItOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.Configure<ClaudeOptions>(builder.Configuration.GetSection(ClaudeOptions.SectionName));
builder.Services.AddHttpClient<IAgentService, ClaudeAgentService>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<ClaudeOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
    client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<ChatSessionState>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/api/barcode-image", async (string data, string? type, TecItBarcodeClient client, ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    var barcodeType = Enum.TryParse<BarcodeType>(type, ignoreCase: true, out var parsed) ? parsed : BarcodeType.Ean13;
    var label = new LabelData { BarcodeType = barcodeType, BarcodeData = data };

    try
    {
        var result = await client.GetBarcodeImageAsync(label, cancellationToken);
        return Results.File(result.ImageBytes, result.ContentType);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "TEC-IT barcode request failed for barcode type {BarcodeType}", barcodeType);
        return Results.Problem("Could not generate the barcode image.", statusCode: StatusCodes.Status502BadGateway);
    }
});

app.Run();
