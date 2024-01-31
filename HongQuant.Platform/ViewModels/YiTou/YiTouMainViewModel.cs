using DynamicData;
using HongQuant.Analysis.YiTou;
using HongQuant.Core.Services;
using HongQuant.Platform.Infrastucture.Drops.Files;
using HongQuant.Platform.ViewModels.YiTou.Configs;
using HongQuant.Platform.ViewModels.YiTou.IncomeStatistics;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Linq;
using System.Windows.Input;

namespace HongQuant.Platform.ViewModels.YiTou;
public class YiTouMainViewModel : ReactiveObject
{
    [Reactive]
    public IConfigIncomeViewModel? Config { get; set; }

    [Reactive]
    public AlgorithmTypeViewModel SelectAlgorithmType { get; set; }

    [Reactive]
    public IncomeStatisticsViewModel? IncomeStatistic { get; set; }

    public ObservableCollection<AlgorithmTypeViewModel> AlgorithmTypes { get; }

    public FileDropMonitor DropMonitor { get; }

    public ICommand OpenFileCommand { get; }

    public ICommand SaveFileCommand { get; }

    public ICommand BarChartCommand { get; }

    public Interaction<string, string> ShowFileDialog { get; }

    public Interaction<string, string> SaveFileDialog { get; }


    public Interaction<YiTouMainViewModel, bool> OpenChartView { get; }


    private readonly ReadOnlyObservableCollection<IncomeStatisticsViewModel> _incomeStatistics;

    public ReadOnlyObservableCollection<IncomeStatisticsViewModel> IncomeStatistics => _incomeStatistics;

    private readonly ICycleIncomeService _cycleIncomeService;

    public YiTouMainViewModel()
    {
        _cycleIncomeService = Locator.Current.GetService<ICycleIncomeService>()!;

        DropMonitor = new FileDropMonitor();
        ShowFileDialog = new Interaction<string, string>();
        SaveFileDialog = new Interaction<string, string>();
        OpenChartView = new Interaction<YiTouMainViewModel, bool>();

        AlgorithmTypes =
        [
            new() { Name = "周期结算", Value = AlgorithmType.Cycle },
            new() { Name = "定投复利", Value = AlgorithmType.Multiple5 },
        ];

        SelectAlgorithmType = AlgorithmTypes[0];
        this.WhenAnyValue(x => x.SelectAlgorithmType).Subscribe(ConfigChang);
        this.WhenAnyValue(x => x.Config)
            .Subscribe(x => {
                x?.Change.Subscribe(y =>
                    {
                        Load();
                    });
            });

        DropMonitor.Dropped.Subscribe(OpenFile);
        OpenFileCommand = ReactiveCommand.CreateFromTask(OpenFile);
        SaveFileCommand = ReactiveCommand.CreateFromTask(SaveFile);
        BarChartCommand = ReactiveCommand.Create(OpenChart);

        _cycleIncomeService.IncomeStatistics
           .Connect()
           .Transform(x=> new IncomeStatisticsViewModel
           {
               Statistics = x,
               Start = x.Start,
               End = x.End,
               Total = x.Total,
               DayCount = x.DayCount,
               MaxTotal = x.MaxTotal,
               MaxTotalDate = x.MaxTotalDate,
               MaxRetracement = x.MaxRetracement,
               RetracementDate = x.RetracementDate,
               NetWorthInfo = x.NetWorthInfo,
               LastMultiples = x.LastMultiples,
           })
           .Bind(out _incomeStatistics)
           .DisposeMany()
           .Subscribe();
    }

    private async Task OpenFile()
    {
        var result = await ShowFileDialog.Handle("");

        if (!string.IsNullOrEmpty(result))
        {
            Load(new(result));
        }
    }

    private void ConfigChang(AlgorithmTypeViewModel x)
    {
        Config = x.Value switch
        {
            AlgorithmType.Cycle => new CycleConfigIncomeViewModel(),
            AlgorithmType.Multiple5 => new Multiple5ConfigIncomeViewModel(),
            _ => throw new NotImplementedException(),
        };
    }

    private async Task OpenChart()
    {
        await OpenChartView.Handle(this);
    }

    private void OpenFile(IEnumerable<FileInfo> files)
    {
        Load(files.First());
    }

    private FileInfo? _fileInfo;

    private void Load(FileInfo? fileInfo = null)
    {
        if (fileInfo != null)
        {
            _fileInfo = fileInfo;
        }

        if (_fileInfo == null) return;

        _cycleIncomeService.Load(_fileInfo, Config?.Config, SelectAlgorithmType.Value);
    }

    private async Task SaveFile()
    {
        var result = await SaveFileDialog.Handle("");

        if (!string.IsNullOrEmpty(result))
        {
            _cycleIncomeService.Save(result, _incomeStatistics.Select(x=>x.Statistics));
        }
    }
}
