using HongQuant.Platform.ViewModels.Hong;
using HongQuant.Platform.Views.Hong.Setting;
using Splat;
using System.Windows.Controls;

namespace HongQuant.Platform.Views.Hong
{
    /// <summary>
    /// HongMain.xaml 的交互逻辑
    /// </summary>
    public partial class HongMain : UserControl
    {
        public HongMain()
        {
            InitializeComponent();

            var viewModel = Locator.Current.GetService<HongMainViewModel>();

            DataContext = viewModel;

            viewModel?.OpenSettingCommand.Subscribe(s =>
            {
                ShowHongSetting();
            });
        }

        private HongSetting? _hongSetting;

        private void ShowHongSetting()
        {
            if(_hongSetting == null)
            {
                _hongSetting = new HongSetting()
                {
                    Owner = System.Windows.Application.Current.MainWindow,
                };

                _hongSetting.Closed += (s,e)=> 
                {
                    _hongSetting = null;
                };

                _hongSetting.Show();
            }
            else
            {
                _hongSetting.Focus();
            }
        }
    }
}
