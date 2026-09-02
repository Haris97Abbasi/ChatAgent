using ChatAgent.Models;
using ChatAgent.Services.Validation;

namespace ChatAgent.Tests.Services.Validation;

public class LabelValidatorTests
{
    private static LabelData ValidEan13Label() => new()
    {
        ProductName = "Cola Classic",
        Volume = "0.5 L",
        BarcodeType = BarcodeType.Ean13,
        BarcodeData = "4006381333931"
    };

    [Fact]
    public void Validate_WithCompleteValidEan13Label_ReturnsValid()
    {
        var result = LabelValidator.Validate(ValidEan13Label());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithMissingProductName_ReturnsProductNameError()
    {
        var label = ValidEan13Label();
        label.ProductName = null;

        var result = LabelValidator.Validate(label);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Field == nameof(LabelData.ProductName));
    }

    [Fact]
    public void Validate_WithMissingVolume_ReturnsVolumeError()
    {
        var label = ValidEan13Label();
        label.Volume = "  ";

        var result = LabelValidator.Validate(label);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Field == nameof(LabelData.Volume));
    }

    [Fact]
    public void Validate_WithInvalidEan13Checksum_ReturnsBarcodeDataError()
    {
        var label = ValidEan13Label();
        label.BarcodeData = "4006381333932";

        var result = LabelValidator.Validate(label);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Field == nameof(LabelData.BarcodeData));
    }

    [Fact]
    public void Validate_WithNullBarcodeType_DefaultsToEan13Rules()
    {
        var label = ValidEan13Label();
        label.BarcodeType = null;
        label.BarcodeData = "not-digits";

        var result = LabelValidator.Validate(label);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Field == nameof(LabelData.BarcodeData));
    }

    [Fact]
    public void Validate_WithCode128AndEmptyBarcodeData_ReturnsBarcodeDataError()
    {
        var label = ValidEan13Label();
        label.BarcodeType = BarcodeType.Code128;
        label.BarcodeData = null;

        var result = LabelValidator.Validate(label);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Field == nameof(LabelData.BarcodeData));
    }

    [Fact]
    public void Validate_WithCode128AndAnyNonEmptyBarcodeData_ReturnsValid()
    {
        var label = ValidEan13Label();
        label.BarcodeType = BarcodeType.Code128;
        label.BarcodeData = "ABC-123";

        var result = LabelValidator.Validate(label);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithAllFieldsMissing_ReturnsAllErrors()
    {
        var label = new LabelData();

        var result = LabelValidator.Validate(label);

        Assert.False(result.IsValid);
        Assert.Equal(3, result.Errors.Count);
    }
}
