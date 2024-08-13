using HongQuant.Platform.ViewModels.RealTime;
using Splat;
using System.Windows.Controls;

namespace HongQuant.Platform.Views.RealTime;
/// <summary>
/// ProductListView.xaml 的交互逻辑
/// </summary>
public partial class ProductListView : UserControl
{
    public ProductListView()
    {
        InitializeComponent();

        DataContext = Locator.Current.GetService<ProductListViewModel>();
    }
}
