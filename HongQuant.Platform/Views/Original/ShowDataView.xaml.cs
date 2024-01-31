using HongQuant.Platform.ViewModels.Original;
using Splat;
using System.Windows.Controls;

namespace HongQuant.Platform.Views.Original;
/// <summary>
/// ShowDataView.xaml 的交互逻辑
/// </summary>
public partial class ShowDataView : UserControl
{
    public ShowDataView()
    {
        InitializeComponent();

        DataContext = Locator.Current.GetService<ShowDataViewModel>();
    }
}
