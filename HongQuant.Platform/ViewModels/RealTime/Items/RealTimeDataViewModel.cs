using HongQuant.Analysis.RealTime;

namespace HongQuant.Platform.ViewModels.RealTime.Items;
public class RealTimeDataViewModel
{
    public RealTimeDataViewModel(AnalyzedData data)
    {
        Product = data.Product;
        SignalDateTime = data.SignalDateTime;
        Position = data.Position;
        Equity = data.Equity;
    }

    /// <summary>
    /// 品种
    /// </summary>
    public string Product { get; set; }

    /// <summary>
    /// 信号对应时间
    /// </summary>
    public DateTime SignalDateTime { get; set; }

    /// <summary>
    /// 持仓
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// 权益
    /// </summary>
    public double Equity { get; set; }
}
