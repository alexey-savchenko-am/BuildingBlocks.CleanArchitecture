using System.Text.Json.Serialization;

namespace BuildingBlocks.CleanArchitecture.Domain.Output;
public class Error : IEquatable<Error>
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Code { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Message { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Error? InnerError { get; }

    protected Error() { }

    public Error(string code, string message)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }

    public Error(string code, string message, Error innerError)
        : this(code, message)
    {
        InnerError = innerError ?? throw new ArgumentNullException(nameof(innerError));
    }

    public static readonly Error None = new();
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");

    public static implicit operator string?(Error? error) => error?.Code;
    public static implicit operator Result(Error error) => Result.Failure(error);

    public bool Equals(Error? other) =>
        other is not null &&
        Code == other.Code &&
        Message == other.Message;

    public override bool Equals(object? obj) => obj is Error other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Code, Message);

    public override string ToString() => Code ?? base.ToString()!;
}
