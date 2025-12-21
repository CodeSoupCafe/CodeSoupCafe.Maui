namespace CodeSoupCafe.Maui.Models;

public abstract class ItemState : ISortable
{
    public abstract Guid Id { get; }
    public abstract string Title { get; }
    public abstract DateTimeOffset DateCreated { get; }
    public abstract DateTimeOffset DateUpdated { get; }

    public virtual string? ThumbnailBase64 { get; set; }

    public override abstract bool Equals(object? other);
    public override abstract int GetHashCode();
}
