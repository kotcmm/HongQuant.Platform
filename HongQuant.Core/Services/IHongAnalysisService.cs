using DynamicData;
using HongQuant.Analysis.Hong.Domain;

namespace HongQuant.Core.Services;
public interface IHongAnalysisService
{
    IObservableList<SignalFileIndex> FileIndexs { get; }

    void LoadFile(IEnumerable<FileInfo> fileInfos);
}
