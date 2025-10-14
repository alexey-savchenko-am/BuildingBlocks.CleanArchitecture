using BuildingBlocks.CleanArchitecture.Domain.Output;

namespace BuildingBlocks.CleanArchitecture.Entities.ValueObjects;

public sealed record Dimensions
{
    public decimal Length { get; }

    public decimal Width { get; }

    public decimal Height { get; }

    public decimal Weight { get; }

    public decimal Volume => Length * Width * Height;

    private Dimensions(decimal length, decimal width, decimal height, decimal weight)
    {
        Length = length;
        Width = width;
        Height = height;
        Weight = weight;
    }

    public static Result<Dimensions> Create(decimal length, decimal width, decimal height, decimal weight)
    {
        if (length <= 0 || width <= 0 || height <= 0)
            return new Error("Dimensions.InvalidSize", "Length, width and height must be greater than zero.");

        if (weight < 0)
            return new Error("Dimensions.InvalidWeight", "Weight cannot be negative.");

        return new Dimensions(length, width, height, weight);
    }

    public static Result<Dimensions> Europalete => Create(120, 80, 15, 0); // пример: 25 кг
    public static Result<Dimensions> Box => Create(40, 30, 25, 0);
    public static Result<Dimensions> EuroBoxS => Create(60, 40, 12, 0);
    public static Result<Dimensions> EuroBoxM => Create(60, 40, 22, 0);
    public static Result<Dimensions> EuroBoxL => Create(60, 40, 32, 0);
    public static Result<Dimensions> EuroBoxXL => Create(60, 40, 42, 0);
    public static Result<Dimensions> PlasticBoxS => Create(40, 30, 12, 0);
    public static Result<Dimensions> PlasticBoxM => Create(40, 30, 22, 0);
    public static Result<Dimensions> PlasticBoxL => Create(40, 30, 32, 0);
}
