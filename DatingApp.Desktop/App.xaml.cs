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

        // ─── Quan trọng: AuthService PHẢI đăng ký trước AuthTokenHandler ───
        services.AddSingleton<DatingApp.Desktop.Services.AuthService>();

        // Register Auth token handler — automatically attaches Bearer token to all requests
        services.AddTransient<DatingApp.Desktop.Http.AuthTokenHandler>();

        // ─── HttpClient cho AuthService (login/register — không cần token) ───
        services.AddHttpClient("PublicClient", client =>
        {
            client.BaseAddress = new Uri("https://localhost:7150");
        });

        // ─── HttpClient cho tất cả ViewModels (tự động đính kèm Bearer token) ───
        services.AddHttpClient("ApiClient", client =>
        {
            client.BaseAddress = new Uri("https://localhost:7150");
        })
        .AddHttpMessageHandler<DatingApp.Desktop.Http.AuthTokenHandler>();

        // Register Views
        services.AddTransient<MainWindow>();

        // Register ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<AdminViewModel>();
        services.AddTransient<OnboardingViewModel>();

        return services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}
