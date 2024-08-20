using System.Globalization;

namespace HongQuant.Analysis.RealTime;
public class AnalyzedData
{
    /// <summary>
    /// 品种
    /// </summary>
    public string Product { get; set; } = string.Empty;

    /// <summary>
    /// 信号对应时间
    /// </summary>
    public DateTime SignalDateTime { get; set; }

    /// <summary>
    /// 交易日
    /// </summary>
    public DateOnly TradeDate { get; set; }

    /// <summary>
    /// 第几周
    /// </summary>
    public int WeekOfYear { get; set; }

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
    /// 日盈亏
    /// </summary>
    public double DayProfitAndLoss { get; set; }

    /// <summary>
    /// 周盈亏
    /// </summary>
    public double WeekProfitAndLoss { get; set; }

    /// <summary>
    /// 月盈亏
    /// </summary>
    public double MonthProfitAndLoss { get; set; }

    /// <summary>
    /// 获取当年
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static int GetWeekNumOfTheYear(DateTime date)
    {
        CultureInfo cultureInfo = new CultureInfo("zh-CN");
        Calendar calendar = cultureInfo.Calendar;
        int weekOfYear = calendar.GetWeekOfYear(date, 
            cultureInfo.DateTimeFormat.CalendarWeekRule, 
            cultureInfo.DateTimeFormat.FirstDayOfWeek);
        return weekOfYear;
    }

    /// <summary>
    /// 计算净盈亏
    /// 净盈亏=当前一条权益-上一条权益
    /// </summary>
    /// <param name="firstData"></param>
    /// <param name="preData"></param>
    public void AttachProfitAndLoss(AnalyzedData firstData, AnalyzedData? preData)
    {
        if (preData == null)
        {
            ProfitAndLoss = Equity - firstData.Equity;
            DayProfitAndLoss = ProfitAndLoss;
            WeekProfitAndLoss = ProfitAndLoss;
        }
        else
        {
            ProfitAndLoss = Math.Round(Equity - preData.Equity, 2);
            DayProfitAndLoss = TradeDate == preData.TradeDate ?
                (ProfitAndLoss + preData.DayProfitAndLoss) : ProfitAndLoss;
            WeekProfitAndLoss = WeekOfYear == preData.WeekOfYear ?
                (ProfitAndLoss + preData.WeekProfitAndLoss) : ProfitAndLoss;
            MonthProfitAndLoss = TradeDate.Month == preData.TradeDate.Month ?
                (ProfitAndLoss + preData.MonthProfitAndLoss) : ProfitAndLoss;
        }
    }

    /// <summary>
    /// 计算交易日
    /// </summary>
    /// <param name="tradeDate"></param>
    public void AttachTradeDayAnWeekOfYear(ConvertTradeDate? tradeDate)
    {
        if (tradeDate != null)
        {
            TradeDate = tradeDate.GetTradeDate(SignalDateTime);
            WeekOfYear = GetWeekNumOfTheYear(TradeDate.ToDateTime(new TimeOnly()));
        }
    }
}
