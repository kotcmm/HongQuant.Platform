using HongQuant.Analysis.Hong.Domain;

namespace HongQuant.Analysis.Hong.Formula;
/// <summary>
/// 周期映射
/// </summary>
public class CycleGroup
{
    /// <summary>
    /// 大周期的日期
    /// </summary>
    public DateOnly KeyDateTime { get; set; }

    /// <summary>
    /// 映射的小周期
    /// </summary>
    public List<SignalData> Children { get; set; } = [];
}
