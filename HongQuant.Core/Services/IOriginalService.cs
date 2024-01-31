using DynamicData;
using HongQuant.Analysis.Original;

namespace HongQuant.Core.Services;
public interface IOriginalService
{
    IObservableList<BasicDataFile> FileIndexs { get; }

    void LoadFile(IEnumerable<FileInfo> fileInfos);
}
