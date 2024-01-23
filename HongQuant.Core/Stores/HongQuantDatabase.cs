using HongQuant.Core.Models;
using LiteDB;

namespace HongQuant.Core.Stores;
public class HongQuantDatabase : LiteDatabase
{
    private const string Dir = "Config";
    private const string TableContractType = "ContractTypes";
    private const string TableContract = "Contracts";
    private const string TableCalculateConfig = "CalculateConfigs";
    static HongQuantDatabase()
    {
        if (!Directory.Exists(Dir))
        {
            Directory.CreateDirectory(Dir);
        }
    }

    public HongQuantDatabase() : base($"{Dir}\\HongQuantConfig.db", BsonMapper.Global)
    {
        Init();
    }

    private void Init()
    {
        Mapper.Entity<ContractType>()
            .DbRef(x => x.Contracts, TableContract);

        Mapper.Entity<Contract>()
            .DbRef(x => x.ContractType, TableContractType);
    }

    public ILiteCollection<ContractType> ContractTypes => GetCollection<ContractType>(TableContractType);

    public ILiteCollection<Contract> Contracts => GetCollection<Contract>(TableContract);

    public ILiteCollection<CalculateConfig> CalculateConfigs => GetCollection<CalculateConfig>(TableCalculateConfig);

}
