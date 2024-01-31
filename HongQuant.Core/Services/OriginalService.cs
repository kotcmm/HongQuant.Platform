using DynamicData;
using HongQuant.Analysis.Original;

namespace HongQuant.Core.Services;
public class OriginalService : IOriginalService
{
    private readonly SourceList<BasicDataFile> _fileIndexs = new();
    public IObservableList<BasicDataFile> FileIndexs => _fileIndexs.AsObservableList();

    public void LoadFile(IEnumerable<FileInfo> fileInfos)
    {
        _fileIndexs.Clear();

        _fileIndexs.AddRange(fileInfos.Select(fileInfo => new BasicDataFile(fileInfo)));
    }
}
