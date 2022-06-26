using System.Reactive;
using System.Reactive.Linq;

namespace PlexRipper.Domain.RxNet
{
    public static class RxNetExtentions
    {
        public static IDisposable SubscribeAsync<T>(this IObservable<T> source, Func<T, Task> asyncAction, Action<Exception> handler = null)
        {
            Func<T, Task<Unit>> wrapped = async t =>
            {
                await asyncAction(t);
                return Unit.Default;
            };
            if (handler == null)
                return source.SelectMany(wrapped).Subscribe(_ => { });

            return source.SelectMany(wrapped).Subscribe(_ => { }, handler);
        }
    }
}