using HongQuant.Platform.ViewModels.Hong.Shows;
using Splat;
using System.Windows.Controls;

namespace HongQuant.Platform.Views.Hong.Shows;
/// <summary>
/// SignalDataView.xaml 的交互逻辑
/// </summary>
public partial class SignalDataView : UserControl
{
    public SignalDataView()
    {
        InitializeComponent();

        DataContext = Locator.Current.GetService<SignalDataViewModel>();
    }
}
