namespace CodeSoupCafe.Maui.Behaviors;

using CodeSoupCafe.Maui.Models;

public class ItemAppearingBehavior : Behavior<VisualElement>
{
    public static readonly BindableProperty AppearingActionProperty =
        BindableProperty.Create(
            nameof(AppearingAction),
            typeof(Action<ISortable>),
            typeof(ItemAppearingBehavior),
            null);

    public static readonly BindableProperty DisappearingActionProperty =
        BindableProperty.Create(
            nameof(DisappearingAction),
            typeof(Action<ISortable>),
            typeof(ItemAppearingBehavior),
            null);

    public Action<ISortable>? AppearingAction
    {
        get => (Action<ISortable>?)GetValue(AppearingActionProperty);
        set => SetValue(AppearingActionProperty, value);
    }

    public Action<ISortable>? DisappearingAction
    {
        get => (Action<ISortable>?)GetValue(DisappearingActionProperty);
        set => SetValue(DisappearingActionProperty, value);
    }

    private bool hasAppeared = false;

    protected override void OnAttachedTo(VisualElement bindable)
    {
        base.OnAttachedTo(bindable);
        bindable.Loaded += OnLoaded;
        bindable.Unloaded += OnUnloaded;
        bindable.BindingContextChanged += OnBindingContextChanged;
    }

    protected override void OnDetachingFrom(VisualElement bindable)
    {
        bindable.Loaded -= OnLoaded;
        bindable.Unloaded -= OnUnloaded;
        bindable.BindingContextChanged -= OnBindingContextChanged;
        base.OnDetachingFrom(bindable);
    }

    private void OnBindingContextChanged(object? sender, EventArgs e)
    {
        // Reset state when binding context changes (view recycling)
        hasAppeared = false;
        
        if (sender is VisualElement element && element.IsLoaded && element.BindingContext is ISortable item)
        {
            hasAppeared = true;
            AppearingAction?.Invoke(item);
        }
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        if (!hasAppeared && sender is VisualElement element && element.BindingContext is ISortable item)
        {
            hasAppeared = true;
            AppearingAction?.Invoke(item);
        }
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        hasAppeared = false;
        if (sender is VisualElement element && element.BindingContext is ISortable item)
        {
            DisappearingAction?.Invoke(item);
        }
    }
}
