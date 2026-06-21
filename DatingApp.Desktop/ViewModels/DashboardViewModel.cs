using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DatingApp.Desktop.Models;
using DatingApp.Desktop.Services;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using System.Windows;
using System.Linq;

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

    [ObservableProperty]
    private bool _isCurrentUserVerified;

    [ObservableProperty]
    private string _currentUserLocation = "";

    [ObservableProperty]
    private string _currentUserZodiac = "";

    [ObservableProperty]
    private string _currentUserMbti = "";

    [ObservableProperty]
    private int _currentUserCompatibilityScore;

    [ObservableProperty]
    private bool _isCompatibilityVisible;

    [ObservableProperty]
    private bool _isSuperLikedBy;

    [ObservableProperty]
    private ObservableCollection<string> _currentUserInterests = new();

    [ObservableProperty]
    private bool _isDiscoverQueueEmpty = false;

    [ObservableProperty]
    private bool _isEmojiPopupOpen = false;

    [ObservableProperty]
    private bool _isUserDetailVisible = false;

    [ObservableProperty]
    private ObservableCollection<PhotoDto> _currentUserPhotos = new();

    [ObservableProperty]
    private int? _currentUserHeight;

    [ObservableProperty]
    private string _currentUserOccupation = "";

    [ObservableProperty]
    private string _currentUserEducation = "";

    [ObservableProperty]
    private string _currentUserSmoking = "";

    [ObservableProperty]
    private string _currentUserDrinking = "";

    [ObservableProperty]
    private double? _currentUserDistance;

    [ObservableProperty]
    private bool _isDistanceVisible;

    // --- Tabs ---
    [ObservableProperty]
    private bool _isDiscoverVisible = true;

    [ObservableProperty]
    private bool _isProfileVisible = false;

    [ObservableProperty]
    private bool _isMessagesVisible = false;

    [ObservableProperty]
    private bool _isLikesVisible = false;

    [ObservableProperty]
    private string _discoverTabColor = "#E6005C";

    [ObservableProperty]
    private string _profileTabColor = "#6B7280";

    [ObservableProperty]
    private string _messagesTabColor = "#6B7280";

    [ObservableProperty]
    private string _likesTabColor = "#6B7280";

    [ObservableProperty]
    private ObservableCollection<DiscoverUserDto> _likesReceived = new();

    [ObservableProperty]
    private int _likesReceivedCount = 0;

    [ObservableProperty]
    private int _unreadNotificationCount;

    [ObservableProperty]
    private ObservableCollection<NotificationDto> _notifications = new();

    [ObservableProperty]
    private bool _isNotificationPanelOpen;

    // --- Profile Data ---
    [ObservableProperty]
    private string? _profileAvatarUrl = null;

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
    private ObservableCollection<PhotoDto> _profilePhotos = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsProfileComplete))]
    private int _profileCompletionScore = 0;

    public bool IsProfileComplete => ProfileCompletionScore == 100;

    // --- Match System ---
    [ObservableProperty]
    private bool _isMatchPopupVisible = false;

    private List<DiscoverUserDto> _discoverQueue = new();
    private DiscoverUserDto? _currentUserDto;

    // --- Discover Filters ---
    [ObservableProperty]
    private string _filterAgeMin = "18";

    [ObservableProperty]
    private string _filterAgeMax = "99";

    [ObservableProperty]
    private string _filterGender = ""; // Empty = All, "0" = Nam, "1" = Nữ

    [ObservableProperty]
    private string _filterMaxDistance = "50";

    [ObservableProperty]
    private bool _filterVerifiedOnly;

    [ObservableProperty]
    private bool _filterOnlineOnly;

    // --- Chat System ---
    private HubConnection? _hubConnection;

    [ObservableProperty]
    private ObservableCollection<MatchDto> _matches = new();

    [ObservableProperty]
    private ObservableCollection<MessageDto> _currentMessages = new();

    private bool _isReloadingMatches;

    public bool IsChatActive => SelectedMatch != null;

    private MatchDto? _selectedMatch;
    public MatchDto? SelectedMatch
    {
        get => _selectedMatch;
        set
        {
            if (_isReloadingMatches && value == null)
            {
                return;
            }

            var oldPartnerId = _selectedMatch?.Partner?.Id;
            if (SetProperty(ref _selectedMatch, value))
            {
                OnPropertyChanged(nameof(IsChatActive));
                if (value != null && value.Partner.Id != oldPartnerId)
                {
                    _ = LoadMessagesAsync(value.Partner.Id);
                }
            }
        }
    }

    [ObservableProperty]
    private string _messageDraft = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditingMessage))]
    private MessageDto? _editingMessage;

    public bool IsEditingMessage => EditingMessage != null;

    [ObservableProperty]
    private bool _isPartnerTyping;

    private System.Threading.CancellationTokenSource? _typingResetTokenSource;
    private DateTime _lastTypingSentTime = DateTime.MinValue;

    partial void OnMessageDraftChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _ = SendTypingNotificationAsync();
        }
    }

    private async Task SendTypingNotificationAsync()
    {
        if (SelectedMatch == null || _hubConnection == null || _hubConnection.State != HubConnectionState.Connected) return;

        var now = DateTime.UtcNow;
        if (now - _lastTypingSentTime > TimeSpan.FromSeconds(2))
        {
            _lastTypingSentTime = now;
            try
            {
                await _hubConnection.SendAsync("Typing", SelectedMatch.Partner.Id.ToString());
            }
            catch {}
        }
    }

    public DashboardViewModel(AuthService authService, IHttpClientFactory httpClientFactory)
    {
        _authService = authService;
        _httpClient = httpClientFactory.CreateClient("ApiClient");
        IsAdmin = _authService.CurrentUser?.IsAdmin == true;
        
        ProfileAvatarUrl = string.IsNullOrEmpty(_authService.CurrentUser?.AvatarUrl) 
            ? "pack://application:,,,/Resources/default-avatar.jpg" 
            : _authService.CurrentUser!.AvatarUrl;

        // Set token trực tiếp — đảm bảo luôn có Bearer token
        if (!string.IsNullOrEmpty(_authService.CurrentToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authService.CurrentToken);
        }

        _ = LoadDiscoverUsersAsync();
        _ = InitializeSignalRAsync();
        _ = LoadUnreadNotificationCountAsync();
        _ = LoadLikesReceivedCountAsync();
    }

    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        await LoadDiscoverUsersAsync();
    }

    private async Task LoadDiscoverUsersAsync()
    {
        try
        {
            CurrentUserName = "Đang tìm kiếm...";
            CurrentUserImage = "https://via.placeholder.com/600x800/FFF5F8/E6005C?text=Loading...";
            
            var query = new List<string>();
            if (int.TryParse(FilterAgeMin, out int ageMin)) query.Add($"ageMin={ageMin}");
            if (int.TryParse(FilterAgeMax, out int ageMax)) query.Add($"ageMax={ageMax}");
            if (int.TryParse(FilterMaxDistance, out int maxDistance)) query.Add($"maxDistance={maxDistance}");
            
            if (FilterGender == "Nam") query.Add("gender=0");
            else if (FilterGender == "Nữ") query.Add("gender=1");
            else if (FilterGender == "Khác") query.Add("gender=2");

            if (FilterVerifiedOnly) query.Add("verifiedOnly=true");
            if (FilterOnlineOnly) query.Add("onlineOnly=true");

            string queryString = query.Count > 0 ? "?" + string.Join("&", query) : "";
            var response = await _httpClient.GetFromJsonAsync<DiscoverResponse>($"/api/User/discover{queryString}");
            
            if (response != null && response.Data.Count > 0)
            {
                _discoverQueue = response.Data;
                IsDiscoverQueueEmpty = false;
                NextProfile();
            }
            else
            {
                CurrentUserName = "Hết người để khám phá!";
                CurrentUserBio = "Hãy thử lại sau nhé.";
                CurrentUserImage = "https://via.placeholder.com/600x800/FFF5F8/E6005C?text=No+more+profiles"; 
                _currentUserDto = null;
                IsDiscoverQueueEmpty = true;
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
            new DatingApp.Desktop.Messages.NavigationMessage(((App)System.Windows.Application.Current).Services.GetService(typeof(AdminViewModel))!)
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
            IsCurrentUserVerified = _currentUserDto.IsVerified;
            CurrentUserLocation = _currentUserDto.Location ?? string.Empty;
            CurrentUserZodiac = _currentUserDto.Zodiac ?? string.Empty;
            CurrentUserMbti = _currentUserDto.Mbti ?? string.Empty;
            CurrentUserCompatibilityScore = _currentUserDto.CompatibilityScore;
            IsCompatibilityVisible = _currentUserDto.CompatibilityScore > 0;
            IsSuperLikedBy = _currentUserDto.IsSuperLikedBy;
            CurrentUserHeight = _currentUserDto.Height;
            CurrentUserOccupation = _currentUserDto.Occupation ?? string.Empty;
            CurrentUserEducation = _currentUserDto.Education ?? string.Empty;
            CurrentUserSmoking = _currentUserDto.Smoking ?? string.Empty;
            CurrentUserDrinking = _currentUserDto.Drinking ?? string.Empty;
            CurrentUserDistance = _currentUserDto.Distance;
            IsDistanceVisible = _currentUserDto.Distance.HasValue;
            IsDiscoverQueueEmpty = false;
            IsUserDetailVisible = false; // Reset view when next profile loads

            CurrentUserInterests.Clear();
            if (_currentUserDto.Interests != null)
            {
                foreach (var interest in _currentUserDto.Interests)
                {
                    CurrentUserInterests.Add(interest);
                }
            }

            CurrentUserPhotos.Clear();
            if (_currentUserDto.Photos != null && _currentUserDto.Photos.Count > 0)
            {
                foreach (var photo in _currentUserDto.Photos)
                {
                    CurrentUserPhotos.Add(photo);
                }
            }
            else if (!string.IsNullOrEmpty(_currentUserDto.AvatarUrl))
            {
                CurrentUserPhotos.Add(new PhotoDto { Url = _currentUserDto.AvatarUrl, IsMain = true });
            }

        }
        else
        {
            CurrentUserName = "Hết người để khám phá!";
            CurrentUserBio = "Hãy thử lại sau nhé.";
            CurrentUserImage = "https://via.placeholder.com/600x800/FFF5F8/E6005C?text=No+more+profiles";
            _currentUserDto = null;
            IsCurrentUserVerified = false;
            CurrentUserLocation = string.Empty;
            CurrentUserZodiac = string.Empty;
            CurrentUserMbti = string.Empty;
            CurrentUserCompatibilityScore = 0;
            IsCompatibilityVisible = false;
            IsSuperLikedBy = false;
            CurrentUserHeight = null;
            CurrentUserOccupation = string.Empty;
            CurrentUserEducation = string.Empty;
            CurrentUserSmoking = string.Empty;
            CurrentUserDrinking = string.Empty;
            CurrentUserDistance = null;
            IsDistanceVisible = false;
            CurrentUserInterests.Clear();
            CurrentUserPhotos.Clear();
            IsDiscoverQueueEmpty = true;
            IsUserDetailVisible = false;
        }
    }

    [RelayCommand]
    private void CloseMatchPopup()
    {
        IsMatchPopupVisible = false;
    }

    [RelayCommand]
    private void OpenUserDetail()
    {
        if (_currentUserDto != null)
            IsUserDetailVisible = true;
    }

    [RelayCommand]
    private void CloseUserDetail()
    {
        IsUserDetailVisible = false;
    }

    [RelayCommand]
    private async Task ResetSwipesAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync("/api/Swipe/reset", null);
            if (response.IsSuccessStatusCode)
            {
                IsDiscoverQueueEmpty = false;
                await LoadDiscoverUsersAsync();
                System.Windows.MessageBox.Show("Đã làm mới lượt vuốt của bạn! Hãy bắt đầu khám phá lại nhé.", "Thành công", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            else
            {
                System.Windows.MessageBox.Show("Có lỗi xảy ra khi làm mới lượt vuốt.", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show("Lỗi kết nối: " + ex.Message, "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ToggleEmojiPopup()
    {
        IsEmojiPopupOpen = !IsEmojiPopupOpen;
    }

    [RelayCommand]
    private void AddEmoji(string emoji)
    {
        if (emoji == null) return;
        MessageDraft += emoji;
        IsEmojiPopupOpen = false;
    }

    // --- TABS & PROFILE COMMANDS ---

    [RelayCommand]
    private void ShowDiscover()
    {
        IsDiscoverVisible = true;
        IsProfileVisible = false;
        IsMessagesVisible = false;
        IsLikesVisible = false;
        DiscoverTabColor = "#E6005C";
        ProfileTabColor = "#6B7280";
        MessagesTabColor = "#6B7280";
        LikesTabColor = "#6B7280";
    }

    [RelayCommand]
    private void ShowMessages()
    {
        IsDiscoverVisible = false;
        IsProfileVisible = false;
        IsMessagesVisible = true;
        IsLikesVisible = false;
        DiscoverTabColor = "#6B7280";
        ProfileTabColor = "#6B7280";
        MessagesTabColor = "#E6005C";
        LikesTabColor = "#6B7280";

        _ = LoadMatchesAsync();
    }

    [RelayCommand]
    private void ShowLikes()
    {
        IsDiscoverVisible = false;
        IsProfileVisible = false;
        IsMessagesVisible = false;
        IsLikesVisible = true;
        DiscoverTabColor = "#6B7280";
        ProfileTabColor = "#6B7280";
        MessagesTabColor = "#6B7280";
        LikesTabColor = "#E6005C";

        _ = LoadLikesReceivedAsync();
    }

    [RelayCommand]
    private async Task ShowProfileAsync()
    {
        IsDiscoverVisible = false;
        IsProfileVisible = true;
        IsMessagesVisible = false;
        IsLikesVisible = false;
        DiscoverTabColor = "#6B7280";
        ProfileTabColor = "#E6005C";
        MessagesTabColor = "#6B7280";
        LikesTabColor = "#6B7280";

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

                ProfilePhotos.Clear();
                if (profile.Photos != null)
                {
                    foreach (var photo in profile.Photos)
                    {
                        ProfilePhotos.Add(photo);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Lỗi tải hồ sơ cá nhân: {ex.Message}", "Lỗi tải hồ sơ");
        }
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
            var response = await _httpClient.PutAsJsonAsync("/api/User/profile", dto);
            if (response.IsSuccessStatusCode)
            {
                System.Windows.MessageBox.Show("Cập nhật hồ sơ thành công!", "Thành công");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                System.Windows.MessageBox.Show($"Cập nhật hồ sơ thất bại: {error}", "Lỗi");
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Lỗi kết nối khi cập nhật hồ sơ: {ex.Message}", "Lỗi");
        }
    }

    [RelayCommand]
    private async Task UploadAvatarAsync()
    {
        await UploadPhotoImplAsync(true);
    }

    [RelayCommand]
    private async Task AddPhotoAsync()
    {
        await UploadPhotoImplAsync(false);
    }

    private async Task UploadPhotoImplAsync(bool isAvatar)
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

                var endpoint = isAvatar ? "/api/User/avatar" : "/api/User/photos";
                var response = await _httpClient.PostAsync(endpoint, form);
                
                if (response.IsSuccessStatusCode)
                {
                    if (isAvatar)
                    {
                        var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                        if (result.TryGetProperty("avatarUrl", out var avatarUrlProp))
                        {
                            ProfileAvatarUrl = avatarUrlProp.GetString() ?? ProfileAvatarUrl ?? string.Empty;
                            _authService.CurrentUser!.AvatarUrl = ProfileAvatarUrl;
                        }
                        System.Windows.MessageBox.Show("Cập nhật Avatar thành công!", "Thành công");
                    }
                    else
                    {
                        var newPhoto = await response.Content.ReadFromJsonAsync<PhotoDto>();
                        if (newPhoto != null)
                        {
                            ProfilePhotos.Add(newPhoto);
                        }
                        System.Windows.MessageBox.Show("Thêm ảnh thành công!", "Thành công");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi upload ảnh: " + ex.Message, "Lỗi");
            }
        }
    }

    [RelayCommand]
    private async Task DeletePhotoAsync(PhotoDto photo)
    {
        if (photo == null) return;
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/User/photos/{photo.Id}");
            if (response.IsSuccessStatusCode)
            {
                ProfilePhotos.Remove(photo);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                System.Windows.MessageBox.Show($"Lỗi xóa ảnh: {error}", "Lỗi");
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Lỗi kết nối khi xóa ảnh: {ex.Message}", "Lỗi");
        }
    }

    [RelayCommand]
    private async Task SetMainPhotoAsync(PhotoDto photo)
    {
        if (photo == null) return;
        try
        {
            var response = await _httpClient.PutAsync($"/api/User/photos/{photo.Id}/setMain", null);
            if (response.IsSuccessStatusCode)
            {
                foreach (var p in ProfilePhotos) p.IsMain = false;
                photo.IsMain = true;
                
                ProfileAvatarUrl = photo.Url;
                _authService.CurrentUser!.AvatarUrl = photo.Url;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                System.Windows.MessageBox.Show($"Lỗi đặt ảnh đại diện: {error}", "Lỗi");
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Lỗi kết nối khi đặt ảnh đại diện: {ex.Message}", "Lỗi");
        }
    }

    // --- CHAT & MATCH LOGIC ---

    private async Task InitializeSignalRAsync()
    {
        if (string.IsNullOrEmpty(_authService.CurrentToken)) return;

        var baseUrl = _httpClient.BaseAddress?.ToString().TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl)) return;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/chatHub", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_authService.CurrentToken)!;
            })
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<System.Text.Json.JsonElement>("ReceiveMessage", message =>
        {
            var msgDto = new MessageDto
            {
                Id = message.GetProperty("id").GetGuid(),
                SenderId = message.GetProperty("senderId").GetGuid(),
                ReceiverId = message.GetProperty("receiverId").GetGuid(),
                Content = message.TryGetProperty("content", out var contentProp) && contentProp.ValueKind != System.Text.Json.JsonValueKind.Null ? contentProp.GetString() ?? "" : "",
                ImageUrl = message.TryGetProperty("imageUrl", out var imageProp) && imageProp.ValueKind != System.Text.Json.JsonValueKind.Null ? imageProp.GetString() ?? "" : "",
                IsSeen = message.GetProperty("isSeen").GetBoolean(),
                SentAt = message.GetProperty("sentAt").GetDateTime(),
                IsMine = false
            };

            // Gửi thông báo toast nếu ứng dụng thu nhỏ/không active
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    if (!mainWindow.IsActive || mainWindow.WindowState == WindowState.Minimized)
                    {
                        var senderName = "Ai đó";
                        var match = Matches.FirstOrDefault(m => m.Partner.Id == msgDto.SenderId);
                        if (match != null)
                        {
                            senderName = match.Partner.FullName;
                        }
                        var previewContent = string.IsNullOrEmpty(msgDto.Content) ? "[Hình ảnh]" : msgDto.Content;
                        mainWindow.ShowToastNotification(senderName, previewContent);
                    }
                }
            });

            // Kiểm tra xem tin nhắn có thuộc về cuộc trò chuyện đang mở không
            if (SelectedMatch != null && msgDto.SenderId == SelectedMatch.Partner.Id)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CurrentMessages.Add(msgDto);
                });
                // Đánh dấu đã đọc trên server
                _ = _httpClient.PutAsync($"/api/Messages/seen/{msgDto.SenderId}", null);
            }
            
            _ = LoadMatchesAsync(); // Làm mới danh sách match để hiển thị last message
        });

        _hubConnection.On<System.Text.Json.JsonElement>("MessageDeleted", payload =>
        {
            if (payload.TryGetProperty("messageId", out var idProp))
            {
                var msgId = idProp.GetGuid();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var msg = CurrentMessages.FirstOrDefault(m => m.Id == msgId);
                    if (msg != null)
                    {
                        msg.Content = "Tin nhắn đã bị thu hồi";
                        msg.ImageUrl = null;
                    }
                });
                _ = LoadMatchesAsync();
            }
        });

        _hubConnection.On<System.Text.Json.JsonElement>("MessageEdited", payload =>
        {
            if (payload.TryGetProperty("messageId", out var idProp) && payload.TryGetProperty("content", out var contentProp))
            {
                var msgId = idProp.GetGuid();
                var newContent = contentProp.GetString() ?? "";
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var msg = CurrentMessages.FirstOrDefault(m => m.Id == msgId);
                    if (msg != null)
                    {
                        msg.Content = newContent;
                    }
                });
                _ = LoadMatchesAsync();
            }
        });

        _hubConnection.On<System.Text.Json.JsonElement>("ReceiveNotification", notif =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UnreadNotificationCount++;
                try
                {
                    var newNotif = System.Text.Json.JsonSerializer.Deserialize<NotificationDto>(notif.GetRawText(), new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (newNotif != null)
                    {
                        Notifications.Insert(0, newNotif);
                    }
                }
                catch {}
            });

            if (notif.TryGetProperty("type", out var typeProp))
            {
                var type = typeProp.GetString();
                if (type == "NewMatch")
                {
                    _ = LoadMatchesAsync();
                }
                else if (type == "NewLike")
                {
                    _ = LoadLikesReceivedCountAsync();
                    if (IsLikesVisible)
                    {
                        _ = LoadLikesReceivedAsync();
                    }
                }
            }
        });

        _hubConnection.On<string>("UserOnline", userIdStr =>
        {
            if (Guid.TryParse(userIdStr, out var userId))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var match = Matches.FirstOrDefault(m => m.Partner.Id == userId);
                    if (match != null)
                    {
                        match.Partner.IsOnline = true;
                    }
                });
            }
        });

        _hubConnection.On<string>("UserOffline", userIdStr =>
        {
            if (Guid.TryParse(userIdStr, out var userId))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var match = Matches.FirstOrDefault(m => m.Partner.Id == userId);
                    if (match != null)
                    {
                        match.Partner.IsOnline = false;
                    }
                });
            }
        });

        _hubConnection.On<System.Text.Json.JsonElement>("MessagesSeen", payload =>
        {
            try
            {
                var byUserIdStr = payload.GetProperty("byUserId").GetString();
                if (Guid.TryParse(byUserIdStr, out var byUserId) && SelectedMatch != null && byUserId == SelectedMatch.Partner.Id)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var msg in CurrentMessages.Where(m => m.IsMine))
                        {
                            msg.IsSeen = true;
                        }
                    });
                }
            }
            catch {}
        });

        _hubConnection.On<string>("Typing", senderIdStr =>
        {
            if (Guid.TryParse(senderIdStr, out var senderId) && SelectedMatch != null && senderId == SelectedMatch.Partner.Id)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsPartnerTyping = true;
                    _typingResetTokenSource?.Cancel();
                    _typingResetTokenSource = new System.Threading.CancellationTokenSource();
                    var token = _typingResetTokenSource.Token;
                    Task.Delay(3000, token).ContinueWith(t =>
                    {
                        if (!token.IsCancellationRequested)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                IsPartnerTyping = false;
                            });
                        }
                    }, TaskScheduler.Default);
                });
            }
        });

        _hubConnection.On<System.Text.Json.JsonElement>("ReceiveMessageReaction", payload =>
        {
            try
            {
                var msgId = payload.GetProperty("messageId").GetGuid();
                var reactions = payload.GetProperty("reactions").GetString() ?? "";
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var msg = CurrentMessages.FirstOrDefault(m => m.Id == msgId);
                    if (msg != null)
                    {
                        msg.Reactions = reactions;
                    }
                });
            }
            catch {}
        });

        _hubConnection.On<System.Text.Json.JsonElement>("PartnerUnmatched", payload =>
        {
            try
            {
                var partnerId = payload.GetProperty("partnerId").GetGuid();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (SelectedMatch != null && SelectedMatch.Partner.Id == partnerId)
                    {
                        SelectedMatch = null;
                        CurrentMessages.Clear();
                        System.Windows.MessageBox.Show("Đối phương đã hủy ghép đôi với bạn hoặc cuộc trò chuyện đã đóng.", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                    _ = LoadMatchesAsync();
                });
            }
            catch {}
        });

        _hubConnection.On<System.Text.Json.JsonElement>("PartnerBlocked", payload =>
        {
            try
            {
                var blockerId = payload.GetProperty("blockerId").GetGuid();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (SelectedMatch != null && SelectedMatch.Partner.Id == blockerId)
                    {
                        SelectedMatch = null;
                        CurrentMessages.Clear();
                        System.Windows.MessageBox.Show("Bạn đã bị chặn hoặc cuộc trò chuyện đã đóng.", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    }
                    _ = LoadMatchesAsync();
                });
            }
            catch {}
        });

        _hubConnection.On("OnUserBanned", () =>
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                System.Windows.MessageBox.Show("Tài khoản của bạn đã bị khóa bởi Quản trị viên.", "Tài khoản bị khóa", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                await LogoutAsync();
            });
        });

        try
        {
            await _hubConnection.StartAsync();
            System.Diagnostics.Debug.WriteLine("[DashboardViewModel] SignalR connected successfully.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] SignalR connection failed: {ex.Message}");
            Application.Current.Dispatcher.Invoke(() =>
            {
                System.Windows.MessageBox.Show("Không thể kết nối đến máy chủ thời gian thực. Tin nhắn sẽ không được cập nhật tự động. Vui lòng kiểm tra lại mạng.", "Cảnh báo kết nối");
            });
        }
    }

    private async Task LoadMatchesAsync()
    {
        try
        {
            var list = await _httpClient.GetFromJsonAsync<List<MatchDto>>("/api/Match");
            if (list != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _isReloadingMatches = true;
                    try
                    {
                        var selectedId = SelectedMatch?.Id;

                        Matches.Clear();
                        foreach (var match in list)
                        {
                            Matches.Add(match);
                        }

                        _isReloadingMatches = false;

                        if (selectedId.HasValue)
                        {
                            var newSelection = Matches.FirstOrDefault(m => m.Id == selectedId.Value);
                            SelectedMatch = newSelection;
                        }
                    }
                    finally
                    {
                        _isReloadingMatches = false;
                    }
                });
            }
        }
        catch { }
    }

    private async Task LoadMessagesAsync(Guid partnerId)
    {
        try
        {
            var messages = await _httpClient.GetFromJsonAsync<List<MessageDto>>($"/api/Messages/{partnerId}");
            if (messages != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CurrentMessages.Clear();
                    foreach (var msg in messages)
                    {
                        msg.IsMine = (msg.SenderId.ToString() == _authService.CurrentUser?.Id);
                        CurrentMessages.Add(msg);
                    }
                });
                
                // Đánh dấu đã đọc
                _ = _httpClient.PutAsync($"/api/Messages/seen/{partnerId}", null);
                _ = LoadMatchesAsync(); // Update unread count
            }
        }
        catch { }
    }

    [RelayCommand]
    private void SelectMatch(MatchDto match)
    {
        SelectedMatch = match;
        ShowMessages();
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(MessageDraft) || SelectedMatch == null) return;

        var draft = MessageDraft;
        MessageDraft = "";

        if (EditingMessage != null)
        {
            // Edit Mode
            var msgId = EditingMessage.Id;
            var dto = new { Content = draft };
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"/api/Messages/{msgId}", dto);
                if (response.IsSuccessStatusCode)
                {
                    EditingMessage.Content = draft;
                    EditingMessage = null;
                    _ = LoadMatchesAsync();
                }
                else
                {
                    MessageDraft = draft; // Revert
                }
            }
            catch
            {
                MessageDraft = draft; // Revert
            }
        }
        else
        {
            // Send Mode
            var dto = new { ReceiverId = SelectedMatch.Partner.Id, Content = draft };
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Messages", dto);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                    var msgDto = new MessageDto
                    {
                        Id = result.GetProperty("id").GetGuid(),
                        SenderId = result.GetProperty("senderId").GetGuid(),
                        ReceiverId = result.GetProperty("receiverId").GetGuid(),
                        Content = result.GetProperty("content").GetString() ?? "",
                        IsSeen = result.GetProperty("isSeen").GetBoolean(),
                        SentAt = result.GetProperty("sentAt").GetDateTime(),
                        IsMine = true
                    };

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CurrentMessages.Add(msgDto);
                    });
                    
                    _ = LoadMatchesAsync();
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        System.Windows.MessageBox.Show("Không thể gửi tin nhắn. Người dùng này đã hủy ghép đôi hoặc chặn tài khoản.", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        SelectedMatch = null;
                        CurrentMessages.Clear();
                        _ = LoadMatchesAsync();
                    }
                    else
                    {
                        MessageDraft = draft; // Revert
                    }
                }
            }
            catch
            {
                MessageDraft = draft; // Revert
            }
        }
    }

    [RelayCommand]
    private async Task SendImageAsync()
    {
        if (SelectedMatch == null) return;

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
                form.Add(new StringContent(SelectedMatch.Partner.Id.ToString()), "receiverId");

                var response = await _httpClient.PostAsync("/api/Messages/image", form);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                    var msgDto = new MessageDto
                    {
                        Id = result.GetProperty("id").GetGuid(),
                        SenderId = result.GetProperty("senderId").GetGuid(),
                        ReceiverId = result.GetProperty("receiverId").GetGuid(),
                        Content = result.TryGetProperty("content", out var contentProp) && contentProp.ValueKind != System.Text.Json.JsonValueKind.Null ? contentProp.GetString() ?? "" : "",
                        ImageUrl = result.TryGetProperty("imageUrl", out var imageProp) && imageProp.ValueKind != System.Text.Json.JsonValueKind.Null ? imageProp.GetString() ?? "" : "",
                        IsSeen = result.GetProperty("isSeen").GetBoolean(),
                        SentAt = result.GetProperty("sentAt").GetDateTime(),
                        IsMine = true
                    };

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CurrentMessages.Add(msgDto);
                    });
                    
                    _ = LoadMatchesAsync();
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        System.Windows.MessageBox.Show("Không thể gửi ảnh. Người dùng này đã hủy ghép đôi hoặc chặn tài khoản.", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        SelectedMatch = null;
                        CurrentMessages.Clear();
                        _ = LoadMatchesAsync();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Lỗi khi gửi ảnh.", "Lỗi");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi gửi ảnh: " + ex.Message, "Lỗi");
            }
        }
    }

    [RelayCommand]
    private async Task UnmatchAsync()
    {
        if (SelectedMatch == null) return;
        
        var confirm = System.Windows.MessageBox.Show($"Bạn có chắc chắn muốn huỷ ghép đôi với {SelectedMatch.Partner.FullName} không?", "Xác nhận", System.Windows.MessageBoxButton.YesNo);
        if (confirm != System.Windows.MessageBoxResult.Yes) return;

        try
        {
            var response = await _httpClient.DeleteAsync($"/api/Match/{SelectedMatch.Id}");
            if (response.IsSuccessStatusCode)
            {
                Matches.Remove(SelectedMatch);
                SelectedMatch = null;
                CurrentMessages.Clear();
            }
        }
        catch { }
    }

    [RelayCommand]
    private async Task BlockAsync()
    {
        if (SelectedMatch == null) return;
        
        var confirm = System.Windows.MessageBox.Show($"Bạn có chắc chắn muốn chặn {SelectedMatch.Partner.FullName} không? Bạn sẽ không thấy họ nữa.", "Cảnh báo", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);
        if (confirm != System.Windows.MessageBoxResult.Yes) return;

        try
        {
            var response = await _httpClient.PostAsync($"/api/Match/block/{SelectedMatch.Partner.Id}", null);
            if (response.IsSuccessStatusCode)
            {
                Matches.Remove(SelectedMatch);
                SelectedMatch = null;
                CurrentMessages.Clear();
            }
        }
        catch { }
    }

    [RelayCommand]
    private async Task RecallMessageAsync(MessageDto msg)
    {
        if (msg == null) return;
        
        var confirm = System.Windows.MessageBox.Show("Bạn có chắc chắn muốn thu hồi tin nhắn này không?", "Xác nhận", System.Windows.MessageBoxButton.YesNo);
        if (confirm != System.Windows.MessageBoxResult.Yes) return;

        try
        {
            var response = await _httpClient.DeleteAsync($"/api/Messages/{msg.Id}");
            if (response.IsSuccessStatusCode)
            {
                msg.Content = "Tin nhắn đã bị thu hồi";
                msg.ImageUrl = null;
                _ = LoadMatchesAsync();
            }
        }
        catch { }
    }

    [RelayCommand]
    private void StartEditMessage(MessageDto msg)
    {
        if (msg == null) return;
        EditingMessage = msg;
        MessageDraft = msg.Content;
    }

    [RelayCommand]
    private async Task ReactLikeAsync(MessageDto msg) => await ReactToMessageAsync(msg, "👍");

    [RelayCommand]
    private async Task ReactLoveAsync(MessageDto msg) => await ReactToMessageAsync(msg, "❤️");

    [RelayCommand]
    private async Task ReactLaughAsync(MessageDto msg) => await ReactToMessageAsync(msg, "😂");

    [RelayCommand]
    private async Task ReactSadAsync(MessageDto msg) => await ReactToMessageAsync(msg, "😢");

    [RelayCommand]
    private async Task ReactAngryAsync(MessageDto msg) => await ReactToMessageAsync(msg, "😠");

    private async Task ReactToMessageAsync(MessageDto msg, string emoji)
    {
        if (msg == null) return;
        try
        {
            var dto = new { Reaction = emoji };
            var response = await _httpClient.PostAsJsonAsync($"/api/Messages/{msg.Id}/react", dto);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                var reactions = result.GetProperty("reactions").GetString() ?? "";
                Application.Current.Dispatcher.Invoke(() =>
                {
                    msg.Reactions = reactions;
                });
            }
        }
        catch {}
    }

    [RelayCommand]
    private void CancelEditMessage()
    {
        EditingMessage = null;
        MessageDraft = "";
    }

    private async Task LoadUnreadNotificationCountAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<NotificationResponse>("/api/Notification");
            if (response != null)
            {
                UnreadNotificationCount = response.UnreadCount;
            }
        }
        catch {}
    }

    [RelayCommand]
    private async Task ToggleNotificationPanelAsync()
    {
        IsNotificationPanelOpen = !IsNotificationPanelOpen;
        if (IsNotificationPanelOpen)
        {
            await LoadNotificationsAsync();
        }
    }

    [RelayCommand]
    private async Task LoadNotificationsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<NotificationResponse>("/api/Notification");
            if (response != null)
            {
                Notifications = new ObservableCollection<NotificationDto>(response.Notifications);
                UnreadNotificationCount = response.UnreadCount;
            }
        }
        catch {}
    }

    [RelayCommand]
    private async Task MarkAllNotificationsAsReadAsync()
    {
        try
        {
            var response = await _httpClient.PutAsync("/api/Notification/read-all", null);
            if (response.IsSuccessStatusCode)
            {
                foreach (var notif in Notifications)
                {
                    notif.IsRead = true;
                }
                Notifications = new ObservableCollection<NotificationDto>(Notifications);
                UnreadNotificationCount = 0;
            }
        }
        catch {}
    }

    [RelayCommand]
    private async Task ClickNotificationAsync(NotificationDto notif)
    {
        if (notif == null) return;

        try
        {
            if (!notif.IsRead)
            {
                var response = await _httpClient.PutAsync($"/api/Notification/{notif.Id}/read", null);
                if (response.IsSuccessStatusCode)
                {
                    notif.IsRead = true;
                    UnreadNotificationCount = Math.Max(0, UnreadNotificationCount - 1);
                }
            }

            IsNotificationPanelOpen = false;

            if (notif.Type == "NewMatch" && notif.RelatedUserId.HasValue)
            {
                ShowMessages();

                var partnerId = notif.RelatedUserId.Value;
                var match = Matches.FirstOrDefault(m => m.Partner.Id == partnerId);
                if (match != null)
                {
                    SelectedMatch = match;
                }
                else
                {
                    await LoadMatchesAsync();
                    match = Matches.FirstOrDefault(m => m.Partner.Id == partnerId);
                    if (match != null)
                    {
                        SelectedMatch = match;
                    }
                }
            }
        }
        catch {}
    }

    public async Task LoadLikesReceivedAsync()
    {
        try
        {
            var likes = await _httpClient.GetFromJsonAsync<List<DiscoverUserDto>>("/api/Swipe/likes");
            if (likes != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LikesReceived.Clear();
                    foreach (var like in likes)
                    {
                        LikesReceived.Add(like);
                    }
                    LikesReceivedCount = LikesReceived.Count;
                });
            }
        }
        catch { }
    }

    private async Task LoadLikesReceivedCountAsync()
    {
        try
        {
            var likes = await _httpClient.GetFromJsonAsync<List<DiscoverUserDto>>("/api/Swipe/likes");
            if (likes != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LikesReceivedCount = likes.Count;
                });
            }
        }
        catch { }
    }

    [RelayCommand]
    private async Task AcceptLikeAsync(DiscoverUserDto user)
    {
        if (user == null) return;
        try
        {
            var dto = new { ToUserId = user.Id, IsLike = true };
            var response = await _httpClient.PostAsJsonAsync("/api/Swipe", dto);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                if (result.TryGetProperty("isMatch", out var isMatchProp) && isMatchProp.GetBoolean())
                {
                    IsMatchPopupVisible = true;
                    _ = LoadMatchesAsync();
                }
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LikesReceived.Remove(user);
                    LikesReceivedCount = LikesReceived.Count;
                });
            }
        }
        catch {}
    }

    [RelayCommand]
    private async Task PassLikeAsync(DiscoverUserDto user)
    {
        if (user == null) return;
        try
        {
            var dto = new { ToUserId = user.Id, IsLike = false };
            var response = await _httpClient.PostAsJsonAsync("/api/Swipe", dto);
            if (response.IsSuccessStatusCode)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LikesReceived.Remove(user);
                    LikesReceivedCount = LikesReceived.Count;
                });
            }
        }
        catch {}
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var loggedOutEmail = _authService.CurrentUser?.Email;

        if (_hubConnection != null)
        {
            try
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
            }
            catch {}
            _hubConnection = null;
        }

        _authService.Logout();

        var app = (App)System.Windows.Application.Current;
        if (app.Services.GetService(typeof(LoginViewModel)) is LoginViewModel loginVm)
        {
            if (!string.IsNullOrEmpty(loggedOutEmail))
            {
                loginVm.Email = loggedOutEmail;
            }
            loginVm.Password = "";
            loginVm.ErrorMessage = "";

            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(
                new DatingApp.Desktop.Messages.NavigationMessage(loginVm)
            );
        }
    }

}

