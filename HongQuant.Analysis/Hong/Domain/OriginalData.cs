
using System.Globalization;

namespace HongQuant.Analysis.Hong.Domain;

internal class OriginalDataIndex
{
    /// <summary>
    /// 合约名称
    /// </summary>
    public int NameIndex { get; private set; }

    /// <summary>
    /// 信号对应时间
    /// </summary>
    public int SignalDateTimeIndex { get; private set; }

    /// <summary>
    /// 权益
    /// </summary>
    public int EquityIndex { get; private set; }

    /// <summary>
    /// 多空状态
    /// </summary>
    public int SignalChangeIndex { get; set; }

    internal static OriginalDataIndex BuildData(string line)
    {
        if (string.IsNullOrEmpty(line)) throw new ArgumentNullException("第一行没有数据");

        var items = line.Split(',').ToList();

        return new OriginalDataIndex()
        {
            NameIndex = items.IndexOf("合约"),
            SignalDateTimeIndex = items.IndexOf("时间"),
            EquityIndex = items.IndexOf("权益"),
            SignalChangeIndex = items.IndexOf("信号"),
        };
    }
}

public enum SignalChange
{
    /// <summary>
    /// 未知状态
    /// </summary>
    Null,
    /// <summary>
    /// 要忽略的数据
    /// </summary>
    Ignore,
    /// <summary>
    /// 转换空仓
    /// </summary>
    None,
    /// <summary>
    /// 转换多头
    /// </summary>
    Buy,
    /// <summary>
    /// 转换空头
    /// </summary>
    Sell
}

/// <summary>
/// 原始数据
/// </summary>
public class OriginalData
{
    /// <summary>
    /// 合约名称
    /// </summary>
    public string? Name { get; private set; }

    /// <summary>
    /// 信号对应时间
    /// </summary>
    public DateTime? SignalDateTime { get; private set; }

    /// <summary>
    /// 权益
    /// </summary>
    public double? Equity { get; private set; }

    /// <summary>
    /// 开平仓转换
    /// </summary>
    public SignalChange SignalChange { get; set; }

    /// <summary>
    /// 数据是否有错误
    /// </summary>
    /// <returns></returns>
    public bool Verify()
    {
        return Equity.HasValue &&
            SignalDateTime.HasValue &&
            SignalChange != SignalChange.Ignore &&
            !string.IsNullOrEmpty(Name);
    }

    internal static OriginalData? BuildData(string? line, OriginalDataIndex index)
    {
        if (string.IsNullOrEmpty(line)) return null;

        var items = line.Split(',');

        return new OriginalData()
        {
            Name = items[index.NameIndex],
            SignalDateTime = ConvertDateTime(items[index.SignalDateTimeIndex]),
            Equity = ConvertDouble(items[index.EquityIndex]),
            SignalChange = ConvertSignalChange(items[index.SignalChangeIndex])
        };
    }

    private static readonly string[] formats = ["yyyyMMdd HH:mm:ss", "yyyy/MM/dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss"];

    private static DateTime? ConvertDateTime(string item)
    {
        return DateTime.TryParseExact(item.Trim(),
                formats,
                new DateTimeFormatInfo(),
                DateTimeStyles.None, out var dateTime) ? dateTime : null;
    }

    private static double? ConvertDouble(string item)
    {
        return double.TryParse(item.Trim(), out var value) ? value : null;
    }

    private static SignalChange ConvertSignalChange(string item)
    {
        return item.Trim() switch
        {
            "BK" => SignalChange.Buy,
            "SK" => SignalChange.Sell,
            "BP" or "SP" => SignalChange.None,
            "移仓" => SignalChange.Ignore,
            _ => SignalChange.Null,
        };
    }
}
