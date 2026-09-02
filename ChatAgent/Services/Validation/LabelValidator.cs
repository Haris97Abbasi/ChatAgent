using ChatAgent.Models;

namespace ChatAgent.Services.Validation;

public sealed record LabelFieldError(string Field, string Message, ValidationErrorCode Code);

public sealed record LabelValidationResult(bool IsValid, IReadOnlyList<LabelFieldError> Errors)
{
    public static LabelValidationResult Valid() => new(true, []);
    public static LabelValidationResult Invalid(IReadOnlyList<LabelFieldError> errors) => new(false, errors);
}

public static class LabelValidator
{
    public static LabelValidationResult Validate(LabelData label)
    {
        var errors = new List<LabelFieldError>();

        if (string.IsNullOrWhiteSpace(label.ProductName))
        {
            errors.Add(new LabelFieldError(nameof(LabelData.ProductName), "Product name is required.", ValidationErrorCode.ProductNameRequired));
        }

        if (string.IsNullOrWhiteSpace(label.Volume))
        {
            errors.Add(new LabelFieldError(nameof(LabelData.Volume), "Volume is required.", ValidationErrorCode.VolumeRequired));
        }

        var barcodeType = label.BarcodeType ?? BarcodeType.Ean13;

        if (barcodeType == BarcodeType.Ean13)
        {
            var eanResult = Ean13Validator.Validate(label.BarcodeData);
            if (!eanResult.IsValid)
            {
                errors.Add(new LabelFieldError(nameof(LabelData.BarcodeData), eanResult.Error!, eanResult.ErrorCode!.Value));
            }
        }
        else if (string.IsNullOrWhiteSpace(label.BarcodeData))
        {
            errors.Add(new LabelFieldError(nameof(LabelData.BarcodeData), "Barcode data is required.", ValidationErrorCode.BarcodeDataRequired));
        }

        return errors.Count == 0 ? LabelValidationResult.Valid() : LabelValidationResult.Invalid(errors);
    }
}
