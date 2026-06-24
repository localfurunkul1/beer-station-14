namespace Content.Shared.Beer.Rating;

public static class BeerRatingTags
{
    public static readonly (string Tag, bool IsPositive)[] All =
    {
        ("Умный", true),
        ("Харизматичный", true),
        ("Хороший товарищ", true),
        ("Профессионал", true),
        ("Героический", true),
        ("Глупый", false),
        ("Не выполняет обязанности", false),
        ("Гриф", false),
        ("Токсичный", false),
        ("Высокомерный", false),
    };
}
