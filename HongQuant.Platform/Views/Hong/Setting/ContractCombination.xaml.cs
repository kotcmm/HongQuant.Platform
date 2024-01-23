using HongQuant.Platform.ViewModels.Hong.Setting.ContractCombinationViewModels;
using Splat;
using System.Windows.Controls;

namespace HongQuant.Platform.Views.Hong.Setting
{
    /// <summary>
    /// ContractCombination.xaml 的交互逻辑
    /// </summary>
    public partial class ContractCombination : UserControl
    {
        public ContractCombination()
        {
            InitializeComponent();

            DataContext = Locator.Current.GetService<ContractCombinationViewModel>();
        }
    }
}
