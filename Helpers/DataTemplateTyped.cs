namespace CodeSoupCafe.Maui.Helpers;

public class DataTemplateTyped<T> : DataTemplate
{
    public DataTemplateTyped() : base(typeof(T))
    {
    }
}
