using HongQuant.Analysis.RealTime;

namespace HongQuant.Platform.ViewModels.RealTime.Items;
public class RealTimeDataViewModel
{
    public RealTimeDataViewModel(AnalyzedData data)
    {
        Product = data.Product;
        SignalDateTime = data.SignalDateTime;
        TradeDate = data.TradeDate;
        Position = data.Position;
        Equity = data.Equity;
        ProfitAndLoss = data.ProfitAndLoss;
        DayProfitAndLoss = data.DayProfitAndLoss;
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
    /// 交易日
    /// </summary>
    public DateOnly TradeDate { get; set; }

    /// <summary>
    /// 持仓
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// 权益
    /// </summary>
    public double Equity { get; set; }

    /// <summary>
    /// 净盈亏
    /// </summary>
    public double ProfitAndLoss { get; set; }

    /// <summary>
    /// 日净盈亏
    /// </summary>
    public double DayProfitAndLoss { get; set; }
    
}
