namespace BuildingBlocks.CleanArchitecture.Application;

public class ValidationException : Exception
{
    public ValidationException(List<ValidationError> errors)
    {
        Errors = errors;
    }

    public List<ValidationError> Errors { get; }
}