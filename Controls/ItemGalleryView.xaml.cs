namespace CodeSoupCafe.Maui.Controls;

using CodeSoupCafe.Maui.Models;
using CodeSoupCafe.Maui.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

using CodeSoupCafe.Maui.Behaviors;

public partial class ItemGalleryView : ContentView
{
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable<ISortable>), typeof(ItemGalleryView), null, propertyChanged: OnItemsSourceChanged);

    public static readonly BindableProperty ItemTemplateProperty =
        BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(ItemGalleryView), null, propertyChanged: OnItemTemplateChanged);

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(ItemGalleryView), "Gallery");

    public static readonly BindableProperty SearchPlaceholderProperty =
        BindableProperty.Create(nameof(SearchPlaceholder), typeof(string), typeof(ItemGalleryView), "Search items...");

    public static readonly BindableProperty EmptyMessageProperty =
        BindableProperty.Create(nameof(EmptyMessage), typeof(string), typeof(ItemGalleryView), "No items found");

    public static readonly BindableProperty GridSpanCountProperty =
        BindableProperty.Create(nameof(GridSpanCount), typeof(int), typeof(ItemGalleryView), 2);

    public static readonly BindableProperty IsGridModeProperty =
        BindableProperty.Create(nameof(IsGridMode), typeof(bool), typeof(ItemGalleryView), true, defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty ItemAppearingActionProperty =
        BindableProperty.Create(nameof(ItemAppearingAction), typeof(Action<ISortable>), typeof(ItemGalleryView), null);

    public static readonly BindableProperty ItemDisappearingActionProperty =
        BindableProperty.Create(nameof(ItemDisappearingAction), typeof(Action<ISortable>), typeof(ItemGalleryView), null);

    public static readonly BindableProperty RemainingItemsThresholdProperty =
        BindableProperty.Create(nameof(RemainingItemsThreshold), typeof(int), typeof(ItemGalleryView), 5);

    public static readonly BindableProperty RemainingItemsThresholdReachedCommandProperty =
        BindableProperty.Create(nameof(RemainingItemsThresholdReachedCommand), typeof(ICommand), typeof(ItemGalleryView), null);
    public static readonly BindableProperty ItemContextCommandsProperty =
        BindableProperty.Create(nameof(ItemContextCommands), typeof(IEnumerable<GalleryContextCommand>), typeof(ItemGalleryView), null, propertyChanged: OnItemContextCommandsChanged);


    public ItemGalleryViewModel ViewModel { get; }

    public ItemGalleryView()
    {
        InitializeComponent();

        ViewModel = new ItemGalleryViewModel();
        RootContainer.BindingContext = ViewModel;

        // Subscribe to item selected event from ViewModel
        ViewModel.ItemSelected += OnItemSelected;

        // Create command for item taps
        ItemTappedCommand = new RelayCommand<ISortable>(OnItemTapped);

        // Apply initial ItemTemplate if set
        if (ItemTemplate != null && GridViewMode != null)
        {
            GridViewMode.ItemTemplate = CreateWrappedTemplate(ItemTemplate);
        }
    }

    public IEnumerable<ISortable>? ItemsSource
    {
        get => (IEnumerable<ISortable>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string SearchPlaceholder
    {
        get => (string)GetValue(SearchPlaceholderProperty);
        set => SetValue(SearchPlaceholderProperty, value);
    }

    public string EmptyMessage
    {
        get => (string)GetValue(EmptyMessageProperty);
        set => SetValue(EmptyMessageProperty, value);
    }

    public int GridSpanCount
    {
        get => (int)GetValue(GridSpanCountProperty);
        set => SetValue(GridSpanCountProperty, value);
    }

    public bool IsGridMode
    {
        get => (bool)GetValue(IsGridModeProperty);
        set => SetValue(IsGridModeProperty, value);
    }

    public Action<ISortable>? ItemAppearingAction
    {
        get => (Action<ISortable>?)GetValue(ItemAppearingActionProperty);
        set => SetValue(ItemAppearingActionProperty, value);
    }

    public Action<ISortable>? ItemDisappearingAction
    {
        get => (Action<ISortable>?)GetValue(ItemDisappearingActionProperty);
        set => SetValue(ItemDisappearingActionProperty, value);
    }

    public int RemainingItemsThreshold
    {
        get => (int)GetValue(RemainingItemsThresholdProperty);
        set => SetValue(RemainingItemsThresholdProperty, value);
    }

    public ICommand? RemainingItemsThresholdReachedCommand
    {
        get => (ICommand?)GetValue(RemainingItemsThresholdReachedCommandProperty);
        set => SetValue(RemainingItemsThresholdReachedCommandProperty, value);
    }
    public IEnumerable<GalleryContextCommand>? ItemContextCommands
    {
        get => (IEnumerable<GalleryContextCommand>?)GetValue(ItemContextCommandsProperty);
        set => SetValue(ItemContextCommandsProperty, value);
    }


    public ICommand ItemTappedCommand { get; }

    public event EventHandler<ItemSelectedEventArgs>? ItemSelected;

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ItemGalleryView view && newValue is IEnumerable<ISortable> items)
        {
            var count = items?.Count() ?? 0;
            System.Diagnostics.Debug.WriteLine($"[ItemGalleryView] ItemsSource changed. Count: {count}");
            view.ViewModel.SetItemsSource(items);
        }
    }

    private static void OnItemTemplateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ItemGalleryView view && newValue is DataTemplate template)
        {
            // Apply template to grid view mode
            if (view.GridViewMode != null)
            {
                view.GridViewMode.ItemTemplate = view.CreateWrappedTemplate(template);
            }
        }
    }
    private static void OnItemContextCommandsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ItemGalleryView view && view.GridViewMode != null && view.ItemTemplate != null)
        {
            // Re-create wrapped template to apply new context menu
            view.GridViewMode.ItemTemplate = view.CreateWrappedTemplate(view.ItemTemplate);
        }
    }


    private DataTemplate CreateWrappedTemplate(DataTemplate innerTemplate)
    {
        return new DataTemplate(() =>
        {
            System.Diagnostics.Debug.WriteLine($"[ItemGalleryView] CreateWrappedTemplate factory called");

            var innerContent = innerTemplate.CreateContent();
            var contentView = innerContent as View ?? (innerContent as ViewCell)?.View;

            if (contentView != null)
            {
                System.Diagnostics.Debug.WriteLine($"[ItemGalleryView] Creating wrapper for content: {contentView.GetType().Name}");

                var wrapper = new ContentView
                {
                    Content = contentView
                };

                // Add tap gesture
                var tapGesture = new TapGestureRecognizer
                {
                    Command = ItemTappedCommand
                };
                tapGesture.SetBinding(TapGestureRecognizer.CommandParameterProperty, new Binding("."));
                wrapper.GestureRecognizers.Add(tapGesture);

                // Add context menu if commands are provided
                if (ItemContextCommands != null && ItemContextCommands.Any())
                {
                    var menuFlyout = new MenuFlyout();

                    foreach (var cmd in ItemContextCommands)
                    {
                        var menuItem = new MenuFlyoutItem
                        {
                            Text = cmd.Text,
                            Command = cmd.Command
                        };

                        if (!string.IsNullOrEmpty(cmd.IconImageSource))
                        {
                            menuItem.IconImageSource = ImageSource.FromFile(cmd.IconImageSource);
                        }

                        if (cmd.IsDestructive)
                        {
                            menuItem.StyleClass = new[] { "DestructiveAction" };
                        }

                        menuItem.SetBinding(MenuFlyoutItem.CommandParameterProperty, new Binding("."));
                        menuFlyout.Add(menuItem);
                    }

                    FlyoutBase.SetContextFlyout(wrapper, menuFlyout);
                }

                // Add appearing/disappearing behavior
                var behavior = new ItemAppearingBehavior();
                behavior.SetBinding(ItemAppearingBehavior.AppearingActionProperty, new Binding(nameof(ItemAppearingAction), source: this));
                behavior.SetBinding(ItemAppearingBehavior.DisappearingActionProperty, new Binding(nameof(ItemDisappearingAction), source: this));
                wrapper.Behaviors.Add(behavior);

                return wrapper;
            }

            System.Diagnostics.Debug.WriteLine($"[ItemGalleryView] ERROR: Invalid template content");
            return new Label { Text = "Invalid template" };
        });
    }

    private void OnItemTapped(ISortable? item)
    {
        if (item != null)
        {
            ViewModel.SelectedItem = item;
        }
    }

    private void OnItemSelected(object? sender, ItemSelectedEventArgs e)
    {
        ItemSelected?.Invoke(this, e);
    }

    private void ItemsView_Scrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        ViewModel?.ItemsView_Scrolled(sender, e);
    }

    private void OnItemAppearing(object? sender, ItemVisibilityEventArgs e)
    {
        if (e.Item is ISortable item)
        {
            ItemAppearingAction?.Invoke(item);
        }
    }

    private void OnItemDisappearing(object? sender, ItemVisibilityEventArgs e)
    {
        if (e.Item is ISortable item)
        {
            ItemDisappearingAction?.Invoke(item);
        }
    }
}
