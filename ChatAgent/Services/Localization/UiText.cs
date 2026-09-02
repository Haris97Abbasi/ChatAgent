using ChatAgent.Services.Validation;

namespace ChatAgent.Services.Localization;

public static class UiText
{
    private static readonly Dictionary<string, (string En, string De)> Strings = new()
    {
        ["AppSubtitle"] = ("Beverage Labels", "Getränkeetiketten"),
        ["InputPlaceholder"] = ("Describe your beverage label...", "Beschreiben Sie Ihr Getränkeetikett..."),
        ["Send"] = ("Send", "Senden"),
        ["AgentThinking"] = ("Agent is thinking…", "Agent denkt nach…"),
        ["PrintLabel"] = ("Print label", "Etikett drucken"),
        ["Ingredients"] = ("Ingredients", "Zutaten"),
        ["BestBefore"] = ("Best before", "Mindesthaltbar bis"),
        ["Manufacturer"] = ("Manufacturer", "Hersteller"),
        ["NewLabel"] = ("Start new label", "Neues Etikett starten"),
    };

    public static string Get(string key, UiLanguage language)
    {
        var (en, de) = Strings[key];
        return language == UiLanguage.German ? de : en;
    }

    public static string Get(ValidationErrorCode code, UiLanguage language) => code switch
    {
        ValidationErrorCode.ProductNameRequired => language == UiLanguage.German
            ? "Der Produktname wird benötigt."
            : "Product name is required.",
        ValidationErrorCode.VolumeRequired => language == UiLanguage.German
            ? "Die Füllmenge wird benötigt."
            : "Volume is required.",
        ValidationErrorCode.BarcodeDataRequired => language == UiLanguage.German
            ? "Barcode-Daten werden benötigt."
            : "Barcode data is required.",
        ValidationErrorCode.Ean13Required => language == UiLanguage.German
            ? "Für EAN-13 werden Barcode-Daten benötigt."
            : "Barcode data is required for EAN-13.",
        ValidationErrorCode.Ean13NonDigits => language == UiLanguage.German
            ? "EAN-13-Daten dürfen nur Ziffern enthalten."
            : "EAN-13 barcode data must contain only digits.",
        ValidationErrorCode.Ean13WrongLength => language == UiLanguage.German
            ? "EAN-13-Daten müssen 12 oder 13 Ziffern lang sein."
            : "EAN-13 barcode data must be 12 or 13 digits long.",
        ValidationErrorCode.Ean13InvalidChecksum => language == UiLanguage.German
            ? "Die EAN-13-Prüfziffer ist für diese Ziffern ungültig."
            : "EAN-13 checksum is invalid for the given digits.",
        _ => code.ToString()
    };

    public static string MessageTooLong(int length, int max, UiLanguage language) => language == UiLanguage.German
        ? $"Diese Nachricht ist zu lang ({length} Zeichen) - bitte unter {max} Zeichen bleiben."
        : $"That message is too long ({length} characters) - please keep it under {max} characters.";

    public static string AssistantUnavailable(UiLanguage language) => language == UiLanguage.German
        ? "Entschuldigung, der Assistent war gerade nicht erreichbar. Bitte versuchen Sie es erneut."
        : "Sorry, I couldn't reach the assistant in time. Please try again.";

    public static string HoldOnBeforeGenerate(string reason, UiLanguage language) => language == UiLanguage.German
        ? $"Einen Moment - bevor ich das Etikett erstelle: {reason}"
        : $"Hold on - before I generate the label: {reason}";
}
