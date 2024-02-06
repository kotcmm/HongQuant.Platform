using DynamicData;
using HongQuant.Analysis.Original;
using HongQuant.Core.Models;

namespace HongQuant.Core.Services;
public class OriginalService : IOriginalService
{
    private readonly SourceList<IDataFile> _fileIndexs = new();
    public IObservableList<IDataFile> FileIndexs => _fileIndexs.AsObservableList();

    private readonly SourceCache<IDataFile, string> _combinationFileIndexs = new(x=>x.Id);
    public IObservableCache<IDataFile, string> CombinationFileIndexs => _combinationFileIndexs.AsObservableCache();

    private readonly SourceList<ProductTemplate> _productTemplates = new();
    public IObservableList<ProductTemplate> ProductTemplates => _productTemplates.AsObservableList();

    public void Combination(List<IDataFile> basicDataFiles)
    {
        var combination = new CombinationBasicDataFile(basicDataFiles);

        _combinationFileIndexs.AddOrUpdate(combination.Calculate());
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

    public void SaveProductTemplate(ProductTemplate productTemplate)
    {
        _productTemplates.Add(productTemplate);
    }
}
