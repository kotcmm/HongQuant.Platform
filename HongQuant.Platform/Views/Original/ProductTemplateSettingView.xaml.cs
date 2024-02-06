using HongQuant.Platform.ViewModels.Original;
using Splat;

namespace HongQuant.Platform.Views.Original;
/// <summary>
/// ProductTemplateSettingView.xaml 的交互逻辑
/// </summary>
public partial class ProductTemplateSettingView
{
    public ProductTemplateSettingView()
    {
        InitializeComponent();

        DataContext = Locator.Current.GetService<ProductTemplateSettingViewModel>();
    }
}
