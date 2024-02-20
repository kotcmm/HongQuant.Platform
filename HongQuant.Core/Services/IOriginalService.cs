using DynamicData;
using HongQuant.Analysis.Original;
using HongQuant.Core.Models;
using LiteDB;

namespace HongQuant.Core.Services;
public interface IOriginalService
{
    IObservableList<IDataFile> FileIndexs { get; }

    IObservableCache<IDataFile, string> CombinationFileIndexs { get; }

    IObservableCache<ProductTemplate, Guid> ProductTemplates { get; }

    void Combination(List<IDataFile> basicDataFiles);

    void Remove(string id);

    void LoadFile(IEnumerable<FileInfo> fileInfos);

    void SaveProductTemplate(ProductTemplate productTemplate);

    void RemoveProductTemplate(Guid id);
}
