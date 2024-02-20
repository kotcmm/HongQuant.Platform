using HongQuant.Platform.ViewModels.Original;
using MahApps.Metro.Controls.Dialogs;
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

        Confirm();
    }

    private void Confirm()
    {
        ProductTemplateSettingViewModel.Confirm.RegisterHandler(
                       async interaction =>
                       {
                           var settings = new MetroDialogSettings(this.MetroDialogOptions)
                           {
                               AffirmativeButtonText = "确定",
                               NegativeButtonText = "取消",
                           };

                           MessageDialogResult result = await this.ShowMessageAsync("删除确认",
                               $"你确认删除[{interaction.Input}]?",
                               MessageDialogStyle.AffirmativeAndNegative,
                               settings);

                           interaction.SetOutput(result == MessageDialogResult.Affirmative);
                       });
    }
}
