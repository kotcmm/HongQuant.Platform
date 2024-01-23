using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HongQuant.Platform.Infrastucture;
public class LimitPropsContractResolver : DefaultContractResolver
{
    private readonly string[] _props;

    private readonly bool _retain;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="props">传入的属性数组</param>
    /// <param name="retain">true:表示props是需要保留的字段  false：表示props是要排除的字段</param>
    public LimitPropsContractResolver(string[] props, bool retain = true)
    {
        //指定要序列化属性的清单
        _props = props;

        _retain = retain;
    }

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        IList<JsonProperty> list = base.CreateProperties(type, memberSerialization);
        //只保留清单有列出的属性
        return list.Where(p =>
        {
            if (_retain)
            {
                return _props.Contains(p.PropertyName);
            }
            else
            {
                return !_props.Contains(p.PropertyName);
            }
        }).ToList();
    }
}
