using DynamicData;
using HongQuant.Analysis.Original;

namespace HongQuant.Core.Services;
public interface IOriginalService
{
    IObservableList<IDataFile> FileIndexs { get; }

    IObservableCache<IDataFile, string> CombinationFileIndexs { get; }

    void Combination(List<IDataFile> basicDataFiles);

    void Remove(string id);

    void LoadFile(IEnumerable<FileInfo> fileInfos);
}
