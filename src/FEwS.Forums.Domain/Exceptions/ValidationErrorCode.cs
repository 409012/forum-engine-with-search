namespace FEwS.Forums.Domain.Exceptions;

public static class ValidationErrorCode
{
    public static string Empty { get; set; } = nameof(Empty);
    public static string TooLong { get; set; } = nameof(TooLong);
    public static string Invalid { get; set; } = nameof(Invalid);
}