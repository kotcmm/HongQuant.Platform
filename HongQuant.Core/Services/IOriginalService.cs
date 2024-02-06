using DynamicData;
using HongQuant.Analysis.Original;
using HongQuant.Core.Models;

namespace HongQuant.Core.Services;
public interface IOriginalService
{
    IObservableList<IDataFile> FileIndexs { get; }

    IObservableCache<IDataFile, string> CombinationFileIndexs { get; }

    IObservableList<ProductTemplate> ProductTemplates { get; }

    void Combination(List<IDataFile> basicDataFiles);

    void Remove(string id);

    void LoadFile(IEnumerable<FileInfo> fileInfos);

    void SaveProductTemplate(ProductTemplate productTemplate);
}
