using DynamicData;
using HongQuant.Analysis.Original;
using HongQuant.Core.Models;
using HongQuant.Core.Stores;
using LiteDB;
using OfficeOpenXml;

namespace HongQuant.Core.Services;
public class OriginalService : IOriginalService
{
    private readonly SourceList<IDataFile> _fileIndexs = new();
    public IObservableList<IDataFile> FileIndexs => _fileIndexs.AsObservableList();

    private readonly SourceCache<IDataFile, string> _combinationFileIndexs = new(x=>x.Id);
    public IObservableCache<IDataFile, string> CombinationFileIndexs => _combinationFileIndexs.AsObservableCache();

    private readonly SourceCache<ProductTemplate, Guid> _productTemplates = new(x=>x.Id);
    public IObservableCache<ProductTemplate, Guid> ProductTemplates => _productTemplates.AsObservableCache();

    public OriginalService()
    {
        LoadTemplates();
    }

    public void Combination(List<IDataFile> basicDataFiles, string name = "")
    {
        var combination = new CombinationBasicDataFile(basicDataFiles);

        _combinationFileIndexs.AddOrUpdate(combination.Calculate(name));
    }

    public void LoadFile(IEnumerable<FileInfo> fileInfos)
    {
        _fileIndexs.Clear();

        _fileIndexs.AddRange(fileInfos
            .Select(fileInfo => new BasicDataFile(fileInfo))
            .Select(x=>x.Calculate()));
    }

    public void Remove(string id)
    {
        _combinationFileIndexs.RemoveKey(id);
    }

    public void LoadTemplates()
    {
        var templates = ProductTemplateStore.LoadTemplates();

        _productTemplates.AddOrUpdate(templates);
    }

    public void SaveProductTemplate(ProductTemplate productTemplate)
    {
        _productTemplates.AddOrUpdate(productTemplate);

        ProductTemplateStore.AddOrUpdate(productTemplate);
    }

    public void RemoveProductTemplate(Guid id)
    {
        _productTemplates.RemoveKey(id);

        ProductTemplateStore.Remove(id);
    }

    public void Save(string dir, IDataFile dataFile)
    {
        var fileName = Path.Combine(dir, $"{dataFile.Product}.xlsx");

        using var package = new ExcelPackage();

        var worksheet = package.Workbook.Worksheets.Add("Info");

        worksheet.Cells["A1"].Value = "品种";
        worksheet.Cells["B1"].Value = "交易日";
        worksheet.Cells["C1"].Value = "权益";
        worksheet.Cells["D1"].Value = "净利润";
        worksheet.Cells["E1"].Value = "收益率%";
        worksheet.Cells["F1"].Value = "累计收益率%";
        worksheet.Cells["G1"].Value = "实时回撤%";
        worksheet.Cells["H1"].Value = "最大回撤%";
        worksheet.Cells["I1"].Value = "回撤程度%";

        int row = 2;

        foreach (var item in dataFile.Items)
        {
            worksheet.Cells["A" + row].Value = item.Product;
            worksheet.Cells["B" + row].Value = item.TradeDate.ToString("yyyyMMdd");
            worksheet.Cells["C" + row].Value = item.Equity;
            worksheet.Cells["D" + row].Value = item.TodayProfitAndLoss;
            worksheet.Cells["E" + row].Value = item.Yields.ToString("P2");
            worksheet.Cells["F" + row].Value = item.TotalYields.ToString("P2");
            worksheet.Cells["G" + row].Value = item.WithdrawDeep.ToString("P2");
            worksheet.Cells["H" + row].Value = item.MaxWithdraw.ToString("P2");
            worksheet.Cells["I" + row].Value = item.WithdrawLevel.ToString("P2");

            row++;
        }

        var xlFile = new FileInfo(fileName);

        package.SaveAs(xlFile);
    }
}
