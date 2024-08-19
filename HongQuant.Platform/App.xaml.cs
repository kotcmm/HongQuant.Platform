using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using OfficeOpenXml;
using ReactiveUI;
using Serilog;
using SkiaSharp;
using System.Diagnostics;
using System.Reactive;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace HongQuant.Platform;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        LiveCharts.Configure(config =>
        {
            config.HasGlobalSKTypeface(SKFontManager.Default.MatchCharacter('汉'));
        });
        _ = new AppBootstrapper();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        //UI线程未捕获异常处理事件（UI主线程）
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        //非UI线程未捕获异常处理事件(例如自己创建的一个子线程)
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        //Task线程内未捕获异常处理事件
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        RxApp.DefaultExceptionHandler = Observer.Create<Exception>(ex =>
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
            Log.Error(ex, ex.Message);
            MessageBox.Show(ex.Message, "异常提示");
        });
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File($"logs\\{DateTime.Now:yyyyMMdd}.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
    }

    //UI线程未捕获异常处理事件（UI主线程）
    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var ex = e.Exception;
        var msg = $"{ex.Message}\n\n{ex.StackTrace}"; //异常信息 和 调用堆栈信息
        Log.Error(ex, ex.Message);
        MessageBox.Show(msg, "UI线程异常");

        e.Handled = true; //表示异常已处理，可以继续运行
    }

    //非UI线程未捕获异常处理事件(例如自己创建的一个子线程)
    //如果UI线程异常DispatcherUnhandledException未注册，则如果发生了UI线程未处理异常也会触发此异常事件
    //此机制的异常捕获后应用程序会直接终止。没有像DispatcherUnhandledException事件中的Handler=true的处理方式，可以通过比如Dispatcher.Invoke将子线程异常丢在UI主线程异常处理机制中处理
    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            var msg = $"{ex.Message}\n\n{ex.StackTrace}"; //异常信息 和 调用堆栈信息
            Log.Error(ex, ex.Message);
            MessageBox.Show(msg, "非UI线程异常");
        }
    }

    //Task线程内未捕获异常处理事件
    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Exception ex = e.Exception;
        Log.Error(ex, ex.Message);
        MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}", "Task异常");
    }
}
