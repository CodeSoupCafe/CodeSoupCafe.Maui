namespace CodeSoupCafe.Maui.Infrastructure;

using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.Messaging;

public enum AppMessageStateType
{
    ThreadMonitorState = 1 << 0,
    ImageLoadingState = 1 << 1,
    ParallaxScrollState = 1 << 2
}

public class GlobalBroadcaster
{
    public static IDisposable Broadcast<TSender>(
        TSender sender,
        AppMessageStateType messageCenterType = AppMessageStateType.ThreadMonitorState,
        TimeSpan? delay = null) where TSender : class
    {
        return Observable.Create<Unit>(x =>
        {
            x.OnNext(Unit.Default);
            return Disposable.Create(() => { });
        })
        .Throttle(delay ?? TimeSpan.FromMilliseconds(189))
        .Subscribe(x =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    WeakReferenceMessenger.Default.Send(sender, messageCenterType.ToString());
                }
                catch //(Exception ex)
                {
                    // Errors can be logged by consumers if needed
                }
            });
        });
    }

    public static IDisposable Subscribe<T>(
        object subscriber,
        AppMessageStateType messageCenterType = AppMessageStateType.ThreadMonitorState,
        MessageHandler<object, T>? callBackAction = null)
        where T : class
    {
        WeakReferenceMessenger.Default.Register<T, string>(subscriber,
            messageCenterType.ToString(),
            callBackAction!);

        return Disposable.Create(() => WeakReferenceMessenger.Default.Unregister<T, string>(subscriber, messageCenterType.ToString()));
    }
}

public class GlobalMessage
{
    public required dynamic Arguments { get; set; }
    public AppMessageStateType MessageCenterType { get; set; }
    public required dynamic Subscriber { get; set; }
}

public enum ImageLoadingType
{
    NotLoading = 0,
    IsLoading = 1,
    ForceRedraw = 2
}

public class ImageLoadingState
{
    public ImageLoadingState(ImageLoadingType state)
    {
        LoadingState = state;
    }

    public ImageLoadingType LoadingState { get; set; }
    public AppMessageStateType Type { get; set; }
}
