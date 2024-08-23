using HongQuant.Analysis.RealTime;

namespace HongQuant.Platform.ViewModels.RealTime.Items;
public class CombRealTimeDataViewModel : RealTimeDataViewModel
{
    public CombRealTimeDataViewModel(CombAnalyzedData data) : base(data)
    {
        LongPosition = data.LongPosition;
        ShortPosition = data.ShortPosition;
    }

    /// <summary>
    /// 多头持仓
    /// </summary>
    public int LongPosition { get; set; }

    /// <summary>
    /// 空头持仓
    /// </summary>
    public int ShortPosition { get; set; }

}
