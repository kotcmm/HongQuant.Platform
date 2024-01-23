using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;

namespace HongQuant.Platform.Infrastucture.Drops.Files
{
    public sealed class FileDropMonitor : IDependencyObjectReceiver, IDisposable
    {
        private readonly SerialDisposable _cleanUp = new();
        private readonly Subject<IEnumerable<FileInfo>> _fileDropped = new Subject<IEnumerable<FileInfo>>();
        private bool _isLoaded;

        public void Receive(DependencyObject value)
        {
            if (_isLoaded || null == value)
                return;

            _isLoaded = true;

            if (value is not UIElement control)
                return;

            control.AllowDrop = true;

            DragAdorner? adorner = null;

            var window = Window.GetWindow(value);

            var createAdorner = Observable.FromEventPattern<DragEventHandler, DragEventArgs>
                (h => control.PreviewDragEnter += h, h => control.PreviewDragEnter -= h)
                .Select(ev => ev.EventArgs)
                .Subscribe(e =>
                {
                    var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    var container = new FileDropContainer(files);
                    var contentPresenter = new ContentPresenter { Content = container };

                    adorner = new DragAdorner(control, contentPresenter);
                });

            var clearAdorner = Observable.FromEventPattern<DragEventHandler, DragEventArgs>(h => control.PreviewDragLeave += h, h => control.PreviewDragLeave -= h).ToUnit()
                .Merge(Observable.FromEventPattern<DragEventHandler, DragEventArgs>(h => control.PreviewDrop += h, h => control.PreviewDrop -= h).ToUnit())
                .Subscribe(e =>
                {
                    if (adorner == null) return;

                    adorner.Detatch();

                    adorner = null;
                });

            var updatePositionOfAdornment = Observable.FromEventPattern<DragEventHandler, DragEventArgs>
                    (h => control.PreviewDragOver += h, h => control.PreviewDragOver -= h)
                    .Select(ev => ev.EventArgs)
                    .Where(_ => adorner != null)
                    .Subscribe(e =>
                    {
                        if (adorner == null) return;

                        adorner.MousePosition = e.GetPosition(window);
                    });

            var dropped = Observable.FromEventPattern<DragEventHandler, DragEventArgs>
                (h => control.Drop += h, h => control.Drop -= h)
                .Select(ev => ev.EventArgs)
                .Select(e =>
                {
                    if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                        return Enumerable.Empty<FileInfo>();

                    // Note that you can have more than one file.
                    var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    var list = new List<FileInfo>();
                    foreach (var file in files)
                    {
                        FileAttributes attr = File.GetAttributes(file);
                        if (attr.HasFlag(FileAttributes.Directory))
                        {
                            var subfiles = Directory.GetFiles(file);
                            list.AddRange(subfiles.Select(f => new FileInfo(f)));
                        }
                        else
                        {
                            list.Add(new FileInfo(file));
                        }
                    }

                    return list;
                })
                .SubscribeSafe(_fileDropped);

            _cleanUp.Disposable = Disposable.Create(() =>
            {
                updatePositionOfAdornment.Dispose();
                clearAdorner.Dispose();
                createAdorner.Dispose();
                dropped.Dispose();
                _fileDropped.OnCompleted();
            });
        }

        public IObservable<IEnumerable<FileInfo>> Dropped => _fileDropped;

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
