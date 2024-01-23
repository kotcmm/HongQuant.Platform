using HongQuant.Platform.ViewModels.Hong.Setting.ContractConfigViewModels;
using Splat;
using System.Windows.Controls;

namespace HongQuant.Platform.Views.Hong.Setting
{
    /// <summary>
    /// ContractConfig.xaml 的交互逻辑
    /// </summary>
    public partial class ContractConfig : UserControl
    {
        public ContractConfig()
        {
            InitializeComponent();

            DataContext = Locator.Current.GetService<ContractConfigViewModel>();
        }
    }
}
