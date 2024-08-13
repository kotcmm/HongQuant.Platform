using HongQuant.Platform.ViewModels.RealTime;
using Splat;
using System.Windows.Controls;

namespace HongQuant.Platform.Views.RealTime;
/// <summary>
/// RealTimeMain.xaml 的交互逻辑
/// </summary>
public partial class RealTimeMain : UserControl
{
    public RealTimeMain()
    {
        InitializeComponent();

        DataContext = Locator.Current.GetService<RealTimeMainViewModel>();
    }
}
