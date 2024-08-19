using HongQuant.Platform.ViewModels.RealTime;
using System.Windows.Controls;

namespace HongQuant.Platform.Views.RealTime;
/// <summary>
/// ProfitAndLossView.xaml 的交互逻辑
/// </summary>
public partial class ProfitAndLossView : UserControl
{
    public ProfitAndLossView()
    {
        InitializeComponent();

        DataContext = new ProfitAndLossViewModel();
    }
}
