namespace Gehtsoft.FourCDesigner.Logic;

public class ValidationError
{
    public string Field { get; set; }
    public string[] Messages { get; set; }

    public ValidationError(string field, params string[] messages)
    {
        Field = field ?? throw new ArgumentNullException(nameof(field));
        Messages = messages ?? throw new ArgumentNullException(nameof(messages));
    }

    public ValidationError(string field, List<string> messages)
    {
        Field = field ?? throw new ArgumentNullException(nameof(field));
        Messages = messages?.ToArray() ?? throw new ArgumentNullException(nameof(messages));
    }
}
