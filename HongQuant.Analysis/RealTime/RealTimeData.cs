using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace HongQuant.Analysis.RealTime;
public class RealTimeData
{
    /// <summary>
    /// 信号标记的时间
    /// </summary>
    public DateTime? SignalDateTime { get; set; }

    /// <summary>
    /// 权益
    /// </summary>
    public double? Equity { get; set; }

    /// <summary>
    /// 持仓
    /// </summary>
    public int? Position { get; set; }

    internal static RealTimeData? BuildData(string? line)
    {
        if (string.IsNullOrEmpty(line)) return null;

        var items = line.Split(',');

        if(items.Length < 3) return null;

        return new RealTimeData()
        {
            SignalDateTime = ConvertDateTime(items[0]),
            Equity = ConvertDouble(items[1]),
            Position = ConvertInt(items[2]),
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

    private static int? ConvertInt(string item)
    {
        return int.TryParse(item.Trim(), out var value) ? value : null;
    }

    [MemberNotNullWhen(true, nameof(Equity), nameof(SignalDateTime))]
    internal bool Verify()
    {
        return Equity.HasValue &&
            SignalDateTime.HasValue;
    }
}
