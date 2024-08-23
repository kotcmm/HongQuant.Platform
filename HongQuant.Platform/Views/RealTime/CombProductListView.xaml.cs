using HongQuant.Platform.ViewModels.RealTime;
using System.Windows.Controls;

namespace HongQuant.Platform.Views.RealTime;
/// <summary>
/// CombProductListView.xaml 的交互逻辑
/// </summary>
public partial class CombProductListView : UserControl
{
    public CombProductListView()
    {
        InitializeComponent();

        DataContext = new CombProductListViewModel();
    }
}
