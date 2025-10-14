namespace BuildingBlocks.CleanArchitecture.Application.CQRS.Behaviors;

public class ValidationException : Exception
{
    public ValidationException(List<ValidationError> errors)
    {
        Errors = errors;
    }

    public List<ValidationError> Errors { get; }
}