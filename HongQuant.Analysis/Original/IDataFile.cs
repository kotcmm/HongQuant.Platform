namespace HongQuant.Analysis.Original;
public interface IDataFile
{
    /// <summary>
    /// 计算原始数据后触发改变
    /// </summary>
    IObservable<IEnumerable<BasicTransformData>> CalculateChange { get; }

    /// <summary>
    /// 获取转换好的数据
    /// </summary>
    IEnumerable<BasicTransformData> Items { get; }

    string Id { get; }

    string Name { get;}

    IDataFile Calculate();
}
