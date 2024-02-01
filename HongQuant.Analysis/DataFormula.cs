namespace HongQuant.Analysis;
public static class DataFormula
{
    // <summary>
    /// 自己定义的累计通用逻辑
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="action"></param>
    /// <param name="firstFunc"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IEnumerable<T> MyAggregate<T>(this IEnumerable<T> source, Action<T, T, T> action, Func<T, T>? firstFunc = null)
    {
        using var e = source.GetEnumerator();

        if (!e.MoveNext()) throw new InvalidOperationException("没有数据");

        var first = firstFunc != null ? firstFunc(e.Current) : e.Current;

        yield return first;

        var preData = e.Current;

        while (e.MoveNext())
        {
            var data = e.Current;

            action(first, preData, data);

            preData = e.Current;

            yield return data;
        }
    }
}
