using HongQuant.Analysis.Hong.Domain;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace HongQuant.Platform.ViewModels.Hong.Shows;
public class SignalFileIndexViewModel : ReactiveObject
{
    private readonly SignalFileIndex _fileIndex;

    public string Path => _fileIndex.FileInfo.Name;

    [Reactive]
    public IEnumerable<SignalDataItemViewModel>? Items { get; set; }

    public SignalFileIndexViewModel(SignalFileIndex fileIndex)
    {
        _fileIndex = fileIndex;

        fileIndex.CalculateChange.Subscribe(items =>
        {
            Items = items.Select(item=>new SignalDataItemViewModel()
            {
                Name = item.Name,
                SignalDateTime = item.SignalDateTime,
                TradeDate = item.TradeDate,
                Equity = item.Equity,
                Yields = item.Yields,
                TotalYields = item.TotalYields,
                WithdrawDeep = item.WithdrawDeep,
                MaxWithdraw = item.MaxWithdraw,
                WithdrawLevel = item.WithdrawLevel,
                PotentialRisk = item.PotentialRisk,
                DayLevel = item.DayLevel,
                WeekLevel = item.WeekLevel,
                MonthLevel = item.MonthLevel,
                EntranceState = item.EntranceState,
                AccurateTotalYields = item.AccurateTotalYields,
                Direction = item.Direction,
                LossEquity = item.LossEquity,
                LossCount = item.LossCount,
                MaxLossCount = item.MaxLossCount,
                LostDegree = item.LostDegree,
                PreProfitCycle = item.PreProfitCycle,
                MaxProfitCycle = item.MaxProfitCycle,
                ProfitCycleDegree = item.ProfitCycleDegree,
                EmptyCycle = item.EmptyCycle,
                MaxEmptyCycle = item.MaxEmptyCycle,
                EmptyDegree = item.EmptyDegree,
                PositionCycle = item.PositionCycle,
                MaxPositionCycle = item.MaxPositionCycle,
                PositionDegree = item.PositionDegree
            });
        });
    }
}
