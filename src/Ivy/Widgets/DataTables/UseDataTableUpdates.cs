using System.Reactive.Linq;

// ReSharper disable once CheckNamespace
namespace Ivy;

public static class UseDataTableUpdatesExtensions
{
    public static IWriteStream<DataTableCellUpdate> UseDataTableUpdates(
        this IViewContext context,
        params IObservable<DataTableCellUpdate>[] sources)
    {
        var stream = context.UseStream<DataTableCellUpdate>();
        context.UseEffect(() =>
        {
            var merged = Observable.Merge(sources);
            var subscription = merged.Subscribe(data => stream.Write(data));
            return subscription;
        }, EffectTrigger.OnMount());
        return stream;
    }
}
