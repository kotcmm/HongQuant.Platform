using DynamicData;
using HongQuant.Analysis.YiTou;
using HongQuant.Analysis.YiTou.Configs;
using HongQuant.Analysis.YiTou.IncomeStatistics;

namespace HongQuant.Core.Services;
public interface ICycleIncomeService
{
    IObservableList<BaseIncomeStatistics> IncomeStatistics {  get; }

    IObservableList<StatisticsResult> FinishStatistics { get; }

    void Load(FileInfo fileInfo, IConfigIncome? config, AlgorithmType algorithmType);

    void Save(string path, IEnumerable<BaseIncomeStatistics> incomes);
}
