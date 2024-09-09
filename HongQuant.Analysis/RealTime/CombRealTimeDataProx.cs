
namespace HongQuant.Analysis.RealTime;
public class CombRealTimeDataProx
{
    private IEnumerable<string>? _datas;

    public void SetData(IEnumerable<string> datas)
    {
        _datas = datas;
    }

    public bool HasDatas()
    {
        return _datas != null && _datas.Any();
    }
}
