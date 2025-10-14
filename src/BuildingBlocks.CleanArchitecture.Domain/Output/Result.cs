using System.Text.Json.Serialization;

namespace BuildingBlocks.CleanArchitecture.Domain.Output;

public class Result
{
    public bool IsSuccess { get; }

    [JsonIgnore]
    public bool IsFailure => !IsSuccess;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error state", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public override string ToString() =>
        IsSuccess ? "Success" : $"Failure: {Error.Code} - {Error.Message}";
}
