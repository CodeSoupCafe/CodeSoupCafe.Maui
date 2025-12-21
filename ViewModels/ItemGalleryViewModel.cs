namespace CodeSoupCafe.Maui.ViewModels;

using CommunityToolkit.Mvvm.Input;
using CodeSoupCafe.Maui.Infrastructure;
using CodeSoupCafe.Maui.Models;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Windows.Input;

public class ItemGalleryViewModel : NotifyPropertyChanged, IDisposable
{
    private bool isGridMode = true;
    private bool isSortVisible;
    private bool isSearchVisible;
    private string searchText = string.Empty;
    private bool isLoading;
    private bool isRefreshing;
    private bool isEmptyItems;
    private SortProperty sortProperty = SortProperty.DateUpdated;
    private SortOrder sortOrder = SortOrder.Descending;
    private object? selectedItem;

    private List<ISortable> allItems = new();
    private IDisposable? refreshFromScroll;
    private IDisposable? refreshImagesFromScroll;
    private double lastScrollValue;
    private readonly int scrollAllowUpdateRange = 120;

    public RangedObservableCollection<ISortable> Items { get; } = new();

    public List<string> SortOrders { get; } = new() { "Ascending", "Descending" };
    public List<string> SortProperties { get; } = new() { "DateCreated", "DateUpdated", "Title" };

    public ItemGalleryViewModel()
    {
        ToggleViewCommand = new RelayCommand(ToggleView);
        ShowHideSortPanelCommand = new RelayCommand(ShowHideSortPanel);
        ShowHideSearchPanelCommand = new RelayCommand(ShowHideSearchPanel);
        ClearSearchPanelCommand = new RelayCommand(ClearSearchPanel);
        RefreshCommand = new RelayCommand(Refresh);
    }

    public bool IsGridMode
    {
        get => isGridMode;
        set => SetProperty(ref isGridMode, value);
    }

    public bool IsSortVisible
    {
        get => isSortVisible;
        set
        {
            if (SetProperty(ref isSortVisible, value) && value)
            {
                IsSearchVisible = false;
            }
        }
    }

    public bool IsSearchVisible
    {
        get => isSearchVisible;
        set
        {
            if (SetProperty(ref isSearchVisible, value) && value)
            {
                IsSortVisible = false;
            }
        }
    }

    public string SearchText
    {
        get => searchText;
        set
        {
            if (SetProperty(ref searchText, value))
            {
                FilterItems();
            }
        }
    }

    public bool IsLoading
    {
        get => isLoading;
        set => SetProperty(ref isLoading, value);
    }

    public bool IsRefreshing
    {
        get => isRefreshing;
        set => SetProperty(ref isRefreshing, value);
    }

    public bool IsEmptyItems
    {
        get => isEmptyItems;
        set => SetProperty(ref isEmptyItems, value);
    }

    public SortProperty SortPropertyValue
    {
        get => sortProperty;
        set
        {
            if (SetProperty(ref sortProperty, value))
            {
                SortItems();
            }
        }
    }

    public SortOrder SortOrderValue
    {
        get => sortOrder;
        set
        {
            if (SetProperty(ref sortOrder, value))
            {
                SortItems();
            }
        }
    }

    public string SortPropertyString
    {
        get => sortProperty.ToString();
        set
        {
            if (Enum.TryParse<SortProperty>(value, out var parsed))
            {
                SortPropertyValue = parsed;
            }
        }
    }

    public string SortOrderString
    {
        get => sortOrder.ToString();
        set
        {
            if (Enum.TryParse<SortOrder>(value, out var parsed))
            {
                SortOrderValue = parsed;
            }
        }
    }

    public object? SelectedItem
    {
        get => selectedItem;
        set
        {
            if (SetProperty(ref selectedItem, value) && value is ISortable item)
            {
                OnItemSelected(item);
            }
        }
    }

    public ICommand ToggleViewCommand { get; }
    public ICommand ShowHideSortPanelCommand { get; }
    public ICommand ShowHideSearchPanelCommand { get; }
    public ICommand ClearSearchPanelCommand { get; }
    public ICommand RefreshCommand { get; }

    public event EventHandler<ItemSelectedEventArgs>? ItemSelected;

    public void SetItemsSource(IEnumerable<ISortable> items)
    {
        allItems = items.ToList();
        FilterItems();
    }

    private void ToggleView()
    {
        IsGridMode = !IsGridMode;
    }

    private void ShowHideSortPanel()
    {
        IsSortVisible = !IsSortVisible;
    }

    private void ShowHideSearchPanel()
    {
        IsSearchVisible = !IsSearchVisible;
    }

    private void ClearSearchPanel()
    {
        SearchText = string.Empty;
        IsSearchVisible = false;
    }

    private void Refresh()
    {
        FilterItems();
    }

    private void OnItemSelected(ISortable item)
    {
        ItemSelected?.Invoke(this, new ItemSelectedEventArgs(item));
    }

    private void FilterItems()
    {
        var filtered = allItems;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(c => c.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        SortItems(filtered);
    }

    private void SortItems(List<ISortable>? inputList = null)
    {
        var list = inputList ?? new List<ISortable>(Items);

        Func<ISortable, object> sortKey = SortPropertyValue switch
        {
            SortProperty.Title => x => x.Title,
            SortProperty.DateCreated => x => x.DateCreated,
            _ => x => x.DateUpdated
        };

        var sorted = SortOrderValue == SortOrder.Descending
            ? list.OrderByDescending(sortKey).ToList()
            : list.OrderBy(sortKey).ToList();

        Items.Clear();
        Items.AddRange(sorted);

        IsEmptyItems = Items.Count == 0;
    }

    public void ItemsView_Scrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        refreshImagesFromScroll?.Dispose();
        refreshImagesFromScroll = null;

        var tempAction = new Action<Unit>((x) =>
        {
            if (lastScrollValue == e.VerticalDelta)
                _ = GlobalBroadcaster.Broadcast(new ImageLoadingState(ImageLoadingType.ForceRedraw), AppMessageStateType.ImageLoadingState);
        });
        refreshImagesFromScroll = Infrastructure.ReactiveExtensions.Debounce(tempAction, TimeSpan.FromMilliseconds(440));

        lastScrollValue = e.VerticalDelta;

        if (e.VerticalDelta < -scrollAllowUpdateRange ||
            e.VerticalDelta > scrollAllowUpdateRange)
        {
            refreshFromScroll?.Dispose();
            refreshFromScroll = null;

            var tempAction2 = new Action<Unit>((x) =>
            {
                if (refreshFromScroll == null)
                    return;
            });
            refreshFromScroll = Infrastructure.ReactiveExtensions.Debounce(tempAction2, TimeSpan.FromMilliseconds(220));
        }
    }

    public void Dispose()
    {
        refreshFromScroll?.Dispose();
        refreshImagesFromScroll?.Dispose();
    }
}

public class ItemSelectedEventArgs : EventArgs
{
    public ItemSelectedEventArgs(ISortable item)
    {
        Item = item;
    }

    public ISortable Item { get; }
}
