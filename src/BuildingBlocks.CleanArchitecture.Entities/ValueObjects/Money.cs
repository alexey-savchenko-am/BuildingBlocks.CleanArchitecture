using BuildingBlocks.CleanArchitecture.Domain.Output;
using BuildingBlocks.Domain.ValueObjects;

namespace BuildingBlocks.CleanArchitecture.Entities.ValueObjects;

public record Money
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Result<Money> Create(decimal amount, Currency currency)
    {
        if (currency == Currency.None)
            return new Error("Money.InvalidCurrency", "Currency must be specified.");

        if (amount < 0)
            return new Error("Money.NegativeAmount", "Amount cannot be negative.");

        return new Money(amount, currency);
    }

    public static Result<Money> CreateUsd(decimal amount) => Create(amount, Currency.Usd);

    public static Money Zero(Currency currency) => new(0, currency);

    public bool IsZero() => Amount == 0;

    public static Result<Money> operator +(Money first, Money second)
    {
        if (first.Currency != second.Currency)
            return new Error("Money.CurrencyMismatch", "Currencies must match to add amounts.");

        return new Money(first.Amount + second.Amount, first.Currency);
    }
}