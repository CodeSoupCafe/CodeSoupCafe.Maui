namespace CodeSoupCafe.Maui.Models;

public interface IDateNow
{
    DateTimeOffset DateCreated { get; }

    DateTimeOffset DateUpdated { get; }
}
