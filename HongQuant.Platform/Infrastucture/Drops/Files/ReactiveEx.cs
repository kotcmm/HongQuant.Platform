using System.Reactive;
using System.Reactive.Linq;

namespace HongQuant.Platform.Infrastucture.Drops.Files
{
    public static class ReactiveEx
    {
        public static IObservable<Unit> ToUnit<T>(this IObservable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.Select(_ => Unit.Default);
        }
    }
}
