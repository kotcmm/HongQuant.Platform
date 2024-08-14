namespace HongQuant.Core.Models;
public class RealTimeFileData
{
    public required string Product { get; set; }

    public required string Strategy { get; set; }

    public string? Value { get; set; }
}
