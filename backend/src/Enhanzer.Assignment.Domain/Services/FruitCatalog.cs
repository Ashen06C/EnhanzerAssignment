namespace Enhanzer.Assignment.Domain.Services;

public static class FruitCatalog
{
    public static readonly string[] AllowedItems =
    [
        "Mango",
        "Apple",
        "Banana",
        "Orange",
        "Grapes",
        "Kiwi",
        "Strawberry"
    ];

    public static bool IsAllowed(string itemName) =>
        AllowedItems.Any(item => string.Equals(item, itemName, StringComparison.OrdinalIgnoreCase));

    public static string Normalize(string itemName) =>
        AllowedItems.First(item => string.Equals(item, itemName, StringComparison.OrdinalIgnoreCase));
}
