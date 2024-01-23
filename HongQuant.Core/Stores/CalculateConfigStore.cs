using HongQuant.Analysis.Hong.Domain;
using HongQuant.Core.Models;

namespace HongQuant.Core.Stores;
public class CalculateConfigStore
{
    private const string DefaultKey = "DefaultKey";

    public static SignalDataConfig? GetDefault()
    {
        using var db = new HongQuantDatabase();

        var query = db.CalculateConfigs
            .FindOne(x => x.Key == DefaultKey);

        return query?.SignalDataConfig;
    }

    public static void SetDefault(SignalDataConfig config)
    {
        using var db = new HongQuantDatabase();

        var query = db.CalculateConfigs
            .FindOne(x => x.Key == DefaultKey);

        if (query == null)
        {
            db.CalculateConfigs.Insert(new CalculateConfig
            {
                Key = DefaultKey,
                SignalDataConfig = config
            });
        }
        else
        {
            query.SignalDataConfig = config;

            db.CalculateConfigs.Update(query);
        }
    }
}
