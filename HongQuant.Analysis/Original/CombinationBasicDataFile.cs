using System.Reactive.Linq;

namespace HongQuant.Analysis.Original;
public class CombinationBasicDataFile : DataFile
{
    private readonly List<IDataFile> _basicDataFiles;

    public CombinationBasicDataFile(List<IDataFile> basicDataFiles)
    {
        _basicDataFiles = basicDataFiles;

        Id = string.Join("-", basicDataFiles.Select(x => x.Product));
    }

    public override CombinationBasicDataFile Calculate(string name = "")
    {
        Name = string.IsNullOrEmpty(name) ?
            $"组合{_basicDataFiles.Count}品种" : name;

        var signalDatas = _basicDataFiles
            .MeragedByTradeDate(_basicDataFiles.Count)
            .AttachMeragedBasicDatas();

        _calculateChangeSubject.OnNext(signalDatas);

        return this;
    }
}
