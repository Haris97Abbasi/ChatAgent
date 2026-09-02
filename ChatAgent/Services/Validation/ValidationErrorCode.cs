namespace ChatAgent.Services.Validation;

public enum ValidationErrorCode
{
    ProductNameRequired,
    VolumeRequired,
    BarcodeDataRequired,
    Ean13Required,
    Ean13NonDigits,
    Ean13WrongLength,
    Ean13InvalidChecksum
}
