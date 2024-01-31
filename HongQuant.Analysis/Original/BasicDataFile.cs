using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace HongQuant.Analysis.Original;

/// <summary>
///  原始数据的单个文件
/// </summary>
/// <param name="fileInfo"></param>
public class BasicDataFile(FileInfo fileInfo)
{
    /// <summary>
    ///     信号文件信息
    /// </summary>
    public FileInfo FileInfo { get; } = fileInfo;

    private readonly Subject<IEnumerable<BasicTransformData>> _calculateChangeSubject = new();

    /// <summary>
    /// 计算原始数据后触发改变
    /// </summary>
    public IObservable<IEnumerable<BasicTransformData>> CalculateChange => _calculateChangeSubject.AsObservable();

    /// <summary>
    /// 加载原始数据
    /// </summary>
    /// <returns></returns>
    private IEnumerable<BasicData> Load()
    {
        using var stream = FileInfo.OpenRead();

        using var streamReader = new StreamReader(stream, Encoding.GetEncoding("GB2312"));

        while (!streamReader.EndOfStream)
        {
            var line = streamReader.ReadLine();

            var data = BasicData.BuildData(line);

            if (data != null && data.Verify())
            {
                yield return data;
            }
        }
    }

    private List<BasicData> _basicDatas = [];
    public IEnumerable<BasicTransformData> Calculate(bool isReload = false)
    {
        if (_basicDatas.Count == 0 || isReload)
        {
            _basicDatas = Load().ToList();
        }

        var signalDatas = _basicDatas.TransformBasicDatas();

        _calculateChangeSubject.OnNext(signalDatas);

        return signalDatas;
    }
}
