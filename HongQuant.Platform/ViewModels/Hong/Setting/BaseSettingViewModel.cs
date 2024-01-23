using HongQuant.Analysis.Hong.Domain;
using HongQuant.Core.Stores;
using ReactiveUI;
using System.Windows.Controls.Primitives;

namespace HongQuant.Platform.ViewModels.Hong.Setting;
public class BaseSettingViewModel : ReactiveObject, IDisposable
{
    private readonly IDisposable _cleanUp;
    public SignalDataConfigViewModel SignalDataConfig { get; } = new();

    public BaseSettingViewModel()
    {
        LoadConfig();

        _cleanUp = SignalDataConfig.Change.Subscribe(SaveConfig);
    }

    private void LoadConfig()
    {
        var config = CalculateConfigStore.GetDefault();

        if (config == null) return;

        SignalDataConfig.DayMaxLevel = config.DayMaxLevel;
        SignalDataConfig.WeekMaxLevel = config.WeekMaxLevel;
        SignalDataConfig.MonthMaxLevel = config.MonthMaxLevel;
        SignalDataConfig.Entrance = config.Entrance;
        SignalDataConfig.Quit = config.Quit;
        SignalDataConfig.Stop = config.Stop;
        SignalDataConfig.MaxCombo = config.MaxCombo;
        SignalDataConfig.SelectTop = config.SelectTop;
    }

    private static void SaveConfig(SignalDataConfigViewModel x)
    {
        CalculateConfigStore.SetDefault(new SignalDataConfig
        {
            DayMaxLevel = x.DayMaxLevel,
            WeekMaxLevel = x.WeekMaxLevel,
            MonthMaxLevel = x.MonthMaxLevel,
            Entrance = x.Entrance,
            Quit = x.Quit,
            Stop = x.Stop,
            MaxCombo = x.MaxCombo,
            SelectTop = x.SelectTop,
        });
    }

    public void Dispose()
    {
        _cleanUp.Dispose();
    }
}
