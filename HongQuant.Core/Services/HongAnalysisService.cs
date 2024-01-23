
using DynamicData;
using HongQuant.Analysis.Hong.Domain;

namespace HongQuant.Core.Services;
public class HongAnalysisService : IHongAnalysisService
{
    private readonly SourceList<SignalFileIndex> _fileIndexs = new();
    public IObservableList<SignalFileIndex> FileIndexs => _fileIndexs.AsObservableList();

    public void LoadFile(IEnumerable<FileInfo> fileInfos)
    {
        _fileIndexs.Clear();

        _fileIndexs.AddRange(fileInfos.Select(fileInfo => new SignalFileIndex(fileInfo)));
    }
}
