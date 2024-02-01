using DynamicData;
using HongQuant.Analysis.YiTou;
using HongQuant.Core.Services;
using HongQuant.Platform.Infrastucture.Drops.Files;
using HongQuant.Platform.ViewModels.Original;
using HongQuant.Platform.ViewModels.YiTou.Configs;
using HongQuant.Platform.ViewModels.YiTou.IncomeStatistics;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Linq;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

    private readonly ReadOnlyObservableCollection<CombinationBasicDataFileViewModel> _combinationFileIndexs;
    public ReadOnlyObservableCollection<CombinationBasicDataFileViewModel> CombinationFileIndexs => _combinationFileIndexs;

    [Reactive]
    public DataFileViewModel? SelectFileIndex { get; set; }

    private readonly ICycleIncomeService _cycleIncomeService;

    public YiTouMainViewModel()
    {
        _cycleIncomeService = Locator.Current.GetService<ICycleIncomeService>()!;
        var originalService = Locator.Current.GetService<IOriginalService>()!;

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

        originalService
              .CombinationFileIndexs
              .Connect()
              .Transform(f => new CombinationBasicDataFileViewModel(f))
              .ObserveOn(RxApp.MainThreadScheduler)
              .Bind(out _combinationFileIndexs)
              .DisposeMany()
              .Subscribe();

        this.WhenAnyValue(x => x.SelectAlgorithmType).Subscribe(ConfigChang);
        this.WhenAnyValue(x => x.Config).Subscribe(ConfigChange);
        this.WhenAnyValue(x => x.SelectFileIndex).Subscribe(SelectFileIndexChange);
    }

    private void ConfigChange(IConfigIncomeViewModel? x)
    {
        x?.Change.Subscribe(y =>
        {
            SelectFileIndexChange(SelectFileIndex);
            Load();
        });
    }

    private void SelectFileIndexChange(DataFileViewModel? x)
    {
        if (x != null && x.Items != null)
        {
            var datas = x.Items.Select(x => new IncomeInfoModel(
                x.TradeDate.ToDateTime(new()),
                0,
                x.Yields
            ));

            _cycleIncomeService.StatisticsData(Config?.Config, SelectAlgorithmType.Value, datas);
        }
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
