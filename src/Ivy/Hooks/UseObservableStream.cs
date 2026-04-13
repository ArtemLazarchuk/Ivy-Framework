using System.Reactive.Linq;

// ReSharper disable once CheckNamespace
namespace Ivy;

public static class UseObservableStreamExtensions
{
    public static void UseObservableStream<T>(this IViewContext context, IWriteStream<T> stream, params IObservable<T>[] sources)
    {
        context.UseEffect(() =>
        {
            var merged = Observable.Merge(sources);
            var subscription = merged.Subscribe(data => stream.Write(data));
            return subscription;
        }, EffectTrigger.OnMount());
    }

    public static IWriteStream<DataTableCellUpdate> UseDataTableUpdates(
        this IViewContext context,
        params IObservable<DataTableCellUpdate>[] sources)
    {
        var stream = context.UseStream<DataTableCellUpdate>();
        context.UseObservableStream(stream, sources);
        return stream;
    }
}
