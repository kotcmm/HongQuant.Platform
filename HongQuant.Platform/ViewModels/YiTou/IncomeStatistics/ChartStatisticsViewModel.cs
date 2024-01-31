using LiveChartsCore.Defaults;

namespace HongQuant.Platform.ViewModels.YiTou.IncomeStatistics;
public class ChartStatisticsViewModel
{
    public ObservablePoint TotalValue { get; set; } = null!;

    public ObservablePoint MaxTotalValue { get; set; } = null!;

    public ObservablePoint MaxRetracementValue { get; set; } = null!;

    public ObservablePoint DayCountValue { get; set; } = null!;

    public ObservablePoint LastMultiple { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
}
