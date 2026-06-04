using System;
using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DatingApp.Desktop.ViewModels;

namespace DatingApp.Desktop;

public partial class App : Application
{
    public new static App Current => (App)Application.Current;
    public IServiceProvider Services { get; }

    public App()
    {
        Services = ConfigureServices();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register HTTP Client
        services.AddHttpClient("ApiClient", client =>
        {
            client.BaseAddress = new Uri("http://localhost:5267"); 
        });

        // Register Views
        services.AddTransient<MainWindow>();

        // Register ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<LoginViewModel>();

        // Register Services
        services.AddSingleton<DatingApp.Desktop.Services.AuthService>();

        return services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
