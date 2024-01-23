using HongQuant.Core.Services;
using HongQuant.Core.Stores;
using HongQuant.Platform.Infrastucture.Drops.Files;
using ReactiveUI;
using Splat;
using System.IO;
using System.Reactive;
using System.Windows.Input;

namespace HongQuant.Platform.ViewModels.Hong
{
    public class HongMainViewModel : ReactiveObject
    {
        private readonly IHongAnalysisService _hongAnalysisService;

        public ReactiveCommand<Unit, Unit> OpenSettingCommand { get; }

        public ICommand StartRunCommand { get; }

        public FileDropMonitor DropMonitor { get; }

        public HongMainViewModel()
        {
            _hongAnalysisService = Locator.Current.GetService<IHongAnalysisService>()!;

            DropMonitor = new FileDropMonitor();
            OpenSettingCommand = ReactiveCommand.Create<Unit>(integer => { });
            StartRunCommand = ReactiveCommand.Create(StartRun);

            DropMonitor.Dropped.Subscribe(OpenFile);
        }

        private void OpenFile(IEnumerable<FileInfo> files)
        {
            _hongAnalysisService.LoadFile(files);
        }

        public void StartRun()
        {
            var config = CalculateConfigStore.GetDefault();

            if(config == null) throw new ArgumentNullException(nameof(config));

            foreach (var item in _hongAnalysisService.FileIndexs.Items)
            {
                item.Calculate(config);
            }
        }
    }
}
