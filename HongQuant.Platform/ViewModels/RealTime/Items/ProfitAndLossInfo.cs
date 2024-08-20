using LiveChartsCore.Defaults;

namespace HongQuant.Platform.ViewModels.RealTime.Items;
public class ProfitAndLossInfo : ObservableValue
{
    /// <summary>
    /// 品种
    /// </summary>
    public string Name { get; set; }

    public ProfitAndLossInfo(string name, double? value)
    {
        Name = name;
        Value = value;
    }
}
