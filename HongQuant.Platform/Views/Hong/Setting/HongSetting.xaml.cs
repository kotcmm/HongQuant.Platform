using HongQuant.Platform.Infrastucture;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

namespace HongQuant.Platform.Views.Hong.Setting
{
    /// <summary>
    /// HongSetting.xaml 的交互逻辑
    /// </summary>
    public partial class HongSetting
    {
        public HongSetting()
        {
            InitializeComponent();

            HongSettingConfirm();
            HongSettingError();
            HongSettingSaveFileDialog();
            HongSettingOpenFileDialog();
        }

        private void HongSettingOpenFileDialog()
        {
            Interactions.HongSettingOpenFileDialog.RegisterHandler(
                           interaction =>
                           {
                               var dialog = new OpenFileDialog 
                               { 
                                   Filter = "Json files (*.json)|*.json"
                               };
                               var result = dialog.ShowDialog();

                               interaction.SetOutput(result == true ? dialog.FileName : string.Empty);
                           });
        }

        private void HongSettingSaveFileDialog()
        {
            Interactions.HongSettingSaveFileDialog.RegisterHandler(
                           interaction =>
                           {
                               var dialog = new SaveFileDialog 
                               { 
                                   Filter = "Json files (*.json)|*.json",
                                   FileName = interaction.Input
                               };
                               var result = dialog.ShowDialog();

                               interaction.SetOutput(result == true ? dialog.FileName : string.Empty);
                           });
        }

        private void HongSettingError()
        {
            Interactions.HongSettingError.RegisterHandler(
                           async interaction =>
                           {

                               MessageDialogResult result = await this.ShowMessageAsync("错误提示", interaction.Input);

                               interaction.SetOutput(false);
                           });
        }

        private void HongSettingConfirm()
        {
            Interactions.HongSettingConfirm.RegisterHandler(
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
}
