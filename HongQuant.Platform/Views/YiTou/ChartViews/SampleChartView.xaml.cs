using HongQuant.Platform.ViewModels.YiTou.ChartViews;
using Splat;
using System.Windows.Controls;

namespace HongQuant.Platform.Views.YiTou.ChartViews;
/// <summary>
/// SampleChartView.xaml 的交互逻辑
/// </summary>
public partial class SampleChartView : UserControl
{
    public SampleChartView()
    {
        InitializeComponent();

        DataContext = Locator.Current.GetService<SampleChartViewModel>()!;
    }
}
