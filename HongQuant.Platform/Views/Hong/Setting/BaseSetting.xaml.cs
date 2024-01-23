using HongQuant.Platform.ViewModels.Hong.Setting;
using Splat;
using System.Windows.Controls;

namespace HongQuant.Platform.Views.Hong.Setting
{
    /// <summary>
    /// BaseSetting.xaml 的交互逻辑
    /// </summary>
    public partial class BaseSetting : UserControl
    {
        public BaseSetting()
        {
            InitializeComponent();

            DataContext = Locator.Current.GetService<BaseSettingViewModel>();
        }
    }
}
