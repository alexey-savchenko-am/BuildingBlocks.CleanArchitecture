using BuildingBlocks.CleanArchitecture.Domain.Output;

namespace BuildingBlocks.CleanArchitecture.Entities.ValueObjects;

public sealed record Currency
{
    public static readonly Currency None = new("");
    public static readonly Currency Usd = new("USD");
    public static readonly Currency Eur = new("EUR");

    public string Code { get; }

    private Currency(string code)
    {
        Code = code.ToUpperInvariant();
    }

    public static Result<Currency> FromCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return new Error("Currency.Empty", "Currency code cannot be empty");

        var upperCode = code.ToUpperInvariant();
        var currency = All.FirstOrDefault(c => c.Code == upperCode);

        return currency is not null
            ? currency
            : new Error("Currency.Invalid", $"Invalid currency code: {code}");
    }

    public static readonly IReadOnlyCollection<Currency> All = new[]
    {
        Usd,
        Eur
    };
}
