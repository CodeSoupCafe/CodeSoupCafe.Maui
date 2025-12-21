namespace CodeSoupCafe.Maui.Models;

public interface ISortable : IDateNow
{
    Guid Id { get; }

    string Title { get; }
}
