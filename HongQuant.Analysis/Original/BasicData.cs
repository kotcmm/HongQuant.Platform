using System.Globalization;

namespace HongQuant.Analysis.Original;

/// <summary>
/// 原始数据
/// </summary>
public class BasicData
{
    /// <summary>
    /// 信号标记的时间
    /// </summary>
    public DateTime? SignalDateTime { get; set; }

    /// <summary>
    /// 权益
    /// </summary>
    public double? Equity { get; set; }

    internal static BasicData? BuildData(string? line)
    {
        if (string.IsNullOrEmpty(line)) return null;

        var items = line.Split(',');

        return new BasicData()
        {
            SignalDateTime = ConvertDateTime(items[0]),
            Equity = ConvertDouble(items[1])
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

    internal bool Verify()
    {
        return Equity.HasValue &&
            SignalDateTime.HasValue;
    }
}
