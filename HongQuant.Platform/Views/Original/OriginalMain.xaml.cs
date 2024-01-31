using HongQuant.Platform.ViewModels.Original;
using Splat;
using System.Windows.Controls;

namespace HongQuant.Platform.Views.Original;
/// <summary>
/// OriginalMain.xaml 的交互逻辑
/// </summary>
public partial class OriginalMain : UserControl
{
    public OriginalMain()
    {
        InitializeComponent();

        DataContext = Locator.Current.GetService<OriginalMainViewModel>();
    }
}
