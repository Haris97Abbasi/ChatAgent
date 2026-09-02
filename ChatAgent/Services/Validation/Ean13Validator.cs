namespace ChatAgent.Services.Validation;

public sealed record EanValidationResult(bool IsValid, string? NormalizedValue, string? Error)
{
    public static EanValidationResult Valid(string normalizedValue) => new(true, normalizedValue, null);
    public static EanValidationResult Invalid(string error) => new(false, null, error);
}

public static class Ean13Validator
{
    public static EanValidationResult Validate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return EanValidationResult.Invalid("Barcode data is required for EAN-13.");
        }

        var digits = input.Trim();

        if (!digits.All(char.IsAsciiDigit))
        {
            return EanValidationResult.Invalid("EAN-13 barcode data must contain only digits.");
        }

        return digits.Length switch
        {
            12 => EanValidationResult.Valid(digits + ComputeCheckDigit(digits)),
            13 when ComputeCheckDigit(digits[..12]) == digits[12] - '0'
                => EanValidationResult.Valid(digits),
            13 => EanValidationResult.Invalid("EAN-13 checksum is invalid for the given digits."),
            _ => EanValidationResult.Invalid("EAN-13 barcode data must be 12 or 13 digits long.")
        };
    }

    public static int ComputeCheckDigit(string first12Digits)
    {
        var sum = 0;
        for (var i = 0; i < 12; i++)
        {
            var digit = first12Digits[i] - '0';
            sum += digit * (i % 2 == 0 ? 1 : 3);
        }

        return (10 - sum % 10) % 10;
    }
}
