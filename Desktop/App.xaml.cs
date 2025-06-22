using System.Windows;
using ApplicationCore.Interfaces;
using DAL.Connectors;
using Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IServiceProvider _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        _serviceProvider = serviceCollection.BuildServiceProvider();
        
        Exit += OnExit;

        var mainWindow = new MainWindow
        {
            DataContext = _serviceProvider.GetService<MainWindowViewModel>()
        };
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();
        
        services.AddSingleton<ITestConnector, BitfinexConnector>();
        services.AddSingleton<MainWindowViewModel>();
    }

    private void OnExit(object sender, ExitEventArgs e)
    {
        Exit -= OnExit;
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}