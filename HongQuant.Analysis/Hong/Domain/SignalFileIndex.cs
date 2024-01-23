using HongQuant.Analysis.Hong.Formula;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace HongQuant.Analysis.Hong.Domain;

/// <summary>
/// 数据文件的解析
/// </summary>
public class SignalFileIndex(FileInfo fileInfo)
{
    /// <summary>
    ///     信号文件信息
    /// </summary>
    public FileInfo FileInfo { get; } = fileInfo;

    private readonly Subject<IEnumerable<SignalData>> _calculateChangeSubject = new();
    public IObservable<IEnumerable<SignalData>> CalculateChange => _calculateChangeSubject.AsObservable();

    private IEnumerable<OriginalData> Load()
    {
        using var stream = FileInfo.OpenRead();

        using var streamReader = new StreamReader(stream, Encoding.GetEncoding("GB2312"));

        var index = OriginalDataIndex.BuildData(streamReader.ReadLine()!);

        while (!streamReader.EndOfStream)
        {
            var line = streamReader.ReadLine();

            var data = OriginalData.BuildData(line, index);

            if (data != null && data.Verify())
            {
                yield return data;
            }
        }
    }

    private List<OriginalData> _originalDatas = [];
    public IEnumerable<SignalData> Calculate(SignalDataConfig config, bool isReload = false)
    {
        if (_originalDatas.Count == 0 || isReload)
        {
            _originalDatas = Load().ToList();
        }

        var signalDatas = _originalDatas.ToSignalDatas(config);

        _calculateChangeSubject.OnNext(signalDatas);

        return signalDatas;
    }
}
