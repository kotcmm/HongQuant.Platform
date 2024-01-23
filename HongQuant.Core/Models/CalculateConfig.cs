using HongQuant.Analysis.Hong.Domain;

namespace HongQuant.Core.Models;
public class CalculateConfig
{
    public int Id { get; set; }

    public string? Key { get; set; }

    public SignalDataConfig? SignalDataConfig { get; set; }
}
