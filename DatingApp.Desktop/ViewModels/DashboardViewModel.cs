using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DatingApp.Desktop.Models;
using DatingApp.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DatingApp.Desktop.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly HttpClient _httpClient;

    [ObservableProperty]
    private bool _isAdmin;

    [ObservableProperty]
    private string _currentUserName = "Đang tìm kiếm...";

    [ObservableProperty]
    private string _currentUserBio = "";

    [ObservableProperty]
    private string _currentUserImage = "https://via.placeholder.com/600x800/FFF5F8/E6005C?text=Loading...";

    // --- Tabs ---
    [ObservableProperty]
    private bool _isDiscoverVisible = true;

    [ObservableProperty]
    private bool _isProfileVisible = false;

    [ObservableProperty]
    private string _discoverTabColor = "#E6005C";

    [ObservableProperty]
    private string _profileTabColor = "#6B7280";

    // --- Profile Data ---
    [ObservableProperty]
    private string _profileAvatarUrl = "pack://application:,,,/Resources/default-avatar.jpg";

    [ObservableProperty]
    private string _profileFullName = "";

    [ObservableProperty]
    private string _profileBio = "";

    [ObservableProperty]
    private string _profileGender = "";

    [ObservableProperty]
    private DateTime? _profileDateOfBirth = null;

    [ObservableProperty]
    private string _profileInterestedIn = "";

    [ObservableProperty]
    private string _profileHeight = "";

    [ObservableProperty]
    private string _profileEducation = "";

    [ObservableProperty]
    private string _profileSmoking = "";

    [ObservableProperty]
    private string _profileDrinking = "";

    [ObservableProperty]
    private string _profileOccupation = "";

    [ObservableProperty]
    private string _profileLocation = "";

    [ObservableProperty]
    private string _profileZodiac = "";

    [ObservableProperty]
    private string _profileMbti = "";

    [ObservableProperty]
    private string _profileLookingFor = "";

    [ObservableProperty]
    private string _profileLifestyle = "";

    [ObservableProperty]
    private string _profileVibe = "";

    [ObservableProperty]
    private string _profileMaxDistance = "";

    [ObservableProperty]
    private string _profileInterests = "";

    [ObservableProperty]
    private string _profileValues = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsProfileComplete))]
    private int _profileCompletionScore = 0;

    public bool IsProfileComplete => ProfileCompletionScore == 100;

    // --- Match System ---
    [ObservableProperty]
    private bool _isMatchPopupVisible = false;

    private List<DiscoverUserDto> _discoverQueue = new();
    private DiscoverUserDto? _currentUserDto;

    public DashboardViewModel(AuthService authService, IHttpClientFactory httpClientFactory)
    {
        _authService = authService;
        _httpClient = httpClientFactory.CreateClient("ApiClient");
        IsAdmin = _authService.CurrentUser?.IsAdmin == true;

        // Set token trực tiếp — đảm bảo luôn có Bearer token
        if (!string.IsNullOrEmpty(_authService.CurrentToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authService.CurrentToken);
        }

        _ = LoadDiscoverUsersAsync();
    }

    private async Task LoadDiscoverUsersAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<DiscoverResponse>("/api/User/discover");
            if (response != null && response.Data.Count > 0)
            {
                _discoverQueue = response.Data;
                NextProfile();
            }
            else
            {
                CurrentUserName = "Hết người để khám phá!";
                CurrentUserBio = "Hãy thử lại sau nhé.";
                CurrentUserImage = "https://via.placeholder.com/600x800/FFF5F8/E6005C?text=No+more+profiles"; 
                _currentUserDto = null;
            }
        }
        catch (Exception ex)
        {
            CurrentUserName = "Lỗi kết nối mạng";
            CurrentUserBio = ex.Message;
        }
    }

    [RelayCommand]
    public async Task LikeAsync()
    {
        if (_currentUserDto == null) return;

        try
        {
            var dto = new { ToUserId = _currentUserDto.Id, IsLike = true };
            var response = await _httpClient.PostAsJsonAsync("/api/Swipe", dto);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                if (result.TryGetProperty("isMatch", out var isMatchProp) && isMatchProp.GetBoolean())
                {
                    IsMatchPopupVisible = true;
                }
            }
        }
        catch {}

        NextProfile();
    }

    [RelayCommand]
    public async Task PassAsync()
    {
        if (_currentUserDto == null) return;

        try
        {
            var dto = new { ToUserId = _currentUserDto.Id, IsLike = false };
            await _httpClient.PostAsJsonAsync("/api/Swipe", dto);
        }
        catch {}

        NextProfile();
    }

    [RelayCommand]
    private void OpenAdmin()
    {
        // Navigate to AdminView
        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(
            new DatingApp.Desktop.Messages.NavigationMessage(((App)System.Windows.Application.Current).Services.GetService(typeof(AdminViewModel)))
        );
    }

    private void NextProfile()
    {
        if (_discoverQueue.Count > 0)
        {
            _currentUserDto = _discoverQueue[0];
            _discoverQueue.RemoveAt(0);

            CurrentUserName = $"{_currentUserDto.FullName}, {_currentUserDto.Age}";
            CurrentUserBio = _currentUserDto.Bio;
            CurrentUserImage = string.IsNullOrEmpty(_currentUserDto.AvatarUrl) 
                ? "pack://application:,,,/Resources/default-avatar.jpg" // Fallback cho có ảnh
                : _currentUserDto.AvatarUrl;
        }
        else
        {
            CurrentUserName = "Hết người để khám phá!";
            CurrentUserBio = "Hãy thử lại sau nhé.";
            CurrentUserImage = "https://via.placeholder.com/600x800/FFF5F8/E6005C?text=No+more+profiles";
            _currentUserDto = null;
        }
    }

    [RelayCommand]
    private void CloseMatchPopup()
    {
        IsMatchPopupVisible = false;
    }

    // --- TABS & PROFILE COMMANDS ---

    [RelayCommand]
    private void ShowDiscover()
    {
        IsDiscoverVisible = true;
        IsProfileVisible = false;
        DiscoverTabColor = "#E6005C";
        ProfileTabColor = "#6B7280";
    }

    [RelayCommand]
    private async Task ShowProfileAsync()
    {
        IsDiscoverVisible = false;
        IsProfileVisible = true;
        DiscoverTabColor = "#6B7280";
        ProfileTabColor = "#E6005C";

        try
        {
            var profile = await _httpClient.GetFromJsonAsync<UserDto>("/api/User/profile");
            if (profile != null)
            {
                ProfileFullName = profile.FullName ?? "";
                ProfileBio = profile.Bio ?? "";
                ProfileGender = profile.Gender == 0 ? "Nam" : (profile.Gender == 1 ? "Nữ" : "Khác");
                ProfileAvatarUrl = string.IsNullOrEmpty(profile.AvatarUrl) ? "pack://application:,,,/Resources/default-avatar.jpg" : profile.AvatarUrl;
                ProfileCompletionScore = profile.ProfileCompletionScore;
                
                ProfileDateOfBirth = profile.DateOfBirth;
                ProfileHeight = profile.Height?.ToString() ?? "";
                ProfileEducation = profile.Education ?? "";
                ProfileOccupation = profile.Occupation ?? "";
                ProfileLocation = profile.Location ?? "";
                ProfileZodiac = profile.Zodiac ?? "";
                ProfileMbti = profile.Mbti ?? "";
                ProfileLookingFor = profile.LookingFor ?? "";
                ProfileLifestyle = profile.Lifestyle ?? "";
                ProfileVibe = profile.Vibe ?? "";
                ProfileSmoking = profile.Smoking ?? "";
                ProfileDrinking = profile.Drinking ?? "";
                ProfileMaxDistance = profile.MaxDistance?.ToString() ?? "";
                ProfileInterests = profile.Interests != null ? string.Join(", ", profile.Interests) : "";
                ProfileValues = profile.Values != null ? string.Join(", ", profile.Values) : "";
            }
        }
        catch { }
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        try
        {
            var dto = new 
            { 
                FullName = ProfileFullName,
                Bio = ProfileBio,
                Height = int.TryParse(ProfileHeight, out var h) ? h : (int?)null,
                MaxDistance = int.TryParse(ProfileMaxDistance, out var d) ? d : (int?)null,
                Education = ProfileEducation,
                Occupation = ProfileOccupation,
                Location = ProfileLocation,
                Zodiac = ProfileZodiac,
                Mbti = ProfileMbti,
                LookingFor = ProfileLookingFor,
                Lifestyle = ProfileLifestyle,
                Vibe = ProfileVibe,
                Smoking = ProfileSmoking,
                Drinking = ProfileDrinking,
                Interests = !string.IsNullOrWhiteSpace(ProfileInterests) ? new List<string>(ProfileInterests.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) : new List<string>(),
                Values = !string.IsNullOrWhiteSpace(ProfileValues) ? new List<string>(ProfileValues.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) : new List<string>()
            };
            await _httpClient.PutAsJsonAsync("/api/User/profile", dto);
            System.Windows.MessageBox.Show("Cập nhật hồ sơ thành công!", "Thành công");
        }
        catch { }
    }

    [RelayCommand]
    private async Task UploadAvatarAsync()
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                using var form = new MultipartFormDataContent();
                using var fileStream = new System.IO.FileStream(openFileDialog.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                using var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                
                form.Add(streamContent, "file", System.IO.Path.GetFileName(openFileDialog.FileName));

                var response = await _httpClient.PostAsync("/api/User/avatar", form);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                    if (result.TryGetProperty("avatarUrl", out var avatarUrlProp))
                    {
                        ProfileAvatarUrl = avatarUrlProp.GetString() ?? ProfileAvatarUrl;
                        _authService.CurrentUser!.AvatarUrl = ProfileAvatarUrl; // Update local session
                    }
                    System.Windows.MessageBox.Show("Cập nhật Avatar thành công!", "Thành công");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi upload ảnh: " + ex.Message, "Lỗi");
            }
        }
    }
}
