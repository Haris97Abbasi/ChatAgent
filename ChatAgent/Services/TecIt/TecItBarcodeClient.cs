using ChatAgent.Models;
using Microsoft.Extensions.Options;

namespace ChatAgent.Services.TecIt;

public sealed record BarcodeImageResult(byte[] ImageBytes, string ContentType);

public sealed class TecItBarcodeClient(HttpClient httpClient, IOptions<TecItOptions> options)
{
    private readonly TecItOptions _options = options.Value;

    public async Task<BarcodeImageResult> GetBarcodeImageAsync(LabelData label, CancellationToken cancellationToken = default)
    {
        var query = BuildQueryString(label, _options.AccessId);

        using var response = await httpClient.GetAsync($"?{query}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/png";
        var imageBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        return new BarcodeImageResult(imageBytes, contentType);
    }

    public static string BuildQueryString(LabelData label, string accessId)
    {
        var barcodeType = label.BarcodeType ?? BarcodeType.Ean13;
        var code = barcodeType switch
        {
            BarcodeType.Ean13 => "EAN13",
            BarcodeType.Code128 => "Code128",
            _ => throw new ArgumentOutOfRangeException(nameof(label), barcodeType, "Unsupported barcode type.")
        };

        var parameters = new (string Key, string Value)[]
        {
            ("accessid", accessId),
            ("code", code),
            ("data", label.BarcodeData ?? ""),
            ("imagetype", "png")
        };

        return string.Join("&", parameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
    }
}
