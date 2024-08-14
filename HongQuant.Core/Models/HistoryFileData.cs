namespace HongQuant.Core.Models;
public class HistoryFileData
{
    public required string Product { get; set; }

    public required string Strategy { get; set; }

    public required List<string> Datas { get; set; }
}
