using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DatingApp.Desktop.Models;
using DatingApp.Desktop.Services;
using System.Windows;
using Application = System.Windows.Application;
using System.Linq;

namespace DatingApp.Desktop.ViewModels;

public partial class OnboardingViewModel : ObservableObject
{
    private readonly HttpClient _httpClient;
    
    [ObservableProperty]
    private int _currentStep = 1;

    [ObservableProperty]
    private int _currentProgress = 25;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = "";


    [ObservableProperty] private int _selectedInterestedInIndex = 1; // 0: Men, 1: Women, 2: Everyone
    [ObservableProperty] private int _distance = 50;
    
    // Bước 2: Ngoại hình & Lối sống
    [ObservableProperty] private string _height = "170";
    [ObservableProperty] private int _smokingIndex = 0; // 0: No, 1: Sometimes, 2: Yes
    [ObservableProperty] private int _drinkingIndex = 0; // 0: No, 1: Socially, 2: Yes
    [ObservableProperty] private string _education = "";
    
    // Bước 3: Sở thích & Bio
    [ObservableProperty] private string _bio = "";
    public ObservableCollection<InterestTag> AvailableInterests { get; } = new()
    {
        new InterestTag { Name = "Du lịch" },
        new InterestTag { Name = "Âm nhạc" },
        new InterestTag { Name = "Thể thao" },
        new InterestTag { Name = "Đọc sách" },
        new InterestTag { Name = "Gaming" },
        new InterestTag { Name = "Nhiếp ảnh" },
        new InterestTag { Name = "Ẩm thực" },
        new InterestTag { Name = "Thú cưng" }
    };
    
    public OnboardingViewModel(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
    }

    [RelayCommand]
    private void NextStep()
    {
        if (CurrentStep < 4)
        {
            CurrentStep++;
            CurrentProgress = CurrentStep * 25;
        }
    }

    [RelayCommand]
    private void PrevStep()
    {
        if (CurrentStep > 1)
        {
            CurrentStep--;
            CurrentProgress = CurrentStep * 25;
        }
    }

    [RelayCommand]
    private async Task CompleteOnboardingAsync()
    {
        IsBusy = true;
        ErrorMessage = "";
        
        try
        {
            var selectedTags = AvailableInterests.Where(x => x.IsSelected).Select(x => x.Name).ToList();
            
            var dto = new
            {
                Gender = (int?)null,
                InterestedIn = SelectedInterestedInIndex == 2 ? (int?)null : SelectedInterestedInIndex,
                Distance,
                Height = int.TryParse(Height, out var h) ? h : (int?)null,
                Smoking = SmokingIndex == 0 ? "Không" : (SmokingIndex == 1 ? "Thỉnh thoảng" : "Thường xuyên"),
                Drinking = DrinkingIndex == 0 ? "Không" : (DrinkingIndex == 1 ? "Xã giao" : "Thường xuyên"),
                Education,
                Bio,
                Interests = selectedTags
            };

            var app = (App)Application.Current;
            var authService = app.Services.GetService(typeof(AuthService)) as AuthService;
            if (authService?.CurrentUser != null)
            {
                var response = await _httpClient.PutAsJsonAsync($"/api/User/onboarding", dto);
                if (response.IsSuccessStatusCode)
                {
                    authService.CurrentUser.IsOnboarded = true;
                    await authService.SaveSessionAsync(authService.CurrentToken!, authService.CurrentUser);
                    // Chuyển về Dashboard
                    var dashboardVm = app.Services.GetService(typeof(DashboardViewModel));
                    WeakReferenceMessenger.Default.Send(new Messages.NavigationMessage(dashboardVm!));
                }
                else
                {
                    ErrorMessage = "Lưu thông tin thất bại. Vui lòng thử lại!";
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Lỗi: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public partial class InterestTag : ObservableObject
{
    [ObservableProperty]
    private string _name = "";
    
    [ObservableProperty]
    private bool _isSelected = false;
}
