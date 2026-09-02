using ChatAgent.Services.Validation;

namespace ChatAgent.Tests.Services.Validation;

public class Ean13ValidatorTests
{
    private const string ValidEan13 = "4006381333931";
    private const string ValidFirst12Digits = "400638133393";

    [Fact]
    public void Validate_WithValid13Digits_ReturnsValid()
    {
        var result = Ean13Validator.Validate(ValidEan13);

        Assert.True(result.IsValid);
        Assert.Equal(ValidEan13, result.NormalizedValue);
    }

    [Fact]
    public void Validate_With12Digits_ComputesCorrectCheckDigitAndReturnsValid()
    {
        var result = Ean13Validator.Validate(ValidFirst12Digits);

        Assert.True(result.IsValid);
        Assert.Equal(ValidEan13, result.NormalizedValue);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345678901234")]
    public void Validate_WithWrongLength_ReturnsInvalid(string input)
    {
        var result = Ean13Validator.Validate(input);

        Assert.False(result.IsValid);
        Assert.Null(result.NormalizedValue);
    }

    [Fact]
    public void Validate_WithNonDigitCharacters_ReturnsInvalid()
    {
        var result = Ean13Validator.Validate("400638133393A");

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_With13DigitsAndWrongChecksum_ReturnsInvalid()
    {
        var result = Ean13Validator.Validate("4006381333932");

        Assert.False(result.IsValid);
        Assert.Null(result.NormalizedValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithNullOrWhitespace_ReturnsInvalid(string? input)
    {
        var result = Ean13Validator.Validate(input);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ComputeCheckDigit_ForKnownDigits_ReturnsExpectedCheckDigit()
    {
        var checkDigit = Ean13Validator.ComputeCheckDigit(ValidFirst12Digits);

        Assert.Equal(1, checkDigit);
    }
}
