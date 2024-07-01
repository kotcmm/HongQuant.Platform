using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace HongQuant.Platform.ViewModels.Original;

public class ShowLastViewModel : ReactiveObject
{
    [Reactive]
    public ObservableCollection<BasicTransformDataViewModel>? LastItems { get; private set; }

    [Reactive]
    public int LastIndex { get; set; }

    public ICommand NextCommand { get; }

    public ICommand PreCommand { get; }

    public ICommand LastCommand { get; }

    public ShowLastViewModel(ShowDataViewModel input)
    {
        PreCommand = ReactiveCommand.Create(() => LastIndex++);
        NextCommand = ReactiveCommand.Create(() => { if (LastIndex > 0) LastIndex--; });
        LastCommand = ReactiveCommand.Create(() => LastIndex = 0);

        this.WhenAnyValue(x => x.LastIndex).Subscribe(i =>
        {
            var selecteds = input.FileIndexs.Where(x => x.IsSelected);

            var lasts = selecteds.Select(x => x.Items?.SkipLast(LastIndex).Last()).OfType<BasicTransformDataViewModel>();

            LastItems = new(lasts);
        });

        LastIndex = 0;
    }
}
