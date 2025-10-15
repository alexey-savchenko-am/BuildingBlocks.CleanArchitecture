using BuildingBlocks.CleanArchitecture.Domain.Output;

namespace BuildingBlocks.CleanArchitecture.Application;

public sealed class ValidationError
    : Error
{
    public ValidationError(string code, string message)
        : base(code, message)
    {
    }
}
