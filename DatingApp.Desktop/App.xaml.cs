using System;
using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MaterialDesignThemes.Wpf;
using DatingApp.Desktop.ViewModels;

namespace DatingApp.Desktop;

public partial class App : System.Windows.Application
{
    public new static App Current => (App)System.Windows.Application.Current;
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

    public void SetDarkTheme(bool isDark)
    {
        try
        {
            var paletteHelper = new MaterialDesignThemes.Wpf.PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(isDark ? MaterialDesignThemes.Wpf.BaseTheme.Dark : MaterialDesignThemes.Wpf.BaseTheme.Light);
            paletteHelper.SetTheme(theme);

            var fbBg = isDark ? System.Windows.Media.Color.FromRgb(20, 20, 26) : System.Windows.Media.Color.FromRgb(240, 242, 245);
            var fbCard = isDark ? System.Windows.Media.Color.FromArgb(180, 28, 28, 36) : System.Windows.Media.Color.FromRgb(255, 255, 255);
            var fbBorder = isDark ? System.Windows.Media.Color.FromArgb(40, 255, 255, 255) : System.Windows.Media.Color.FromRgb(228, 230, 235);
            var fbText = isDark ? System.Windows.Media.Color.FromRgb(245, 245, 247) : System.Windows.Media.Color.FromRgb(28, 30, 33);
            var fbTextSec = isDark ? System.Windows.Media.Color.FromRgb(170, 170, 180) : System.Windows.Media.Color.FromRgb(101, 103, 107);
            var glassBg = isDark ? System.Windows.Media.Color.FromArgb(30, 255, 255, 255) : System.Windows.Media.Color.FromArgb(80, 255, 255, 255);
            var glassBorder = isDark ? System.Windows.Media.Color.FromArgb(60, 255, 255, 255) : System.Windows.Media.Color.FromArgb(30, 0, 0, 0);

            Resources["FbBackground"] = new System.Windows.Media.SolidColorBrush(fbBg);
            Resources["FbCardBackground"] = new System.Windows.Media.SolidColorBrush(fbCard);
            Resources["FbBorder"] = new System.Windows.Media.SolidColorBrush(fbBorder);
            Resources["FbTextPrimary"] = new System.Windows.Media.SolidColorBrush(fbText);
            Resources["FbTextSecondary"] = new System.Windows.Media.SolidColorBrush(fbTextSec);
            Resources["GlassOverlayBackground"] = new System.Windows.Media.SolidColorBrush(glassBg);
            Resources["GlassOverlayBorder"] = new System.Windows.Media.SolidColorBrush(glassBorder);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[App] Error setting theme: {ex.Message}");
        }
    }
}
