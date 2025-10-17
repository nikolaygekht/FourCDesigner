namespace Gehtsoft.FourCDesigner.Logic;

public class ValidationException : Exception
{
    public ValidationError[] Errors { get; }

    public ValidationException(string message, params ValidationError[] errors)
        : base(message)
    {
        Errors = errors ?? throw new ArgumentNullException(nameof(errors));
    }

    public ValidationException(params ValidationError[] errors)
        : base(BuildMessage(errors))
    {
        Errors = errors ?? throw new ArgumentNullException(nameof(errors));
    }

    private static string BuildMessage(ValidationError[] errors)
    {
        if (errors == null || errors.Length == 0)
            return "Validation failed";

        var messages = new List<string>();
        foreach (var error in errors)
        {
            foreach (var message in error.Messages)
                messages.Add($"{error.Field}: {message}");
        }

        return string.Join("; ", messages);
    }
}
