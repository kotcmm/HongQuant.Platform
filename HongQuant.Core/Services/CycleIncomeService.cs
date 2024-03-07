using DynamicData;
using HongQuant.Analysis.YiTou;
using HongQuant.Analysis.YiTou.Configs;
using HongQuant.Analysis.YiTou.IncomeStatistics;
using OfficeOpenXml;
using System.Globalization;

namespace HongQuant.Core.Services;
public class CycleIncomeService : ICycleIncomeService
{
    private readonly ISourceList<BaseIncomeStatistics> _source;
    public IObservableList<BaseIncomeStatistics> IncomeStatistics => _source;

    private readonly ISourceList<StatisticsResult> _finishSource;
    public IObservableList<StatisticsResult> FinishStatistics => _finishSource;

    public CycleIncomeService()
    {
        _source = new SourceList<BaseIncomeStatistics>();
        _finishSource = new SourceList<StatisticsResult>();
    }

    public void Load(FileInfo fileInfo, IConfigIncome? config, AlgorithmType algorithmType)
    {
        StatisticsData(config, algorithmType, ReadDatas(fileInfo));
    }

    public void StatisticsData(IConfigIncome? config, AlgorithmType algorithmType, IEnumerable<IncomeInfoModel> source)
    {
        _source.Clear();

        var datas = source
            .ToIncomeStatistics(algorithmType, config);

        _source.AddRange(datas);

        _finishSource.Clear();

        var statisticsResults = _source.Items.Select((value, i) => new StatisticsResult
        {
            Index = i,
            Result = value
        });

        _finishSource.AddRange(statisticsResults);
    }

    public static IEnumerable<IncomeInfoModel> ReadDatas(FileInfo fi, int workbook = 0)
    {
        using var p = new ExcelPackage(fi);

        var worksheet = p.Workbook.Worksheets[workbook];

        var list = new List<IncomeInfoModel>(worksheet.Dimension.Rows - 1);

        for (int row = 2; row <= worksheet.Dimension.Rows; row++)
        {
            if(DateTime.TryParseExact(worksheet.Cells["A" + row].Text, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                var preNetWorth = row == 2 ? 1 : (double)worksheet.Cells["C" + (row - 1)].Value;
                var netWorth = (double)worksheet.Cells["C" + row].Value;

                list.Add(new IncomeInfoModel(
                    date,
                    netWorth,
                    netWorth - preNetWorth
                ));
            } 
        }

        return list;
    }

    public void Save(string path, IEnumerable<BaseIncomeStatistics> incomes)
    {
        using var package = new ExcelPackage();

        var worksheet = package.Workbook.Worksheets.Add("Info");

        worksheet.Cells["A1"].Value = "开始日期";
        worksheet.Cells["B1"].Value = "结束日期";
        worksheet.Cells["C1"].Value = "累计收益%";
        worksheet.Cells["D1"].Value = "最大回撤%";
        worksheet.Cells["E1"].Value = "回撤日期";
        worksheet.Cells["F1"].Value = "最大收益%";
        worksheet.Cells["G1"].Value = "收益日期";
        worksheet.Cells["H1"].Value = "累计天数";
        worksheet.Cells["I1"].Value = "累计杠杆";

        int row = 2;

        foreach (var item in incomes)
        {
            worksheet.Cells["A" + row].Value = item.Start.ToString("yyyyMMdd");
            worksheet.Cells["B" + row].Value = item.End.ToString("yyyyMMdd");
            worksheet.Cells["C" + row].Value = item.Total;
            worksheet.Cells["D" + row].Value = item.MaxRetracement;
            worksheet.Cells["E" + row].Value = item.RetracementDate.ToString("yyyyMMdd");
            worksheet.Cells["F" + row].Value = item.MaxTotal;
            worksheet.Cells["G" + row].Value = item.MaxTotalDate.ToString("yyyyMMdd");
            worksheet.Cells["H" + row].Value = item.DayCount;
            worksheet.Cells["I" + row].Value = item.LastMultiples;

            row++;
        }

        var xlFile = new FileInfo(path);

        package.SaveAs(xlFile);
    }
}
