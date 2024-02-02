using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace HongQuant.Analysis.Original;
public abstract class DataFile : IDataFile
{
    protected readonly BehaviorSubject<IEnumerable<BasicTransformData>> _calculateChangeSubject = new([]);
    public IObservable<IEnumerable<BasicTransformData>> CalculateChange => _calculateChangeSubject.AsObservable();
    public IEnumerable<BasicTransformData> Items => _calculateChangeSubject.Value;

    public string Id { get; protected set; } = string.Empty;

    public string Name { get; protected set; } = string.Empty;

    public string Product { get; protected set; } = string.Empty;

    public string Strategy { get; protected set; } = string.Empty;

    public abstract IDataFile Calculate();
}
