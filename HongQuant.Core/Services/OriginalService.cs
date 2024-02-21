using DynamicData;
using HongQuant.Analysis.Original;
using HongQuant.Core.Models;
using HongQuant.Core.Stores;
using LiteDB;

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
}
