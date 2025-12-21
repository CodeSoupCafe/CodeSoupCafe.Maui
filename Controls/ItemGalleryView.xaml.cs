namespace CodeSoupCafe.Maui.Controls;

using CodeSoupCafe.Maui.Models;
using CodeSoupCafe.Maui.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

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

    public ItemGalleryViewModel ViewModel { get; }

    public ItemGalleryView()
    {
        InitializeComponent();

        ViewModel = new ItemGalleryViewModel();
        BindingContext = ViewModel;

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

                var tapGesture = new TapGestureRecognizer
                {
                    Command = ItemTappedCommand
                };
                tapGesture.SetBinding(TapGestureRecognizer.CommandParameterProperty, new Binding("."));
                wrapper.GestureRecognizers.Add(tapGesture);

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
}
