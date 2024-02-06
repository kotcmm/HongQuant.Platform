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

        var viewModel = Locator.Current.GetService<OriginalMainViewModel>();

        DataContext = viewModel;

        viewModel?.OpenProductTemplateSettingCommand.Subscribe(s =>
        {
            ShowProductTemplateSetting();
        });
    }

    private ProductTemplateSettingView? _hongSetting;

    private void ShowProductTemplateSetting()
    {
        if (_hongSetting == null)
        {
            _hongSetting = new ProductTemplateSettingView()
            {
                Owner = System.Windows.Application.Current.MainWindow,
            };

            _hongSetting.Closed += (s, e) =>
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
