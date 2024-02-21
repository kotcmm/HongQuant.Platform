using System.Reactive.Linq;
using System.Text;

namespace HongQuant.Analysis.Original;

/// <summary>
///  原始数据的单个文件
/// </summary>
/// <param name="fileInfo"></param>
public class BasicDataFile(FileInfo fileInfo) : DataFile
{
    /// <summary>
    ///     信号文件信息
    /// </summary>
    public FileInfo FileInfo { get; } = fileInfo;

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
    public override BasicDataFile Calculate(string name = "")
    {
        if (_basicDatas.Count == 0)
        {
            _basicDatas = Load().ToList();
        }

        Name = FileInfo.Name;

        var s = Name.Split('_');

        Product = s[0];

        Strategy = s[1];

        var signalDatas = _basicDatas.TransformBasicDatas(Product);

        _calculateChangeSubject.OnNext(signalDatas);

        return this;
    }
}
