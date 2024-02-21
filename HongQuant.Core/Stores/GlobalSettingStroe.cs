using HongQuant.Core.Models;

namespace HongQuant.Core.Stores;
public class GlobalSettingStroe
{
    /// <summary>
    ///     添加一个配置
    /// </summary>
    public static void SetValue(GlobalSetting setting)
    {
        using var db = new HongQuantDatabase();

        var old = db.GlobalSettings.FindOne(x=>x.Key == setting.Key);

        if(old == null)
        {
            db.GlobalSettings.Insert(setting);
        }
        else
        {
            old.Value = setting.Value;

            db.GlobalSettings.Update(old);
        } 
    }

    /// <summary>
    ///     获取一个配置值
    /// </summary>
    public static string? GetValue(string key)
    {
        using var db = new HongQuantDatabase();

        var old = db.GlobalSettings.FindOne(x => x.Key == key);

        return old?.Value;
    }
}
